using System.Collections.Generic;
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
    private Camera camera2;
    private Light light;
    private A5Player player;
    private Agent agent;
    private Agent agent2;
    private Agent agent3;
    private List<Camera> cameras;
    private List<Agent> _agents;
    private int score;
    private SpriteFont font;

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

        cameras = new List<Camera>();
        _agents = new List<Agent>();
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        font = Content.Load<SpriteFont>("font");
        ScreenManager.Setup(false, 1080, 720);
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
        camera.Position = new Vector2(0f, 0f);
        camera.Size = new Vector2(0.5f, 1f);
        camera.Transform = new Transform();
        camera.Transform.LocalPosition = Vector3.Up * 50;
        camera.Transform.Rotate(Vector3.Left, MathHelper.PiOver2);
        camera2 = new Camera();
        camera2.Position = new Vector2(0.5f, 0f);
        camera2.Size = new Vector2(0.5f, 1f);
        camera2.Transform = new Transform();
        camera2.Transform.LocalPosition = Vector3.Up * 50;
        camera2.Transform.Rotate(Vector3.Left, MathHelper.PiOver2);
        
        cameras.Add(camera);
        cameras.Add(camera2);

        light = new Light();
        light.Transform = new Transform();
        //light.Transform.LocalPosition = Vector3.Backward * 5 + Vector3.Right * 5 + Vector3.Up * 20;
        light.Transform.Position = new Vector3(0, 20, 15);

        player = new A5Player(terrain, Content, camera, GraphicsDevice, light);
        agent = new Agent(terrain, Content, camera, GraphicsDevice, light);
        agent2 = new Agent(terrain, Content, camera, GraphicsDevice, light);
        agent3 = new Agent(terrain, Content, camera, GraphicsDevice, light);
        _agents.Add(agent);
        _agents.Add(agent2);
        _agents.Add(agent3);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        Time.Update(gameTime);
        InputManager.Update();
        
        if (InputManager.IsKeyDown(Keys.Up)) camera.Transform.Rotate(Vector3.Left, Time.ElapsedGameTime);
        if (InputManager.IsKeyDown(Keys.Down)) camera.Transform.Rotate(Vector3.Right, Time.ElapsedGameTime);

        player.Update();
        agent.Update();
        agent2.Update();
        agent3.Update();

        foreach (Agent agent in _agents)
        {
            Vector3 normal;
            if (agent.Collider.Collides(player.Collider, out normal))
            {
                agent.path = null;
                score++;
            }
        }
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        foreach (Camera camera in cameras)
        {
            GraphicsDevice.DepthStencilState = new DepthStencilState();
            GraphicsDevice.Viewport = camera.Viewport;
            Matrix view = camera.View;
            Matrix projection = camera.Projection;
            effect.Parameters["View"].SetValue(view);
            effect.Parameters["Projection"].SetValue(projection);
            effect.Parameters["World"].SetValue(terrain.Transform.World);
            effect.Parameters["LightPosition"].SetValue(light.Transform.Position);
            effect.Parameters["CameraPosition"].SetValue(camera.Transform.Position);
            effect.Parameters["Shininess"].SetValue(20f);
            effect.Parameters["NormalMap"].SetValue(terrain.NormalMap);
            effect.Parameters["AmbientColor"].SetValue(new Vector3(0.1f, 0.1f, 0.1f));
            effect.Parameters["DiffuseColor"].SetValue(new Vector3(0.1f, 0.1f, 0.1f));
            effect.Parameters["SpecularColor"].SetValue(new Vector3(0.8f, 0.8f, 0.8f));
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                terrain.Draw();

                player.Draw();
                agent.Draw();
                agent2.Draw();
                agent3.Draw();
            }
        }
        
        _spriteBatch.Begin();
        _spriteBatch.DrawString(font, "Time: " + Time.TotalGameTime, new Vector2(30, 30), Color.White);
        _spriteBatch.DrawString(font, "Score: " + score, new Vector2(30, 60), Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}

