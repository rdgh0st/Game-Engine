using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CPI311.GameEngine;

namespace Lab10;

public class Lab10 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    TerrainRenderer terrain;
    private Effect effect;
    private Camera camera;

    public Lab10()
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

        terrain = new TerrainRenderer(
            Content.Load<Texture2D>("Heightmap"),
            Vector2.One * 100, Vector2.One * 200);
        terrain.NormalMap = Content.Load<Texture2D>("Normalmap");
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
        camera.Transform.LocalPosition = Vector3.Backward * 5 + Vector3.Right * 3;
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        InputManager.Update();
        Time.Update(gameTime);

        if (InputManager.IsKeyDown(Keys.W)) 
            camera.Transform.LocalPosition += camera.Transform.Forward * Time.ElapsedGameTime;
        if (InputManager.IsKeyDown(Keys.S)) 
            camera.Transform.LocalPosition += camera.Transform.Backward * Time.ElapsedGameTime;
        if (InputManager.IsKeyDown(Keys.A)) 
            camera.Transform.LocalPosition += camera.Transform.Left * Time.ElapsedGameTime;
        if (InputManager.IsKeyDown(Keys.D)) 
            camera.Transform.LocalPosition += camera.Transform.Right * Time.ElapsedGameTime;

        if (InputManager.IsKeyDown(Keys.Right))
            camera.Transform.Rotate(Vector3.Up, MathHelper.ToRadians(10 * -Time.ElapsedGameTime));
        if (InputManager.IsKeyDown(Keys.Left))
            camera.Transform.Rotate(Vector3.Up, MathHelper.ToRadians(10 * Time.ElapsedGameTime));
        
        camera.Transform.LocalPosition = new Vector3(
            camera.Transform.LocalPosition.X,
            terrain.GetAltitude(camera.Transform.LocalPosition),
            camera.Transform.LocalPosition.Z) + Vector3.Up;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        effect.Parameters["View"].SetValue(camera.View);
        effect.Parameters["Projection"].SetValue(camera.Projection);
        effect.Parameters["World"].SetValue(terrain.Transform.World);
        effect.Parameters["CameraPosition"].SetValue(camera.Transform.Position);
        effect.Parameters["LightPosition"].SetValue(camera.Transform.Position * (Vector3.Up * 10));
        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
        {     
            pass.Apply();
            terrain.Draw();
        }

        base.Draw(gameTime);
    }
}

