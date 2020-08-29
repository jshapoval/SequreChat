using Messenger.Common.DTOs;
using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Messenger.Entities.Models;
using System.Runtime.CompilerServices;
using Messenger.Common;

namespace MessengerWinClient.Controls
{
    /// <summary>
    /// Логика взаимодействия для DialogPanel.xaml
    /// </summary>
    public partial class DialogPanel : UserControl, INotifyPropertyChanged
    {
        private bool _hasUnreadMessages;

        public event PropertyChangedEventHandler PropertyChanged;
        public DialogDTO Dialog { get; set; }
        public ParticipantDTO Interlocutor { get; set; }
        public DialogInfo Info { get; set; }
        public bool HasUnreadMessages
        {
            get => _hasUnreadMessages;
            set
            {
                _hasUnreadMessages = value;
                OnPropertyChanged("DialogShadowColor");
            }
        }

        public DialogStatus Status
        {
            get => Dialog.Status;
            set
            {
                Dialog.Status = value;
                OnPropertyChanged("DialogBackgroundColor");
            }
        }

        public SolidColorBrush DialogBackgroundColor
        {
            get
            {
                switch (Dialog.Status)
                {
                    case DialogStatus.AwaitingKeyExchange:
                        return new SolidColorBrush(Colors.LightGray);
                    case DialogStatus.Active:
                        return new SolidColorBrush(Color.FromRgb(41, 150, 243));
                    case DialogStatus.Canceled:
                    default:
                        return new SolidColorBrush(Colors.Gray);
                }
            }
        }

        public Color DialogShadowColor
        {
            get
            {
                return HasUnreadMessages ? new Color {R = 50, G = 205, B = 50} : new Color {R = 139, G = 139, B = 139};
            }
        }

        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public DialogPanel() { }

        public DialogPanel(AuthObject authObject, DialogDTO dialogDTO)
        {
            Dialog = dialogDTO;
            Interlocutor = dialogDTO.Participants.First(x => x.Id != authObject.User.Id);

            InitializeComponent();

            DownloadImage();
        }

        public async void DownloadImage()
        {
            try
            {
                SynchronizationContext sc = SynchronizationContext.Current;

                var wrq = WebRequest.CreateDefault(new Uri($"{Common.Host}/file/get/{Interlocutor.ImageId}"));

                wrq.ContentType = "image/jpeg";

                var wrs = await wrq.GetResponseAsync();

                var image = new BitmapImage();
                image.CreateOptions = BitmapCreateOptions.None;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.BeginInit();
                image.StreamSource = wrs.GetResponseStream();
                image.EndInit();

                sc.Post(
                    state => { UserpicImage.Source = (BitmapImage)state; }, image
                );
            }
            catch (Exception e)
            {
            }
        }
    }
}
