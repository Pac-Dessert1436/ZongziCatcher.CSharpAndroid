using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace ZongziCatcher.CSharpAndroid;

public static class Essentials
{
    public const int ScreenWidth = 800;
    public const int ScreenHeight = 600;
    public const float PlayerSpeed = 300f;
    public const float SpawnInterval = 1.5f;
    public const float BaseItemSpeed = 150f;
    public const int PointsPerCollect = 10;
    public const int PointsPerDrop = -5;
    public const int InitialLives = 3;

    public static event Action? ZongziCollected;
    public static event Action? ZongziDropped;
    public static event Action? PlayerHit;
    public static event Action? GameStarted;
    public static event Action? GameEnded;

    public static void TriggerZongziCollected() => ZongziCollected?.Invoke();
    public static void TriggerZongziDropped() => ZongziDropped?.Invoke();
    public static void TriggerPlayerHit() => PlayerHit?.Invoke();
    public static void TriggerGameStarted() => GameStarted?.Invoke();
    public static void TriggerGameEnded() => GameEnded?.Invoke();
}

public sealed class SoundManager
{
    private static readonly Lazy<SoundManager> _instance = new(() => new SoundManager());
    
    private bool _initialized = false;
    private SoundEffect? _itemCollectedSound;
    private SoundEffect? _dropSound;
    private SoundEffect? _playerHitSound;
    private SoundEffect? _gameOverSound;
    private Song? _bgm;

    private SoundManager() { }

    public static SoundManager Instance => _instance.Value;

    public bool Initialized => _initialized;

    public void LoadSounds(Microsoft.Xna.Framework.Content.ContentManager content)
    {
        if (_initialized) return;

        try { _itemCollectedSound = content.Load<SoundEffect>("Sounds/item_collected"); }
        catch { }

        try { _dropSound = content.Load<SoundEffect>("Sounds/drop_into_water"); }
        catch { }

        try { _playerHitSound = content.Load<SoundEffect>("Sounds/player_hit"); }
        catch { }

        try { _gameOverSound = content.Load<SoundEffect>("Sounds/game_over"); }
        catch { }

        try { _bgm = content.Load<Song>("Sounds/BGM/main_theme"); }
        catch { }

        _initialized = true;
    }

    public void PlayItemCollected() => _itemCollectedSound?.Play();

    public void PlayDrop() => _dropSound?.Play();

    public void PlayPlayerHit() => _playerHitSound?.Play();

    public void PlayGameOver() => _gameOverSound?.Play();

    public void PlayBGM()
    {
        if (MediaPlayer.State != MediaState.Playing && _bgm != null)
        {
            MediaPlayer.Play(_bgm);
            MediaPlayer.IsRepeating = true;
        }
    }

    public static void StopBGM() => MediaPlayer.Stop();

    public static void PauseBGM() => MediaPlayer.Pause();

    public static void ResumeBGM() => MediaPlayer.Resume();
}