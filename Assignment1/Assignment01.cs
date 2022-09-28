using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CPI311.GameEngine;

namespace Assignment1;

public class Assignment01 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private List<Texture2D> walkDown = new List<Texture2D>();
    private List<Texture2D> walkUp = new List<Texture2D>();
    private List<Texture2D> walkLeft = new List<Texture2D>();
    private List<Texture2D> walkRight = new List<Texture2D>();
    private AnimatedSprite activeSprite;
    private AnimatedSprite spriteDown;
    private AnimatedSprite spriteUp;
    private AnimatedSprite spriteLeft;
    private AnimatedSprite spriteRight;
    private ProgressBar timeBar;
    private ProgressBar walkBar;
    private float timer;
    private float speed = 100f;
    private Vector2 mousePos;
    private const float maxTime = 30f;
    private float timeLeft = 30f;
    private Vector2 previousPos;
    private float distanceTravelled;
    private Sprite bonusSprite;
    private Random _random = new Random();
    private bool gameOver = false;
    private SpriteFont font;
    private string endString = "GAME OVER";

    public Assignment01()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        Time.Initialize();
        InputManager.Initialize();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        walkDown.Add(Content.Load<Texture2D>("tile008"));
        walkDown.Add(Content.Load<Texture2D>("tile009"));
        walkDown.Add(Content.Load<Texture2D>("tile010"));
        walkDown.Add(Content.Load<Texture2D>("tile011"));
        walkDown.Add(Content.Load<Texture2D>("tile012"));
        walkDown.Add(Content.Load<Texture2D>("tile013"));
        walkDown.Add(Content.Load<Texture2D>("tile014"));
        walkDown.Add(Content.Load<Texture2D>("tile015"));

        spriteDown = new AnimatedSprite(walkDown);
        
        walkUp.Add(Content.Load<Texture2D>("tile000"));
        walkUp.Add(Content.Load<Texture2D>("tile001"));
        walkUp.Add(Content.Load<Texture2D>("tile002"));
        walkUp.Add(Content.Load<Texture2D>("tile003"));
        walkUp.Add(Content.Load<Texture2D>("tile004"));
        walkUp.Add(Content.Load<Texture2D>("tile005"));
        walkUp.Add(Content.Load<Texture2D>("tile006"));
        walkUp.Add(Content.Load<Texture2D>("tile007"));

        spriteUp = new AnimatedSprite(walkUp);
        
        walkLeft.Add(Content.Load<Texture2D>("tile016"));
        walkLeft.Add(Content.Load<Texture2D>("tile017"));
        walkLeft.Add(Content.Load<Texture2D>("tile018"));
        walkLeft.Add(Content.Load<Texture2D>("tile019"));
        walkLeft.Add(Content.Load<Texture2D>("tile020"));
        walkLeft.Add(Content.Load<Texture2D>("tile021"));
        walkLeft.Add(Content.Load<Texture2D>("tile022"));
        walkLeft.Add(Content.Load<Texture2D>("tile023"));

        spriteLeft = new AnimatedSprite(walkLeft);
        
        walkRight.Add(Content.Load<Texture2D>("tile024"));
        walkRight.Add(Content.Load<Texture2D>("tile025"));
        walkRight.Add(Content.Load<Texture2D>("tile026"));
        walkRight.Add(Content.Load<Texture2D>("tile027"));
        walkRight.Add(Content.Load<Texture2D>("tile028"));
        walkRight.Add(Content.Load<Texture2D>("tile029"));
        walkRight.Add(Content.Load<Texture2D>("tile030"));
        walkRight.Add(Content.Load<Texture2D>("tile031"));

        spriteRight = new AnimatedSprite(walkRight);

        activeSprite = spriteDown;
        activeSprite.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
        mousePos = activeSprite.Position;
        previousPos = activeSprite.Position;

        timeBar = new ProgressBar(Content.Load<Texture2D>("Square"), Color.Red);
        walkBar = new ProgressBar(Content.Load<Texture2D>("Square"), Color.Green);
        timeBar.Position = new Vector2(50, 50);
        walkBar.Position = new Vector2(200, 50);

        bonusSprite = new Sprite(Content.Load<Texture2D>("Square"));
        bonusSprite.Scale = new Vector2(0.5f, 0.5f);
        bonusSprite.Position = new Vector2(_random.Next(0, GraphicsDevice.Viewport.Width),
            _random.Next(0, GraphicsDevice.Viewport.Height));

        font = Content.Load<SpriteFont>("font");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        spriteDown.Position = activeSprite.Position;
        spriteUp.Position = activeSprite.Position;
        spriteLeft.Position = activeSprite.Position;
        spriteRight.Position = activeSprite.Position;

        if (gameOver) return;
        Time.Update(gameTime);
        InputManager.Update();
        timer += Time.ElapsedGameTime;
        timeLeft -= Time.ElapsedGameTime;

        if (timeLeft <= 0)
        {
            gameOver = true;
        }

        if (InputManager.IsKeyPressed(Keys.Left))
        {
            if (activeSprite == spriteDown)
            {
                activeSprite = spriteRight;
            }
            else if (activeSprite == spriteUp)
            {
                activeSprite = spriteLeft;
            } else if (activeSprite == spriteLeft)
            {
                activeSprite = spriteDown;
            } else if (activeSprite == spriteRight)
            {
                activeSprite = spriteUp;
            }
        }
        if (InputManager.IsKeyPressed(Keys.Right))
        {
            if (activeSprite == spriteDown)
            {
                activeSprite = spriteLeft;
            }
            else if (activeSprite == spriteUp)
            {
                activeSprite = spriteRight;
            } else if (activeSprite == spriteLeft)
            {
                activeSprite = spriteUp;
            } else if (activeSprite == spriteRight)
            {
                activeSprite = spriteDown;
            }
        }
        if (InputManager.IsKeyDown(Keys.Up))
        {
            if (timer > 0.05f)
            {
                timer = 0f;
                activeSprite.Update();
            }
            if (activeSprite == spriteDown)
            {
                activeSprite.Position += new Vector2(0, speed * Time.ElapsedGameTime);
            }
            else if (activeSprite == spriteUp)
            {
                activeSprite.Position += new Vector2(0, speed * -Time.ElapsedGameTime);
            } else if (activeSprite == spriteLeft)
            {
                activeSprite.Position += new Vector2(speed * -Time.ElapsedGameTime, 0);
            } else if (activeSprite == spriteRight)
            {
                activeSprite.Position += new Vector2(speed * Time.ElapsedGameTime, 0);
            }
        }
        
        timeBar.setProgressScale(timeLeft / maxTime);
        distanceTravelled += Vector2.Distance(previousPos, activeSprite.Position);
        walkBar.setProgressScale(distanceTravelled / 10000f);

        if (distanceTravelled >= 10000f)
        {
            endString = "YOU WIN";
            gameOver = true;
        }

        if (Vector2.Distance(activeSprite.Position, bonusSprite.Position) < 25)
        {
            bonusSprite.Position = new Vector2(_random.Next(0, GraphicsDevice.Viewport.Width),
                _random.Next(0, GraphicsDevice.Viewport.Height));
            timeLeft += 3f;
        }

        previousPos = activeSprite.Position;
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        activeSprite.Draw(_spriteBatch);
        timeBar.Draw(_spriteBatch);
        walkBar.Draw(_spriteBatch);
        bonusSprite.Draw(_spriteBatch);
        _spriteBatch.DrawString(font, "Time Remaining:", new Vector2(timeBar.Position.X - 25, timeBar.Position.Y - 20), Color.White);
        _spriteBatch.DrawString(font, "Distance Travelled:", new Vector2(walkBar.Position.X - 30, timeBar.Position.Y - 20), Color.White);
        if (gameOver) _spriteBatch.DrawString(font, endString, new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2), Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}

