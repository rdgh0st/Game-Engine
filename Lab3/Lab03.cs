using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CPI311.GameEngine;

namespace Lab3;

public class Lab03 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont font;
    private Model model;
    private Matrix world;
    private Matrix view;
    private Matrix proj;
    private Vector3 cameraPosition = new Vector3(0, 0, 5);
    private Vector3 modelPosition = new Vector3(0, 0, 0);
    private float yaw, pitch, roll;
    private float scale = 1.0f;
    private bool orderSRT = true;
    private bool perspectiveActive = true;
    private float left = -1f, right = 1f, bottom = -1.33f, top = 1.33f;

    public Lab03()
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
        font = Content.Load<SpriteFont>("font");
        model = Content.Load<Model>("Torus");
        foreach (ModelMesh mesh in model.Meshes)
        {
            foreach (BasicEffect effect in mesh.Effects)
            {
                effect.EnableDefaultLighting();
                effect.PreferPerPixelLighting = true;
            }
        }

    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        InputManager.Update();
        Time.Update(gameTime);

        if (InputManager.IsKeyDown(Keys.W))
        {
            if (InputManager.IsKeyDown(Keys.LeftShift))
            {
                top += 1;
                bottom += 1;
            } else if (InputManager.IsKeyDown(Keys.LeftControl))
            {
                top += 1;
            }
            else
            {
                cameraPosition += Vector3.Up * Time.ElapsedGameTime * 5;
            }
        }
        if (InputManager.IsKeyDown(Keys.S))
        {
            if (InputManager.IsKeyDown(Keys.LeftShift))
            {
                top -= 1;
                bottom -= 1;
            } else if (InputManager.IsKeyDown(Keys.LeftControl))
            {
                bottom -= 1;
            }
            else
            {
                cameraPosition += Vector3.Down * Time.ElapsedGameTime * 5;
            }
        }
        if (InputManager.IsKeyDown(Keys.A))
        {
            if (InputManager.IsKeyDown(Keys.LeftShift))
            {
                left -= 1;
                right -= 1;
            } else if (InputManager.IsKeyDown(Keys.LeftControl))
            {
                left -= 1;
            }
            else
            {
                cameraPosition += Vector3.Left * Time.ElapsedGameTime * 5;
            }
        }
        if (InputManager.IsKeyDown(Keys.D))
        {
            if (InputManager.IsKeyDown(Keys.LeftShift))
            {
                left += 1;
                right += 1;
            } else if (InputManager.IsKeyDown(Keys.LeftControl))
            {
                right += 1;
            }
            else
            {
                cameraPosition += Vector3.Right * Time.ElapsedGameTime * 5;
            }
        }
        if (InputManager.IsKeyDown(Keys.Up))
        {
            if (InputManager.IsKeyDown(Keys.LeftShift))
            {
                scale += 1;
            }
            else
            {
                modelPosition += Vector3.Up * Time.ElapsedGameTime * 5;
            }
        }
        if (InputManager.IsKeyDown(Keys.Down))
        {
            if (InputManager.IsKeyDown(Keys.LeftShift))
            {
                scale -= 1;
            }
            else
            {
                modelPosition += Vector3.Down * Time.ElapsedGameTime * 5;
            }
        }
        if (InputManager.IsKeyDown(Keys.Left))
        {
            modelPosition += Vector3.Left * Time.ElapsedGameTime * 5;
        }
        if (InputManager.IsKeyDown(Keys.Right))
        {
            modelPosition += Vector3.Right * Time.ElapsedGameTime * 5;
        }
        if (InputManager.IsKeyDown(Keys.Insert))
        {
            yaw += 1;
        }
        if (InputManager.IsKeyDown(Keys.Delete))
        {
            yaw -= 1;
        }
        if (InputManager.IsKeyDown(Keys.Home))
        {
            pitch += 1;
        }
        if (InputManager.IsKeyDown(Keys.End))
        {
            pitch -= 1;
        }
        if (InputManager.IsKeyDown(Keys.PageUp))
        {
            roll += 1;
        }
        if (InputManager.IsKeyDown(Keys.PageDown))
        {
            roll -= 1;
        }
        if (InputManager.IsKeyPressed(Keys.Space))
        {
            orderSRT = !orderSRT;
        }
        if (InputManager.IsKeyPressed(Keys.Tab))
        {
            perspectiveActive = !perspectiveActive;
        }

        if (orderSRT)
        {
            world = Matrix.CreateScale(scale) * Matrix.CreateFromYawPitchRoll(yaw, pitch, roll) *
                    Matrix.CreateTranslation(modelPosition);
        }
        else
        {
            world = Matrix.CreateTranslation(modelPosition) * Matrix.CreateFromYawPitchRoll(yaw, pitch, roll) *
                    Matrix.CreateScale(scale);
        }
        view = Matrix.CreateLookAt(
            cameraPosition,
            new Vector3(0, 0, -1),
            new Vector3(0, 1, 0));
        //proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1.33f, 0.1f, 1000f);
        proj = perspectiveActive ? Matrix.CreatePerspectiveOffCenter(left, right, bottom, top, 0.1f, 1000f) : Matrix.CreateOrthographicOffCenter(left, right, bottom, top, 0.1f, 1000f);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        model.Draw(world, view, proj);
        _spriteBatch.Begin();
        _spriteBatch.DrawString(font, "Arrow Keys to move obj", new Vector2(10, GraphicsDevice.Viewport.Height - 20), Color.Black);
        _spriteBatch.DrawString(font, "yaw (Ins/Del): " + yaw, new Vector2(10, GraphicsDevice.Viewport.Height - 40), Color.Black);
        _spriteBatch.DrawString(font, "pitch (Home/End): " + pitch, new Vector2(10, GraphicsDevice.Viewport.Height - 60), Color.Black);
        _spriteBatch.DrawString(font, "roll (PgUp/PgDn): " + roll, new Vector2(10, GraphicsDevice.Viewport.Height - 80), Color.Black);
        _spriteBatch.DrawString(font, "scale (Shift + Up/Dn): " + scale, new Vector2(10, GraphicsDevice.Viewport.Height - 100), Color.Black);
        string multOrder = orderSRT ? "Scale, Rotation, Translation" : "Translation, Rotation, Scale";
        _spriteBatch.DrawString(font, "Multiplication order (Space): " + multOrder, new Vector2(10, GraphicsDevice.Viewport.Height - 120), Color.Black);
        _spriteBatch.DrawString(font, "WASD to move camera", new Vector2(10, GraphicsDevice.Viewport.Height - 140), Color.Black);
        string persOrOrth = perspectiveActive ? "Perspective" : "Orthogonal";
        _spriteBatch.DrawString(font, "Projection Type (Tab): " + persOrOrth, new Vector2(10, GraphicsDevice.Viewport.Height - 160), Color.Black);
        _spriteBatch.DrawString(font, "left (Ctrl + A): " + left, new Vector2(10, GraphicsDevice.Viewport.Height - 180), Color.Black);
        _spriteBatch.DrawString(font, "right (Ctrl + D): " + right, new Vector2(10, GraphicsDevice.Viewport.Height - 200), Color.Black);
        _spriteBatch.DrawString(font, "top (Ctrl + W): " + top, new Vector2(10, GraphicsDevice.Viewport.Height - 220), Color.Black);
        _spriteBatch.DrawString(font, "bottom (Ctrl + S): " + bottom, new Vector2(10, GraphicsDevice.Viewport.Height - 240), Color.Black);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}

