using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MediaManager;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Server.RequestModel;

namespace xamarinJKH.CustomControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MessageCallControl : Frame
    {
        public static readonly BindableProperty CallProperty = BindableProperty.Create(
            nameof(Call),
            typeof(MessageCall),
            typeof(MessageCallControl),
            new MessageCall(),
            BindingMode.TwoWay
        ); 
        
        public MessageCall Call
        {
            get => (MessageCall)GetValue(CallProperty);
            set
            {
                Maximum = value.Duration;
                SetValue(CallProperty, value);
            }
        }
        public static readonly BindableProperty ColorProperty = BindableProperty.Create(
            nameof(Color),
            typeof(Color),
            typeof(MessageCallControl),
            Color.White,
            BindingMode.TwoWay
        ); 
        
        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set
            {
                SetValue(ColorProperty, value);
            }
        }

        #region Position

        private TimeSpan _position = TimeSpan.Zero;

        public TimeSpan Position
        {
            get { return _position; }
            set
            {
                _position = value;
                OnPropertyChanged("Position");
            }
        }

        #endregion

        #region Duration

        private TimeSpan _duration;

        public TimeSpan Duration
        {
            get { return _duration; }
            set
            {
                _duration = value;
                OnPropertyChanged("Duration");
            }
        }

        #endregion

        public IMediaManager MediaInfo { get; set; }

        double maximum = 100f;
        public double Maximum
        {
            get { return maximum; }
            set
            {
                if (value > 0)
                {
                    maximum = value;
                    OnPropertyChanged(); 
                }
            }
        }
        
        async void PlaySound(string link)
        {
            MediaInfo = CrossMediaManager.Current;
            await MediaInfo.Play(link);
            IsPlaying = true;
            MediaInfo.MediaItemFinished += (sender, args) =>
            {
                IsPlaying = false;
            };
            Maximum = MediaInfo.Duration.Seconds;
            Device.StartTimer(TimeSpan.FromMilliseconds(500), () => 
            {
                Duration = MediaInfo.Duration;
                Position = MediaInfo.Position;
                return true;
            });
        }
        
        private async void Play()
        {
            if(isPlaying)
            {
                await CrossMediaManager.Current.Pause();
                IsPlaying = false; ;
            }
            else
            {
                if (MediaInfo != null)
                {
                    await CrossMediaManager.Current.Play();
                    IsPlaying = true;
                }
                else
                {
                    PlaySound(Call.Link);
                }
            }
        }

        #region PlayPauseCommand
        
        public ICommand PlayPauseCommand => new Command(Play);
        public ICommand DownLoadCommand => new Command(DownLoadActions);

        private async void DownLoadActions()
        {
            try
            {
                await Launcher.OpenAsync(Call?.Link);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        #endregion

        
        
        public string PlayIcon { get => isPlaying ? "Ⅱ" : "ᐅ"; }
        
        private bool isPlaying;
        public bool IsPlaying
        {
            get { return isPlaying; }
            set
            {
                isPlaying = value;
                OnPropertyChanged("IsPlaying");
                OnPropertyChanged(nameof(PlayIcon));
            }
        }
        
        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            if (BindingContext != null)
            {
            
            }
        }

        public MessageCallControl()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private void Slider_OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if(Math.Abs(e.NewValue - e.OldValue) > 1)
                if (MediaInfo != null)
                    MediaInfo.SeekTo(TimeSpan.FromSeconds(e.NewValue));
        }
    }
}