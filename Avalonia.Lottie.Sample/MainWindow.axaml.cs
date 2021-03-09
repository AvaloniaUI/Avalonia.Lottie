using System;
using System.IO;
using System.Net;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform; 

namespace Avalonia.Lottie.Sample
{
    public class MainWindow : Window
    {
        private LottieDrawable _lottieDrawable;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            var h = this.FindControl<ContentControl>("Cont");

            DoLoadDrawable(h);
        }

        private async void DoLoadDrawable(ContentControl contentControl)
        {
            _lottieDrawable = new LottieDrawable();
            contentControl.Content = _lottieDrawable;
            // var xurl =
            //     "https://raw.githubusercontent.com/wintb/lottie-example/master/LottieSample/src/androidTest/assets/LightBulb.json";
            // string s;
            // using (WebClient client = new WebClient())
            // {
            //  s = client.DownloadString(xurl);
            // }
            
            
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            var getstr = assets.Open(new Uri("avares://Avalonia.Lottie.Sample/Assets/2719-bitcoin-to-the-moon.json"));
            var a = await new StreamReader(getstr).ReadToEndAsync();

            var res = await LottieCompositionFactory.FromJsonString(a, "cache1");
            if (res is not null)
            {
                _lottieDrawable.SetComposition(res.Value);
                 //_lottieDrawable.DirectScale = 0.25f;
                 _lottieDrawable.Start();
                 _lottieDrawable.RepeatCount = Int32.MaxValue;
                _lottieDrawable.Scale = 0.25f;
                
            }
        }
    }
}