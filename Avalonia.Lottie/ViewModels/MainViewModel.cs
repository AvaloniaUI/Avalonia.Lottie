using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Avalonia.Lottie.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        private IEnumerable<string>? _assetSources;
        
        [ObservableProperty]
        private string? _selectedAsset;

        public IEnumerable<string> AssetSources
        {
            get
            {
                if (_assetSources is not null) return _assetSources;
                
                var asset = AvaloniaLocator.Current.GetService<IAssetLoader>();
                
                _assetSources = asset.GetAssets(
                        new Uri("avares://Avalonia.Lottie/Assets"),
                        new Uri("avares://Avalonia.Lottie/"))
                    .Select(x=>x.AbsoluteUri)
                    .ToList();
                
                return _assetSources;
            }
        }
 
        
    }
}
