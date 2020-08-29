using Messenger.Common.DTOs;
using MessengerWinClient.Controls;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Messenger.Common;
using Newtonsoft.Json;
using Exception = System.Exception;
using System.Collections.ObjectModel;
using Messenger.Entities.DTOs;
using Messenger.Entities.Models;

namespace MessengerWinClient
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string keyFolder = @"keys";

        private AuthObject _auth { get; set; }
        private ApiClient _apiClient { get; set; }
        private int? SelectedDialogId { get; set; }

        public delegate void ConnectionStatusHandler();
        public event ConnectionStatusHandler Connected = delegate { };
        public event ConnectionStatusHandler Disonnected = delegate { };

        public ObservableCollection<DialogPanel> DialogPanels = new ObservableCollection<DialogPanel>();

        HubConnection connection;
        
        private IEnumerable<DialogDTO> Dialogs
        {
            get { return DialogPanels.Select(x => x.Dialog); }
        }

        private DialogPanel SelectedDialogPanel
        {
            get
            {
                return SelectedDialogId != null
                    ? DialogPanels.First(x => x.Dialog.Id.Equals(SelectedDialogId))
                    : null;
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) => { return true; };

            DataContext = this;

            DialogListView.ItemsSource = DialogPanels;

            Connected += () =>
            {
                DialogListView.IsEnabled = MessagesStackPanel.IsEnabled =
                    MessageText.IsEnabled = AddDialogButton.IsEnabled = SendButton.IsEnabled = true;
            };

            Disonnected += () =>
            {
                DialogListView.IsEnabled = MessagesStackPanel.IsEnabled =
                    MessageText.IsEnabled = AddDialogButton.IsEnabled = SendButton.IsEnabled = false;
            };
        }

        private async Task Login()
        {
            while (_auth == null)
            {
                try
                {
                    AuthWindow authWindow = new AuthWindow();

                    if (authWindow.ShowDialog() == true)
                    {
                        if (string.IsNullOrWhiteSpace(authWindow.Email) ||
                            string.IsNullOrWhiteSpace(authWindow.Password))
                            continue;

                        var auth = await Common.GetAuthObject(authWindow.Email, authWindow.Password);

                        if (!auth.Success)
                        {
                            throw new ApplicationException(auth.ErrorText);
                        }

                        _auth = auth;
                        _apiClient = new ApiClient(auth.Token);
                    }
                    else
                    {
                        Environment.Exit(0);
                    }
                }
                catch (ApplicationException e)
                {
                    MessageBox.Show(e.Message, "Something went wrong", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception)
                {
                }
            }            
        }

        private async Task Connect()
        {
            try
            {
                connection = new HubConnectionBuilder()
                    .WithUrl(Common.Host + Common.Hub,
                        options => { options.AccessTokenProvider = () => Task.FromResult(_auth.Token); })
                    .Build();

                connection.Closed += async (error) =>
                {
                    Disonnected();

                    await Task.Delay(new Random().Next(0, 5) * 1000);
                    await connection.StartAsync();

                    Connected();
                };

                connection.On<int, string, bool>("InitKeyExchange",
                    async (dialogId, publicKey, keyExchangeCompleted) =>
                    {
                        await InitKeyExchange(dialogId, publicKey, keyExchangeCompleted);
                    });

                connection.On<MessageDTO>("Notify",
                    (message) =>
                    {
                        Dispatcher.Invoke(() =>
                        {
                            if (SelectedDialogId == message.DialogId)
                            {
                                var dialogInfo = DialogPanels.First(y => y.Info.Id.Equals(message.DialogId)).Info;

                                MessagesStackPanel.Children.Add(CreateMessagePanel(dialogInfo, message));
                            }
                            else if (!message.OwnerId.Equals(_auth.User.Id))
                            {
                                DialogPanels.First(x => x.Dialog.Id.Equals(message.DialogId)).HasUnreadMessages = true;
                            }
                        });
                    });

                await connection.StartAsync();

                Connected();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Something went wrong", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private MessagePanel CreateMessagePanel(DialogInfo dialogInfo, MessageDTO message)
        {
            var isMine = message.OwnerId.Equals(_auth.User.Id);

            var messageKey = Common.DecryptRSA(isMine ? message.OwnerRcaEncKey : message.RecipientRcaEncKey,
                dialogInfo.PrivateKey);

            var decryptedText = Common.DecryptAES(message.Body, messageKey);

            return new MessagePanel(message, decryptedText, isMine, message.Date);
        }

        public async Task Send()
        {
            var selectedDialogPanel = SelectedDialogPanel;

            if (selectedDialogPanel == null)
                return;

            var dialogInfo = selectedDialogPanel.Info;
            
            var text = new TextRange(MessageText.Document.ContentStart, MessageText.Document.ContentEnd).Text.Trim();

            if (string.IsNullOrWhiteSpace(text)) return;

            var messageKey = Common.CreateAesKey();

            var encryptedData = Common.EncryptAES(text, messageKey);

            var ownerEncKey = Common.EncryptRSA(messageKey, dialogInfo.MyPublicKey);
            var recipientEncKey = Common.EncryptRSA(messageKey, dialogInfo.HisPublicKey);

            await connection.InvokeAsync("SendMessage", dialogInfo.Id, encryptedData, ownerEncKey, recipientEncKey);

            MessageText.Document.Blocks.Clear();
        }
        
        private async Task RefreshDialogs()
        {
            var response =
                await _apiClient.CallApi<ApiCallResult<List<DialogDTO>>>("dialog", "list/" + _auth.User.Id);

            if (response.Status != HttpStatusCode.OK) throw new ApplicationException(response.Details);

            foreach (var item in response.Data.OrderByDescending(x=>x.LastActivityUtc))
            {
                var dialog = Dialogs.FirstOrDefault(x => x.Id == item.Id);

                if (dialog == null)
                {
                    var dialogInfo = Common.GetDialogInfo(item.Id, keyFolder, _auth);

                    if (item.Status == DialogStatus.Active && dialogInfo == null)
                    {
                        // do nothing
                    }
                    else
                    {
                        await Dispatcher.InvokeAsync(() =>
                        {
                            var dialogPanel = new DialogPanel(_auth, item)
                            {
                                Info = dialogInfo
                            };

                            DialogPanels.Add(dialogPanel);
                        });
                    }
                }
                else
                {
                    DialogPanels.First(x => x.Dialog.Id == dialog.Id).Status = item.Status;
                    dialog.Participants = item.Participants;
                }
            }

            SelectedDialogId = null;
        }

        private async Task RefreshMessages()
        {
            MessagesStackPanel.Children.Clear();

            var response =
                await _apiClient.CallApi<ApiCallResult<List<MessageDTO>>>("message", "list/" + SelectedDialogId.Value);

            if (response.Status != HttpStatusCode.OK)
                throw new ApplicationException(response.Details);

            var dialog = DialogPanels.First(y => y.Info.Id.Equals(SelectedDialogId.Value));

            var dialogInfo = dialog.Info;

            foreach (var item in response.Data)
            {
                MessagesStackPanel.Children.Add(CreateMessagePanel(dialogInfo, item));
            }

            dialog.HasUnreadMessages = false;
        }

        private async Task InitKeyExchange(int dialogId, string publicKey, bool keyExchangeCompleted)
        {
            var dialog = DialogPanels.FirstOrDefault(x => x.Dialog.Id.Equals(dialogId));

            if (dialog == null)
            {
                await RefreshDialogs();

                dialog = DialogPanels.FirstOrDefault(x => x.Dialog.Id.Equals(dialogId));

                if (dialog == null)
                    throw new ApplicationException("Dialog not found");
            }

            if (dialog.Info == null)
            {
                var keys = Common.CreateRSAKeys();

                dialog.Info = new DialogInfo(dialogId, keys.Item1, keys.Item2);
            }

            dialog.Info.HisPublicKeyString = publicKey;

            if (dialog.Info.IsCompleted)
            {
                Common.SaveDialogInfo(dialog.Info, keyFolder, _auth);
            }

            if (!keyExchangeCompleted)
            {
                await connection.InvokeAsync("KeyExchange", dialogId, dialog.Info.MyPublicKeyString);
            }

            if (dialog.Info.IsCompleted)
            {
                await Dispatcher.InvokeAsync(() => dialog.Status = DialogStatus.Active);
            }
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await Login();
                await RefreshDialogs();
                await Connect();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Something went wrong", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AddDialogButton_OnClick(object sender, RoutedEventArgs e)
        {
            var addDialog = new AddDialogWindow();

            if (addDialog.ShowDialog() == true)
            {
                var dialog = await _apiClient.GetDialog(addDialog.Email);
                
                if(dialog.Status == HttpStatusCode.OK)
                {
                    if (!Dialogs.Any(x => x.Id == dialog.Data.Id))
                    {
                        DialogPanels.Add(new DialogPanel(_auth, dialog.Data));

                        await connection.InvokeAsync("OnDialogCreated");
                    }
                }
                else
                {
                    MessageBox.Show(dialog.Details, "Something went wrong", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void SendButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await Send();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private async void DialogListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView lv = (ListView)sender;

            if (lv.SelectedItem != null)
            {
                var dialog = (DialogPanel)lv.SelectedItem;

                if (dialog.Status == DialogStatus.Active)
                {
                    SelectedDialogId = dialog.Dialog.Id;

                    if (dialog.Info != null)
                    {
                        await RefreshMessages();
                    }
                }
            }
            else
            {
                SelectedDialogId = null;
            }
        }

        private Boolean AutoScroll = true;
        private void ScrollViewer_OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;

            if (e.ExtentHeightChange == 0)
            {   
                if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
                {   
                    AutoScroll = true;
                }
                else
                {   
                    AutoScroll = false;
                }
            }

            if (AutoScroll && e.ExtentHeightChange != 0)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.ExtentHeight);
            }
        }
    }
}
