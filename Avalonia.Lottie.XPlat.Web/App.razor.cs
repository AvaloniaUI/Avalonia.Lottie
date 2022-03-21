using Avalonia.Web.Blazor;

namespace Avalonia.Lottie.XPlat.Web;

public partial class App
{
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        
        WebAppBuilder.Configure<Avalonia.Lottie.XPlat.App>()
            .SetupWithSingleViewLifetime();
    }
}