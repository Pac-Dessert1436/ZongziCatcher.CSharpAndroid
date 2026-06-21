using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ZongziCatcher.CSharpAndroid;

public enum ActorType : int
{
    DragonBoat = 0,
    Zongzi = 1,
    Scorpion = 2
}

public class Actor(Vector2 position, float speed, ActorType actorType)
{
    protected Vector2 _position = position;
    protected float _speed = speed;
    protected readonly ActorType _actorType = actorType;

    public Guid Id { get; } = Guid.NewGuid();

    public Vector2 Position
    {
        get => _position;
        set => _position = value;
    }

    public float Speed
    {
        get => _speed;
        set => _speed = value;
    }

    public ActorType Type => _actorType;

    public Vector2 SpriteSize
    {
        get
        {
            return _actorType switch
            {
                ActorType.DragonBoat => new Vector2(130f, 80f),
                _ => new Vector2(50f, 50f)
            };
        }
    }

    public virtual void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _position = new Vector2(_position.X, _position.Y + _speed * dt);
    }

    public virtual void Draw(SpriteBatch spriteBatch, Texture2D texture)
    {
        spriteBatch.Draw(
            texture,
            new Rectangle(
                (int)_position.X,
                (int)_position.Y,
                (int)SpriteSize.X,
                (int)SpriteSize.Y
            ),
            Color.White
        );
    }
}

public class Player(Vector2 position) : Actor(position, 0f, ActorType.DragonBoat)
{
    public int Score { get; set; }  // Already equals 0
    public int Lives { get; set; } = Essentials.InitialLives;

    public void MoveLeft(float speed, float dt)
    {
        float newX = Math.Max(0f, Position.X - speed * dt);
        Position = new Vector2(newX, Position.Y);
    }

    public void MoveRight(float speed, float dt, int deviceWidth)
    {
        float maxX = deviceWidth - SpriteSize.X;
        float newX = Math.Min(maxX, Position.X + speed * dt);
        Position = new Vector2(newX, Position.Y);
    }
}

public class FallingItem(Vector2 position, float speed, ActorType itemType)
    : Actor(position, speed, itemType)
{
    public bool IsOffScreen(int screenHeight)
    {
        return Position.Y > screenHeight;
    }
}

public class WaterSplash(Vector2 position)
{
    private Vector2 _position = position;
    private float _lifeTime = 0.5f;

    public Vector2 Position => _position;
    public float LifeTime => _lifeTime;

    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _lifeTime -= dt;
    }

    public bool IsDead => _lifeTime <= 0f;

    public void Draw(SpriteBatch spriteBatch, Texture2D texture)
    {
        int alpha = (int)(_lifeTime / 0.5f * 255f);
        spriteBatch.Draw(
            texture,
            new Rectangle((int)_position.X - 25, (int)_position.Y - 25, 50, 50),
            new Color(255, 255, 255, alpha)
        );
    }
}

public class Caption(string text, Vector2 position)
{
    private readonly string _text = text;
    private Vector2 _position = position;
    private float _lifeTime = 1.0f;

    public string Text => _text;
    public Vector2 Position => _position;
    public float LifeTime => _lifeTime;

    public void Update(GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _lifeTime -= dt;
        _position = new Vector2(_position.X, _position.Y - 20f * dt);
    }

    public bool IsDead => _lifeTime <= 0f;
}