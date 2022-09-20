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
    private float speed = 2f;
    private Vector2 mousePos;
    private const float maxTime = 10f;
    private float timeLeft = 10f;
    private Vector2 previousPos;
    private float distanceTravelled;
    private Sprite bonusSprite;
    private Random _random = new Random();

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
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        Time.Update(gameTime);
        InputManager.Update();
        timer += Time.ElapsedGameTime;
        timeLeft -= Time.ElapsedGameTime;

        if (InputManager.isMouseLeftClicked())
        {
            mousePos = InputManager.getMouseState().Position.ToVector2();
        }
        // get mouse position
        // if moving, lerp to mouse position
        // on arrival, stop moving
        activeSprite.Position = Vector2.Lerp(activeSprite.Position, mousePos, speed * Time.ElapsedGameTime);
        timeBar.setProgressScale(timeLeft / maxTime);
        distanceTravelled += Vector2.Distance(previousPos, activeSprite.Position);
        walkBar.setProgressScale(distanceTravelled / 10000f);

        if (Vector2.Distance(activeSprite.Position, bonusSprite.Position) < 25)
        {
            bonusSprite.Position = new Vector2(_random.Next(0, GraphicsDevice.Viewport.Width),
                _random.Next(0, GraphicsDevice.Viewport.Height));
            timeLeft += 1.5f;
        }
        
        if (InputManager.IsKeyDown(Keys.W))
        {
            
            /*
            Vector2 currentPosition = activeSprite.Position;
            activeSprite = spriteUp;
            activeSprite.Position = currentPosition + new Vector2(0, -Time.ElapsedGameTime * speed);
            */
        }
        if (InputManager.IsKeyDown(Keys.S))
        {
            /*
            Vector2 currentPosition = activeSprite.Position;
            activeSprite = spriteDown;
            activeSprite.Position = currentPosition + new Vector2(0, Time.ElapsedGameTime * speed);
            */
        }
        if (InputManager.IsKeyDown(Keys.A))
        {
            activeSprite.Rotation -= speed * Time.ElapsedGameTime;
            /*
            Vector2 currentPosition = activeSprite.Position;
            activeSprite = spriteLeft;
            activeSprite.Position = currentPosition + new Vector2(-Time.ElapsedGameTime * speed, 0);
            */
        }
        if (InputManager.IsKeyDown(Keys.D))
        {
            activeSprite.Rotation += speed * Time.ElapsedGameTime;
            /*
            Vector2 currentPosition = activeSprite.Position;
            activeSprite = spriteRight;
            activeSprite.Position = currentPosition + new Vector2(Time.ElapsedGameTime * speed, 0);
            */
        }

        if (timer > 0.05f)
        {
            timer = 0f;
            activeSprite.Update();
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
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}

