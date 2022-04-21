using Android.App;
using Android.Content;

namespace Avalonia.Lottie.Android
{
    [Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : Activity
    {
        protected override void OnResume()
        {
            base.OnResume();

            StartActivity(new Intent(global::Android.App.Application.Context, typeof(MainActivity)));
        }
    }
}
