using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Platform;

namespace Avalonia.Lottie.XPlat.ViewModels
{
    public class MainViewModel : ViewModelBase
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
                        new Uri("avares://Avalonia.Lottie.XPlat/Assets"),
                        new Uri("avares://Avalonia.Lottie.XPlat/"))
                    .Select(x=>x.AbsoluteUri)
                    .ToList();
                
                return _assetSources;
            }
        }

        public string? SelectedAsset
        {
            get => _selectedAsset;
            set
            {
                this._selectedAsset = value;
                OnPropertyChanged(SelectedAsset);
            }
        }
    }
}
