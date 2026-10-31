using Android.App;
using Android.Content.PM;
using Android.OS;

namespace PlantGuard_proyecto;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
	protected override void OnCreate(Bundle? savedInstanceState)
	{
		base.OnCreate(savedInstanceState);
		var window = Window;
		var decorView = window?.DecorView;
		if (window is null || decorView is null)
		{
			return;
		}

		window.SetStatusBarColor(Android.Graphics.Color.White);
		window.SetNavigationBarColor(Android.Graphics.Color.White);

		if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
		{
			window.InsetsController?.SetSystemBarsAppearance(8 | 16, 8 | 16);
		}
		else if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
		{
			decorView.SystemUiFlags |= Android.Views.SystemUiFlags.LightStatusBar
				| Android.Views.SystemUiFlags.LightNavigationBar;
		}
		else if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
		{
			decorView.SystemUiFlags |= Android.Views.SystemUiFlags.LightStatusBar;
		}
	}
}
