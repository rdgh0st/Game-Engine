using System.Security.Cryptography;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CPI311.GameEngine;
using CPI311.GameEngine.Physics;
using CPI311.GameEngine.GUI;

namespace Assignment5;

public class Assignment05 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private TerrainRenderer terrain;
    private Effect effect;
    private Camera camera;
    private Light light;

    public Assignment05()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        Time.Initialize();
        InputManager.Initialize();
        ScreenManager.Initialize(_graphics);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        terrain = new TerrainRenderer(Content.Load<Texture2D>("mazeH"), Vector2.One * 100, Vector2.One * 200);
        terrain.NormalMap = Content.Load<Texture2D>("mazeN");
        float height = terrain.GetHeight(new Vector2(0.5f, 0.5f));
        terrain.Transform = new Transform();
        terrain.Transform.LocalScale *= new Vector3(1, 5, 1);

        effect = Content.Load<Effect>("TerrainShader");
        effect.Parameters["AmbientColor"].SetValue(new Vector3(0.1f, 0.1f, 0.1f));
        effect.Parameters["DiffuseColor"].SetValue(new Vector3(0.3f, 0.1f, 0.1f));
        effect.Parameters["SpecularColor"].SetValue(new Vector3(0, 0, 0.2f));
        effect.Parameters["Shininess"].SetValue(20f);
        effect.Parameters["NormalMap"].SetValue(terrain.NormalMap);
        camera = new Camera();
        camera.Transform = new Transform();
        camera.Transform.LocalPosition = Vector3.Up * 50;
        camera.Transform.Rotate(Vector3.Left, MathHelper.PiOver2);

        light = new Light();
        light.Transform = new Transform();
        //light.Transform.LocalPosition = Vector3.Backward * 5 + Vector3.Right * 5 + Vector3.Up * 20;
        light.Transform.Position = new Vector3(0, 20, 15);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        Time.Update(gameTime);
        InputManager.Update();
        
        if (InputManager.IsKeyDown(Keys.Up)) camera.Transform.Rotate(Vector3.Left, Time.ElapsedGameTime);
        if (InputManager.IsKeyDown(Keys.Down)) camera.Transform.Rotate(Vector3.Right, Time.ElapsedGameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        effect.Parameters["View"].SetValue(camera.View);
        effect.Parameters["Projection"].SetValue(camera.Projection);
        effect.Parameters["World"].SetValue(terrain.Transform.World);
        effect.Parameters["CameraPosition"].SetValue(camera.Transform.Position);
        effect.Parameters["LightPosition"].SetValue(light.Transform.Position);
        effect.Parameters["NormalMap"].SetValue(terrain.NormalMap);
        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
        {     
            pass.Apply();
            terrain.Draw();
        }

        base.Draw(gameTime);
    }
}

