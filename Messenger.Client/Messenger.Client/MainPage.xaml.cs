using Messenger.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Messenger.Common;
using Messenger.Entities.DTOs;
using Messenger.Entities.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Xamarin.Forms;

namespace Messenger.Client
{
    public partial class MainPage : ContentPage
    {
        private readonly string keyFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"/keys/";

        private AuthObject _auth { get; set; }
        private ApiClient _apiClient { get; set; }
        private int? SelectedDialogId { get; set; }

        public delegate void ConnectionStatusHandler();
        public event ConnectionStatusHandler Connected = delegate { };
        public event ConnectionStatusHandler Disonnected = delegate { };

        public ObservableCollection<DialogPanel> DialogPanels = new ObservableCollection<DialogPanel>();
        public ObservableCollection<MessagePanel> MessagesPanel = new ObservableCollection<MessagePanel>();

        //public StackLayout MessagesStackPanel;
        public Editor MessageText;

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

        public MainPage()
        {
            InitializeComponent();

            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = (senderX, certificate, chain, sslPolicyErrors) =>
            {
                return true;
            };
            
            SetMainPage();
        }

        private bool returnedToMainPage = false;

        protected override bool OnBackButtonPressed()
        {
            returnedToMainPage = true;

            SetMainPage();

            return true;
        }

        private void SetDialogPage()
        {
            MessageText = new Editor {HorizontalOptions = LayoutOptions.FillAndExpand};
           
            MessagesPanel = new ObservableCollection<MessagePanel>();

            var messagesListView = new ListView()
            {
                ItemsSource = MessagesPanel,
                HasUnevenRows = true,
                VerticalOptions = LayoutOptions.FillAndExpand,
                ItemTemplate = new DataTemplate(() =>
                {
                    var messageText = new Label { Margin = new Thickness(5), TextColor = Color.Black };
                    messageText.SetBinding(Label.TextProperty, "MessageText");
                    messageText.SetBinding(Label.HorizontalTextAlignmentProperty, "MessagePanelHorizontalAlignment");
                    messageText.SetBinding(Label.MarginProperty, "MessagePanelMargin");

                    var dateText = new Label { TextColor = Color.Gray, HorizontalTextAlignment = TextAlignment.End };
                    dateText.SetBinding(Label.TextProperty, "SentString");
                    dateText.SetBinding(Label.HorizontalTextAlignmentProperty, "MessagePanelHorizontalAlignment");
                    dateText.SetBinding(Label.MarginProperty, "MessagePanelMargin");

                    var sl = new StackLayout
                    {
                        Padding = new Thickness(5),
                        Orientation = StackOrientation.Vertical,
                        BackgroundColor = Color.WhiteSmoke,
                        Children = { messageText, dateText }
                    };

                    return new ViewCell()
                    {
                        View = sl
                    };
                })
            };

            var sendButton = new Button {Text = "Send", HorizontalOptions = LayoutOptions.End};

            sendButton.Clicked += async (sender, e) => { await Send(); };
            
            var stackLayout = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                Children =
                {
                    messagesListView,
                    new StackLayout {Orientation = StackOrientation.Horizontal, Children = {MessageText, sendButton}}
                }
            };

            this.Content = stackLayout;
        }

        private void SetAddDialogPage(object s, EventArgs ea)
        {
            var okButton = new Button {Text = "Ok"};
            var cancelButton = new Button { Text = "Cancel" };
            var email = new Editor {HorizontalOptions = LayoutOptions.FillAndExpand};

            var layout = new StackLayout
            {
                Padding = new Thickness(10),
                Children =
                {
                    email,
                    new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal, Children = {okButton, cancelButton},
                        HorizontalOptions = LayoutOptions.End
                    }
                }
            };

