using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input.Touch;
using static ZongziCatcher.CSharpAndroid.Essentials;

namespace ZongziCatcher.CSharpAndroid;

internal enum GameState : int
{
    Title = 0,
    Playing = 1,
    Paused = 2,
    GameOver = 3
}

public sealed class GameMain : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private RenderTarget2D? _renderTarget;
    private SpriteFont _font = null!;

    private Texture2D imgBackground = null!;
    private Texture2D imgDragonBoat = null!;
    private Texture2D imgZongzi = null!;
    private Texture2D imgScorpion = null!;
    private Texture2D imgWaterSplash = null!;
    private Texture2D imgLeftArrow = null!;

    private Player _player = null!;
    private List<FallingItem> _fallingItems = [];
    private readonly List<WaterSplash> _waterSplashes = [];
    private readonly List<Caption> _captions = [];

    private GameState _gameState = GameState.Title;
        private float _spawnTimer = 0f;
        private readonly Random _random = Random.Shared;
        private bool _leftArrowPressed;
        private bool _rightArrowPressed;

    public GameMain()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        _graphics.IsFullScreen = true;
        _graphics.ApplyChanges();

        Window.Title = "Zongzi Catcher - Dragon Boat Festival";
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _renderTarget = new RenderTarget2D(GraphicsDevice, ScreenWidth, ScreenHeight);

        imgBackground = Content.Load<Texture2D>("Images/background");
        imgDragonBoat = Content.Load<Texture2D>("Images/dragon_boat");
        imgZongzi = Content.Load<Texture2D>("Images/zongzi");
        imgScorpion = Content.Load<Texture2D>("Images/scorpion");
        imgWaterSplash = Content.Load<Texture2D>("Images/water_splash");
        imgLeftArrow = Content.Load<Texture2D>("Images/left_arrow");
        _font = Content.Load<SpriteFont>("Fonts/GameFont");

        if (!SoundManager.Instance.Initialized)
        {
            SoundManager.Instance.LoadSounds(Content);
        }

        _player = CreatePlayer();
    }

    private static Player CreatePlayer()
    {
        Vector2 playerPosition = new(
            ScreenWidth / 2f - 65f,
            ScreenHeight - 100f
        );

        Player player = new(playerPosition)
        {
            Lives = InitialLives,
            Score = 0
        };
        return player;
    }

    protected override void Update(GameTime gameTime)
        {
            switch (_gameState)
            {
                case GameState.Title:
                    HandleTitleState();
                    break;

                case GameState.Playing:
                    HandlePlayingState(gameTime);
                    break;

                case GameState.Paused:
                    HandlePausedState();
                    break;

                case GameState.GameOver:
                    HandleGameOverState();
                    break;
            }

            base.Update(gameTime);
        }

    private void HandleTitleState()
    {
        if (TouchPanel.GetState().Any(t => t.State == TouchLocationState.Pressed))
        {
            SoundManager.Instance.PlayBGM();
            TriggerGameStarted();

            _gameState = GameState.Playing;
            _player = CreatePlayer();
            _fallingItems.Clear();
            _waterSplashes.Clear();
            _captions.Clear();
            _spawnTimer = 0f;
        }
    }

    private void HandlePlayingState(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _leftArrowPressed = false;
        _rightArrowPressed = false;

        int deviceWidth = GraphicsDevice.Viewport.Width;
        float scale = (float)GraphicsDevice.Viewport.Height / ScreenHeight;
        float scaledWidth = ScreenWidth * scale;
        float gameAreaX = (GraphicsDevice.Viewport.Width - scaledWidth) / 2f;

        foreach (TouchLocation touch in TouchPanel.GetState())
        {
            if (touch.State == TouchLocationState.Pressed)
            {
                if (touch.Position.X < gameAreaX)
                {
                    _player.MoveLeft(PlayerSpeed, dt);
                    _leftArrowPressed = true;
                }
                else if (touch.Position.X > gameAreaX + scaledWidth)
                {
                    _player.MoveRight(PlayerSpeed, dt, ScreenWidth);
                    _rightArrowPressed = true;
                }
                else
                {
                    _gameState = GameState.Paused;
                    SoundManager.PauseBGM();
                }
                break;
            }
            else if (touch.State == TouchLocationState.Moved)
            {
                if (touch.Position.X < gameAreaX)
                {
                    _player.MoveLeft(PlayerSpeed, dt);
                    _leftArrowPressed = true;
                }
                else if (touch.Position.X > gameAreaX + scaledWidth)
                {
                    _player.MoveRight(PlayerSpeed, dt, ScreenWidth);
                    _rightArrowPressed = true;
                }
            }
        }

        foreach (var item in _fallingItems)
        {
            item.Update(gameTime);
        }

        foreach (var splash in _waterSplashes)
        {
            splash.Update(gameTime);
        }
        _waterSplashes.RemoveAll(s => s.IsDead);

        foreach (var caption in _captions)
        {
            caption.Update(gameTime);
        }
        _captions.RemoveAll(c => c.IsDead);

        List<FallingItem> remainingItems = [];

        foreach (var item in _fallingItems)
        {
            Vector2 captionPos = new(item.Position.X, item.Position.Y - 30f);

            if (item.IsOffScreen(ScreenHeight))
            {
                if (item.Type == ActorType.Zongzi)
                {
                    _player.Score += PointsPerDrop;
                    _waterSplashes.Add(new WaterSplash(new Vector2(item.Position.X, ScreenHeight - 25f)));
                    _captions.Add(new Caption(PointsPerDrop.ToString(), captionPos));
                    SoundManager.Instance.PlayDrop();
                }
            }
            else if (CheckCollision(_player, item))
            {
                if (item.Type == ActorType.Zongzi)
                {
                    _player.Score += PointsPerCollect;
                    _captions.Add(new Caption($"+{PointsPerCollect}", captionPos));
                    SoundManager.Instance.PlayItemCollected();
                }
                else
                {
                    _player.Lives--;
                    _captions.Add(new Caption("Ouch!", new Vector2(item.Position.X, item.Position.Y - 30f)));
                    SoundManager.Instance.PlayPlayerHit();
                }
            }
            else
            {
                remainingItems.Add(item);
            }
        }

        _fallingItems = remainingItems;

        _spawnTimer += dt;
        float spawnInterval = Math.Max(0.3f, SpawnInterval - (_player.Score / 100f) * 0.1f);

        if (_spawnTimer >= spawnInterval)
        {
            _fallingItems.Add(SpawnItem(_player.Score));
            _spawnTimer = 0f;
        }

        if (_player.Lives <= 0)
        {
            SoundManager.StopBGM();
            SoundManager.Instance.PlayGameOver();
            _gameState = GameState.GameOver;
            TriggerGameEnded();
        }
    }

    private void HandlePausedState()
    {
        if (TouchPanel.GetState().Any(t => t.State == TouchLocationState.Pressed))
        {
            SoundManager.ResumeBGM();
            _gameState = GameState.Playing;
        }
    }

    private void HandleGameOverState()
    {
        if (TouchPanel.GetState().Any(t => t.State == TouchLocationState.Pressed))
        {
            SoundManager.Instance.PlayBGM();
            TriggerGameStarted();

            _gameState = GameState.Playing;
            _player = CreatePlayer();
            _fallingItems.Clear();
            _waterSplashes.Clear();
            _captions.Clear();
            _spawnTimer = 0f;
        }
    }

    private FallingItem SpawnItem(int score)
    {
        ActorType itemType = _random.NextDouble() < 0.8 ? ActorType.Zongzi : ActorType.Scorpion;

        float x = _random.Next(ScreenWidth - 50);
        float speedBoost = (score / 50f) * 10f;
        float speed = BaseItemSpeed + _random.Next(100) + speedBoost;

        return new FallingItem(new Vector2(x, -50f), speed, itemType);
    }

    private static bool CheckCollision(Player player, FallingItem item)
    {
        Rectangle playerRect = new(
            (int)player.Position.X,
            (int)player.Position.Y,
            (int)player.SpriteSize.X,
            (int)player.SpriteSize.Y
        );

        Rectangle itemRect = new(
            (int)item.Position.X,
            (int)item.Position.Y,
            (int)item.SpriteSize.X,
            (int)item.SpriteSize.Y
        );

        return playerRect.Intersects(itemRect);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetRenderTarget(_renderTarget);
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

        _spriteBatch.Draw(imgBackground, new Rectangle(0, 0, ScreenWidth, ScreenHeight), Color.White);

        if (_gameState == GameState.Title)
        {
            DrawTextWithBgColor("ZONGZI CATCHER: Dragon Boat Festival",
                new Vector2(ScreenWidth / 2f - 200f, ScreenHeight / 2f - 80f),
                Color.White, Color.DarkCyan);

            DrawTextWithBgColor("Tap the screen to begin",
                new Vector2(ScreenWidth / 2f - 130f, ScreenHeight / 2f - 30f),
                Color.Yellow, new Color(0, 0, 0, 200));

            DrawTextWithBgColor("Tap arrow buttons to move | Tap game area to pause",
                new Vector2(ScreenWidth / 2f - 220f, ScreenHeight / 2f + 50f),
                Color.White, new Color(0, 0, 0, 200));

            DrawTextWithBgColor("Catch zongzi (+10 pts) | Avoid scorpions (-1 Life)",
                new Vector2(ScreenWidth / 2f - 225f, ScreenHeight / 2f + 90f),
                Color.White, new Color(0, 0, 0, 200));
        }
        else
        {
            foreach (var item in _fallingItems)
            {
                Texture2D texture = item.Type == ActorType.Zongzi ? imgZongzi : imgScorpion;
                _spriteBatch.Draw(texture,
                    new Rectangle((int)item.Position.X, (int)item.Position.Y,
                        (int)item.SpriteSize.X, (int)item.SpriteSize.Y),
                    Color.White);
            }

            _spriteBatch.Draw(imgDragonBoat,
                new Rectangle((int)_player.Position.X, (int)_player.Position.Y,
                    (int)_player.SpriteSize.X, (int)_player.SpriteSize.Y),
                Color.White);

            foreach (var splash in _waterSplashes)
            {
                int alpha = (int)(splash.LifeTime / 0.5f * 255f);
                _spriteBatch.Draw(imgWaterSplash,
                    new Rectangle((int)splash.Position.X - 25, (int)splash.Position.Y - 25, 50, 50),
                    new Color(255, 255, 255, alpha));
            }

            DrawTextWithBgColor($"Score: {_player.Score}", new Vector2(10f, 10f), Color.White, new Color(0, 0, 0, 200));
            DrawTextWithBgColor($"Lives: {_player.Lives}", new Vector2(ScreenWidth - 100f, 10f), Color.White, new Color(0, 0, 0, 200));

            if (_gameState == GameState.Playing)
            {
                const string PauseMessage = "Tap game area to pause";
                Vector2 textSize = _font.MeasureString(PauseMessage);
                Vector2 textPos = new(ScreenWidth / 2 - textSize.X / 2, 10f);
                _spriteBatch.DrawString(_font, PauseMessage, textPos, Color.White);
            }

            foreach (var caption in _captions)
            {
                int alpha = (int)(caption.LifeTime * 255f);
                Color textColor = caption.Text.StartsWith('+') ? Color.Green
                    : caption.Text.StartsWith('-') ? Color.Red
                    : Color.Yellow;

                _spriteBatch.DrawString(_font, caption.Text, caption.Position,
                    new Color(textColor.R, textColor.G, textColor.B, alpha));
            }

            if (_gameState == GameState.GameOver)
            {
                DrawTextWithBgColor("GAME OVER!",
                    new Vector2(ScreenWidth / 2f - 80f, ScreenHeight / 2f - 50f),
                    Color.White, Color.DarkRed);

                DrawTextWithBgColor($"Final Score: {_player.Score,5}",
                    new Vector2(ScreenWidth / 2f - 100f, ScreenHeight / 2f),
                    Color.White, new Color(0, 0, 0, 200));

                DrawTextWithBgColor("Tap the screen to restart",
                    new Vector2(ScreenWidth / 2f - 150f, ScreenHeight / 2f + 50f),
                    Color.White, new Color(0, 0, 0, 200));
            }

            if (_gameState == GameState.Paused)
            {
                DrawTextWithBgColor("PAUSED",
                    new Vector2(ScreenWidth / 2f - 60f, ScreenHeight / 2f - 30f),
                    Color.White, Color.DarkBlue);

                DrawTextWithBgColor("Tap the screen to continue",
                    new Vector2(ScreenWidth / 2f - 150f, ScreenHeight / 2f + 20f),
                    Color.Yellow, new Color(0, 0, 0, 200));
            }
        }

        _spriteBatch.End();

        GraphicsDevice.SetRenderTarget(null);
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

        float scale = (float)GraphicsDevice.Viewport.Height / ScreenHeight;
        float scaledWidth = ScreenWidth * scale;
        float gameAreaX = (GraphicsDevice.Viewport.Width - scaledWidth) / 2f;

        _spriteBatch.Draw(_renderTarget, new Rectangle((int)gameAreaX, 0, (int)scaledWidth, GraphicsDevice.Viewport.Height), Color.White);

        if (_gameState == GameState.Playing && gameAreaX > 0)
        {
            const int ArrowSize = 200, ArrowMargin = 80;
            int arrowY = GraphicsDevice.Viewport.Height / 2 - ArrowSize / 2;

            Color leftArrowColor = _leftArrowPressed ? Color.LightGreen : Color.White;
            _spriteBatch.Draw(imgLeftArrow,
                new Rectangle(ArrowMargin, arrowY, ArrowSize, ArrowSize),
                leftArrowColor);

            Color rightArrowColor = _rightArrowPressed ? Color.LightGreen : Color.White;
            int rightArrowX = GraphicsDevice.Viewport.Width - ArrowSize - ArrowMargin;
            _spriteBatch.Draw(imgLeftArrow,
                new Rectangle(rightArrowX, arrowY, ArrowSize, ArrowSize),
                null,
                rightArrowColor,
                MathHelper.Pi,
                new Vector2(ArrowSize / 2f, ArrowSize / 2f),
                SpriteEffects.None,
                0f);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void DrawTextWithBgColor(string text, Vector2 position, Color textColor, Color bgColor)
    {
        Vector2 textSize = _font.MeasureString(text);
        const float padding = 5f;

        Rectangle bgRect = new(
            (int)(position.X - padding),
            (int)(position.Y - padding),
            (int)(textSize.X + padding * 2f),
            (int)(textSize.Y + padding * 2f)
        );

        Texture2D pixel = new(GraphicsDevice, 1, 1);
        pixel.SetData([bgColor]);
        _spriteBatch.Draw(pixel, bgRect, bgColor);
        _spriteBatch.DrawString(_font, text, position, textColor);
    }
}