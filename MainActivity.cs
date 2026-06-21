using Android.Content.PM;
using Android.Views;
using Microsoft.Xna.Framework;

namespace ZongziCatcher.CSharpAndroid;

[Activity(
    Label = "@string/app_name",
    MainLauncher = true,
    Icon = "@drawable/icon",
    AlwaysRetainTaskState = true,
    LaunchMode = LaunchMode.SingleInstance,
    ScreenOrientation = ScreenOrientation.Landscape,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize
)]
public class MainActivity : AndroidGameActivity
{
    private GameMain? _game;
    private View? _view;

    protected override void OnCreate(Bundle bundle)
    {
        base.OnCreate(bundle);

        _game = new GameMain();
        _view = _game.Services.GetService<View>();

        SetContentView(_view);
        _game.Run();
    }
}