            okButton.Clicked += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(email.Text)) return;

                AddDialog(email.Text);

                returnedToMainPage = true;
                SetMainPage();
            };
            cancelButton.Clicked += (sender, e) =>
            {
                returnedToMainPage = true;
                SetMainPage();
            };

            this.Content = layout;
        }

        private void SetMainPage()
        {
            ListView dialogsListView = new ListView
            {
                IsEnabled = returnedToMainPage || false,

                HasUnevenRows = true,
                // Определяем источник данных
                ItemsSource = DialogPanels,

                // Определяем формат отображения данных
                ItemTemplate = new DataTemplate(() =>
                {
                    var image = new Image {Aspect = Aspect.AspectFit, WidthRequest = 64, HeightRequest = 64};
                    image.SetBinding(Image.SourceProperty, "ImageUrl");

                    var label = new Label {Margin = new Thickness(10)};
                    label.SetBinding(Label.TextProperty, "Interlocutor.FullName");

                    var view = new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Children = {image, label}
                    };

                    return new ViewCell
                    {
                        View = view
                    };
                })
            };

            dialogsListView.ItemTapped += async (sender, e) =>
            {
                ListView lv = (ListView) sender;

                if (lv.SelectedItem != null)
                {
                    var dialog = (DialogPanel) lv.SelectedItem;

                    if (dialog.Status == DialogStatus.Active)
                    {
                        SelectedDialogId = dialog.Dialog.Id;

                        SetDialogPage();

                        if (dialog.Info != null)
                        {
                            await RefresMessages();
                        }
                    }
                }
                else
                {
                    SelectedDialogId = null;
                }
            };

            var addButton = new Button { Text = "Add Dialog", IsEnabled = returnedToMainPage || false };

            addButton.Clicked += SetAddDialogPage;

            Connected += () =>
            {
                dialogsListView.IsEnabled =  addButton.IsEnabled = true;
            };

            Disonnected += () =>
            {
                dialogsListView.IsEnabled = addButton.IsEnabled = false;
            };

            this.Content = new StackLayout { Children = { dialogsListView, addButton } };
        }

        public async Task Send()
        {
            var selectedDialogPanel = SelectedDialogPanel;

            if (selectedDialogPanel == null)
                return;

            var dialogInfo = selectedDialogPanel.Info;

            var text = MessageText.Text.Trim();

            if (string.IsNullOrWhiteSpace(text)) return;

            var messageKey = Common.Common.CreateAesKey();

            var encryptedData = Common.Common.EncryptAES(text, messageKey);

            var ownerEncKey = Common.Common.EncryptRSA(messageKey, dialogInfo.MyPublicKey);
            var recipientEncKey = Common.Common.EncryptRSA(messageKey, dialogInfo.HisPublicKey);

            await connection.InvokeAsync("SendMessage", dialogInfo.Id, encryptedData, ownerEncKey, recipientEncKey);

            MessageText.Text = string.Empty;
        }

        private async Task Login()
        {
            while (_auth == null)
            {
                try
                {
                    var authWindow = new {Email = "user3@mail.com", Password = "pwd"};

                    if (string.IsNullOrWhiteSpace(authWindow.Email) ||
                        string.IsNullOrWhiteSpace(authWindow.Password))
                        continue;

                    var auth = await Common.Common.GetAuthObject(authWindow.Email, authWindow.Password);

                    if (!auth.Success)
                    {
                        throw new ApplicationException(auth.ErrorText);
                    }

                    _auth = auth;
                    _apiClient = new ApiClient(auth.Token);
                }
                catch (ApplicationException e)
                {
                    await DisplayAlert("Something went wrong", e.Message, "Ok");
                }
                catch (Exception e)
                {
                }
            }
        }

        private async Task RefresMessages()
        {
            //MessagesPanel.Clear();

            var response =
                await _apiClient.CallApi<ApiCallResult<List<MessageDTO>>>("message", "list/" + SelectedDialogId.Value);

            if (response.Status != HttpStatusCode.OK)
                throw new ApplicationException(response.Details);

            var dialog = DialogPanels.First(y => y.Info.Id.Equals(SelectedDialogId.Value));

            var dialogInfo = dialog.Info;

            foreach (var item in response.Data.Select(x => CreateMessagePanel(dialogInfo, x)))
            {
                MessagesPanel.Add(item);
            }

            dialog.HasUnreadMessages = false;
        }

        private async Task Connect()
        {
            try
            {
                connection = new HubConnectionBuilder()
                    .WithUrl(Common.Common.Host + Common.Common.Hub,
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
                            if (SelectedDialogId == message.DialogId)
                            {
                                var dialogInfo = DialogPanels.First(y => y.Info.Id.Equals(message.DialogId)).Info;

                                MessagesPanel.Add(CreateMessagePanel(dialogInfo, message));
                            }
                            else if (!message.OwnerId.Equals(_auth.User.Id))
                            {
                                DialogPanels.First(x => x.Dialog.Id.Equals(message.DialogId)).HasUnreadMessages = true;
                            }
                    });

                await connection.StartAsync();

                Connected();
            }
            catch (Exception e)
            {
                await DisplayAlert("Something went wrong", e.Message, "Ok");
            }
        }

        private MessagePanel CreateMessagePanel(DialogInfo dialogInfo, MessageDTO message)
        {
            var isMine = message.OwnerId.Equals(_auth.User.Id);

            var messageKey = Common.Common.DecryptRSA(isMine ? message.OwnerRcaEncKey : message.RecipientRcaEncKey,
                dialogInfo.PrivateKey);

            var decryptedText = Common.Common.DecryptAES(message.Body, messageKey);

            return new MessagePanel(message, decryptedText, isMine, message.Date);
        }

        private async Task RefreshDialogs()
        {
            var response =
                await _apiClient.CallApi<ApiCallResult<List<DialogDTO>>>("dialog", "list/" + _auth.User.Id);

            if (response.Status != HttpStatusCode.OK) throw new ApplicationException(response.Details);

            foreach (var item in response.Data.OrderByDescending(x => x.LastActivityUtc))
            {
                var dialog = Dialogs.FirstOrDefault(x => x.Id == item.Id);

                if (dialog == null)
                {
                    var dialogInfo = Common.Common.GetDialogInfo(item.Id, keyFolder, _auth);

                    if (item.Status == DialogStatus.Active && dialogInfo == null)
                    {
                        // do nothing
                    }
                    else
                    {
                        var dialogPanel = new DialogPanel(_auth, item)
                        {
                            Info = dialogInfo
                        };

                        DialogPanels.Add(dialogPanel);
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
                var keys = Common.Common.CreateRSAKeys();

                dialog.Info = new DialogInfo(dialogId, keys.Item1, keys.Item2);
            }

            dialog.Info.HisPublicKeyString = publicKey;

            if (dialog.Info.IsCompleted)
            {
                Common.Common.SaveDialogInfo(dialog.Info, keyFolder, _auth);
            }

            if (!keyExchangeCompleted)
            {
                await connection.InvokeAsync("KeyExchange", dialogId, dialog.Info.MyPublicKeyString);
            }

            if (dialog.Info.IsCompleted)
            {
                dialog.Status = DialogStatus.Active;
            }
        }


        private async void MainPage_OnAppearing(object sender, EventArgs e)
        {
            try
            {
                await Login();
                await RefreshDialogs();
                await Connect();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Something went wrong", ex.Message, "Ok");
            }
        }

        private async void AddDialog(string email)
        {
            var dialog = await _apiClient.GetDialog(email);

            if (dialog.Status == HttpStatusCode.OK)
            {
                if (!Dialogs.Any(x => x.Id == dialog.Data.Id))
                {
                    DialogPanels.Add(new DialogPanel(_auth, dialog.Data));

                    await connection.InvokeAsync("OnDialogCreated");
                }
            }
            else
            {
                await DisplayAlert("Something went wrong", dialog.Details, "Ok");
            }
        }
    }
}
