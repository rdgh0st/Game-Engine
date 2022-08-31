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
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        Time.Update(gameTime);
        InputManager.Update();
        if(InputManager.IsKeyDown(Keys.Left)) _sprite.Position += Vector2.UnitX * -5;
        if(InputManager.IsKeyDown(Keys.Right)) _sprite.Position += Vector2.UnitX * 5;
        if(InputManager.IsKeyDown(Keys.Up)) _sprite.Position += Vector2.UnitY * -5;
        if(InputManager.IsKeyDown(Keys.Down)) _sprite.Position += Vector2.UnitY * 5;
        if(InputManager.IsKeyDown(Keys.Space)) _sprite.Rotation += 0.05f;

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

