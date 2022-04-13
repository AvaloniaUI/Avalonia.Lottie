using Avalonia.Web.Blazor;

namespace Avalonia.Lottie.Web;

public partial class App
{
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        
        WebAppBuilder.Configure<Avalonia.Lottie.App>()
            .SetupWithSingleViewLifetime();
    }
}