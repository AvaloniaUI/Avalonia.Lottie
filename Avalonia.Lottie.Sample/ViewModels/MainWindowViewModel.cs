using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Platform;
using ReactiveUI;

namespace Avalonia.Lottie.Sample.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private IEnumerable<string>? _assetSources;
        private string? _selectedAsset;

        public IEnumerable<string> AssetSources
        {
            get
            {
                if (_assetSources is not null) return _assetSources;

                var asset = AvaloniaLocator.Current.GetService<IAssetLoader>();

                _assetSources = asset.GetAssets(
                        new Uri("avares://Avalonia.Lottie.Sample/Assets"),
                        new Uri("avares://Avalonia.Lottie.Sample/"))
                    .Select(x => x.AbsoluteUri)
                    .ToList();


                Task.Run(() =>
                {
                    Thread.Sleep(1000);
                    SelectedAsset = _assetSources.First();
                });

                return _assetSources;
            }
        }

        public string? SelectedAsset
        {
            get => _selectedAsset;
            set => this.RaiseAndSetIfChanged(ref _selectedAsset, value, nameof(SelectedAsset));
        }
    }
}