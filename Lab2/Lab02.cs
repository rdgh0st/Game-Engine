using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CPI311.GameEngine;

namespace Lab2;

public class Lab02 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Sprite _sprite;
    private Spiral _spiral;
    private SpriteFont font;

    public Lab02()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        InputManager.Initialize();
        Time.Initialize();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _sprite = new Sprite(Content.Load<Texture2D>("Square"));
        _spiral = new Spiral(_sprite);
        _sprite.Position = new Vector2(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2); // 400, 240
        font = Content.Load<SpriteFont>("font");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        Time.Update(gameTime);
        InputManager.Update();
        _spiral.UpdateSpiral();
        
        /*
        if(InputManager.IsKeyDown(Keys.Left)) _sprite.Position += Vector2.UnitX * -5;
        if(InputManager.IsKeyDown(Keys.Right)) _sprite.Position += Vector2.UnitX * 5;
        if(InputManager.IsKeyDown(Keys.Up)) _sprite.Position += Vector2.UnitY * -5;
        if(InputManager.IsKeyDown(Keys.Down)) _sprite.Position += Vector2.UnitY * 5;
        if(InputManager.IsKeyDown(Keys.Space)) _sprite.Rotation += 0.05f;
        */
        if(InputManager.IsKeyDown(Keys.Left)) _spiral.Radius -= 1;
        if(InputManager.IsKeyDown(Keys.Right)) _spiral.Radius += 1;
        if(InputManager.IsKeyDown(Keys.Up)) _spiral.Speed += 0.1f;
        if(InputManager.IsKeyDown(Keys.Down)) _spiral.Speed -= 0.1f;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        _sprite.Draw(_spriteBatch);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}

