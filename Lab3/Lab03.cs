using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CPI311.GameEngine;

namespace Lab3;

public class Lab03 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Model model;
    private Matrix world;
    private Matrix view;
    private Matrix proj;
    private Vector3 cameraPosition = new Vector3(0, 0, 5);
    private Vector3 modelPosition = new Vector3(0, 0, 0);

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
            cameraPosition += Vector3.Up * Time.ElapsedGameTime * 5;
        }
        if (InputManager.IsKeyDown(Keys.S))
        {
            cameraPosition += Vector3.Down * Time.ElapsedGameTime * 5;
        }
        if (InputManager.IsKeyDown(Keys.A))
        {
            cameraPosition += Vector3.Left * Time.ElapsedGameTime * 5;
        }
        if (InputManager.IsKeyDown(Keys.D))
        {
            cameraPosition += Vector3.Right * Time.ElapsedGameTime * 5;
        }
        if (InputManager.IsKeyDown(Keys.Up))
        {
            modelPosition += Vector3.Up * Time.ElapsedGameTime * 5;
        }
        if (InputManager.IsKeyDown(Keys.Down))
        {
            modelPosition += Vector3.Down * Time.ElapsedGameTime * 5;
        }
        if (InputManager.IsKeyDown(Keys.Left))
        {
            modelPosition += Vector3.Left * Time.ElapsedGameTime * 5;
        }
        if (InputManager.IsKeyDown(Keys.Right))
        {
            modelPosition += Vector3.Right * Time.ElapsedGameTime * 5;
        }

        world = Matrix.CreateScale(1.0f) * Matrix.CreateFromYawPitchRoll(0, 0, 0) *
                Matrix.CreateTranslation(modelPosition);
        view = Matrix.CreateLookAt(
            cameraPosition,
            new Vector3(0, 0, -1),
            new Vector3(0, 1, 0));
        proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver2, 1.33f, 0.1f, 1000f);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        model.Draw(world, view, proj);

        base.Draw(gameTime);
    }
}

