using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using ReactiveUI;

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
            var assetLoader = AvaloniaLocator.Current.GetService<IAssetLoader>();
 
            AvaloniaXamlLoader.Load(this);

            var cb = this.FindControl<ComboBox>("AssetSelector");
            var lt = this.FindControl<Lottie>("Lottie");
            
            cb.Items =  assetLoader.GetAssets(new Uri("avares://Avalonia.Lottie.Sample/Assets/"), new Uri("/"))
                .ToList();

            cb.WhenAnyValue(x => x.SelectedItem)
                .Cast<Uri>()
                .Where(x=>x is not null)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x =>
                {
                    using var asset = assetLoader.Open(x);
                    
                    var result = new LottieCompositionSource
                    {
                        Composition = LottieCompositionFactory
                            .FromJsonInputStreamSync(asset, x.AbsoluteUri).Value
                    };
                    
                    lt.Source = result;
                });


        }
    }
}