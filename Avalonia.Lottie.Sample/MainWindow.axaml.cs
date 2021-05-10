using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;

namespace Avalonia.Lottie.Sample
{
    public class MainWindow : Window
    {
        private Lottie _lottie;

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
            _lottie = new Lottie();
            contentControl.Content = _lottie;
            // var xurl =
            //     "https://raw.githubusercontent.com/wintb/lottie-example/master/LottieSample/src/androidTest/assets/LightBulb.json";
            // string s;
            // using (WebClient client = new WebClient())
            // {
            //  s = client.DownloadString(xurl);
            // }


            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            var getstr = assets.Open(new Uri("avares://Avalonia.Lottie.Sample/Assets/day_night_cycle.json"));
            // var getstr = assets.Open(new Uri("avares://Avalonia.Lottie.Sample/Assets/42495-payment-security.json"));

            var a = await new StreamReader(getstr).ReadToEndAsync();

            var res = await LottieCompositionFactory.FromJsonString(a, "asd");
            if (res is not null)
            {
                LottieLog.TraceEnabled = true;
                _lottie.SetComposition(res.Value);
                //_lottieDrawable.DirectScale = 0.25f;
                _lottie.Start();
                _lottie.RepeatCount = -1;
            }
        }
    }
}