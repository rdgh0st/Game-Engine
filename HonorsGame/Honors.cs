using System;
using System.Collections.Generic;
using System.Threading;
using CPI311.GameEngine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace HonorsGame;

public class Honors : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private HonorsPlayer player;
    private Sprite background;
    private Sprite background2;
    private List<ObstacleCar> cars;
    private List<Fuel> items;
    private int timeToSpawn = 3000;
    private float score = 0;
    private SpriteFont font;

    public Honors()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        ScreenManager.Initialize(_graphics);
        Time.Initialize();
        InputManager.Initialize();
        cars = new List<ObstacleCar>();
        items = new List<Fuel>();
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        ScreenManager.Setup(false, 480, 720);

        font = Content.Load<SpriteFont>("font");

        background = new Sprite(Content.Load<Texture2D>("road"));
        background.Scale = new Vector2(0.45f, 2f);
        background.Position = new Vector2(GraphicsDevice.Viewport.Width / 4, GraphicsDevice.Viewport.Height / 2);
        background2 = new Sprite(Content.Load<Texture2D>("road"));
        background2.Scale = new Vector2(0.45f, 2f);
        background2.Rotation = MathHelper.ToRadians(180);
        background2.Position = new Vector2(3 * GraphicsDevice.Viewport.Width / 4, GraphicsDevice.Viewport.Height / 2);
        
        player = new HonorsPlayer(Content.Load<Texture2D>("Black_viper"), Content.Load<Texture2D>("Square"));
        ThreadPool.QueueUserWorkItem(new WaitCallback(SpawnCar));
        ThreadPool.QueueUserWorkItem(new WaitCallback(SpawnItem));
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        if (player.crashed) return;
        Time.Update(gameTime);
        InputManager.Update();

        if (InputManager.isMouseLeftClicked())
        {
            Console.WriteLine(InputManager.GetMousePosition().ToString());
        }

        if (background.Position.Y < GraphicsDevice.Viewport.Height)
        {
            background.Position =
                new Vector2(background.Position.X, background.Position.Y + 500 * Time.ElapsedGameTime);
        }
        else
        {
            background.Position = new Vector2(background.Position.X, GraphicsDevice.Viewport.Height / 2);
        }

        if (background2.Position.Y < GraphicsDevice.Viewport.Height)
        {
            background2.Position =
                new Vector2(background2.Position.X, background2.Position.Y + 500 * Time.ElapsedGameTime);
        }
        else
        {
            background2.Position = new Vector2(background2.Position.X, GraphicsDevice.Viewport.Height / 2);
        }

        player.Update();
        for (int i = 0; i < cars.Count; i++)
        {
            cars[i].Update();
            if (Vector2.Distance(cars[i].Position, player.Position) <= 100 && cars[i].Position.X == player.Position.X)
            {
                cars[i] = null;
                cars.RemoveAt(i);
                player.health -= 5;
            } else if (cars[i].Position.Y >= GraphicsDevice.Viewport.Height + cars[i].Texture.Height * cars[i].Scale.Y)
            {
                cars[i] = null;
                cars.RemoveAt(i);
                score += 50;
            }
        }
        
        for (int i = 0; i < items.Count; i++)
        {
            items[i].Update();
            if (Vector2.Distance(items[i].Position, player.Position) <= 100 && items[i].Position.X == player.Position.X)
            {
                items[i] = null;
                items.RemoveAt(i);
                player.health += 2;
            } else if (items[i].Position.Y >= GraphicsDevice.Viewport.Height + items[i].Texture.Height * items[i].Scale.Y)
            {
                items[i] = null;
                items.RemoveAt(i);
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();
        background.Draw(_spriteBatch);
        background2.Draw(_spriteBatch);
        for (int i = 0; i < cars.Count; i++)
        {
            cars[i].Draw(_spriteBatch);
        }
        for (int i = 0; i < items.Count; i++)
        {
            items[i].Draw(_spriteBatch);
        }
        player.Draw(_spriteBatch);
        if (!player.crashed)
        {
            _spriteBatch.DrawString(font, "Score: " + score, new Vector2(60, 30), Color.White);
        } else
        {
            _spriteBatch.DrawString(font, "Score: " + score, new Vector2(GraphicsDevice.Viewport.Width / 2 - 50, GraphicsDevice.Viewport.Height / 2 + 20), Color.White);
            _spriteBatch.DrawString(font, "GAME OVER", new Vector2(GraphicsDevice.Viewport.Width / 2 - 50, GraphicsDevice.Viewport.Height / 2 - 20), Color.Red);
        }
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void SpawnCar(Object obj)
    {
        while (!player.crashed)
        {
            Random random = new Random();
            Texture2D texture2D = null;

            switch (random.Next(1, 7))
            {
                case 1:
                    texture2D = Content.Load<Texture2D>("Ambulance");
                    break;
                case 2:
                    texture2D = Content.Load<Texture2D>("Audi");
                    break;
                case 3:
                    texture2D = Content.Load<Texture2D>("Car");
                    break;
                case 4:
                    texture2D = Content.Load<Texture2D>("taxi");
                    break;
                case 5:
                    texture2D = Content.Load<Texture2D>("Mini_van");
                    break;
                case 6:
                    texture2D = Content.Load<Texture2D>("Police");
                    break;
            }
            
            ObstacleCar car = new ObstacleCar(texture2D, player);
            cars.Add(car);
            
            Thread.Sleep(Math.Max(timeToSpawn -= 100, 500));
        }
    }

    private void SpawnItem(Object obj)
    {
        while (!player.crashed)
        {
            Fuel fuel = new Fuel(Content.Load<Texture2D>("gas"), cars);
            items.Add(fuel);
            Thread.Sleep(2000);
        }
    }
}

