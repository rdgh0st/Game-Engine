using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CPI311.GameEngine;
using CPI311.GameEngine.Physics;

namespace Assignment2;

public class Assignment02 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    SphereCollider sunCollider;
    Transform sunTransform;
    List<Transform> transforms;
    List<Renderer> renderers;
    List<Model> models;
    Light light;
    Camera defaultCamera, fpsCamera;
    Renderer sunRenderer, mercuryRenderer, earthRenderer, moonRenderer, planeRenderer;
    Transform cameraTransform, fpsTransform, mercuryTransform, earthTransform, moonTransform, planeTransform;
    Model sunModel, mercuryModel, earthModel, moonModel, planeModel;
    Texture2D sunTex, mercuryTex, earthTex, moonTex;
    float animSpeed = 1;

    public Assignment02()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        Time.Initialize();
        InputManager.Initialize();
        transforms = new List<Transform>();
        renderers = new List<Renderer>();
        models = new List<Model>();
        light = new Light();
        Transform lightTransform = new Transform();
        lightTransform.LocalPosition = Vector3.Backward * 10 + Vector3.Right * 5;
        light.Transform = lightTransform;
        cameraTransform = new Transform();
        cameraTransform.LocalPosition = Vector3.Up * 60;
        cameraTransform.Rotate(Vector3.Left, (float) (Math.PI / 2));
        defaultCamera = new Camera();
        defaultCamera.Transform = cameraTransform;
        fpsTransform = new Transform();
        fpsTransform.LocalPosition = Vector3.Backward * 30;
        fpsCamera = new Camera();
        fpsCamera.Transform = fpsTransform;
        sunTransform = new Transform();
        sunTransform.Scale = new Vector3(5, 5, 5);
        mercuryTransform = new Transform();
        mercuryTransform.Parent = sunTransform;
        mercuryTransform.Scale = new Vector3(2f, 2f, 2f);
        mercuryTransform.LocalPosition += new Vector3(4, 0, 0);
        earthTransform = new Transform();
        earthTransform.Parent = sunTransform;
        earthTransform.Scale = new Vector3(3f, 3f, 3f);
        earthTransform.LocalPosition += new Vector3(8, 0, 0);
        moonTransform = new Transform();
        moonTransform.Parent = earthTransform;
        moonTransform.Scale = new Vector3(1f, 1f, 1f);
        moonTransform.LocalPosition += new Vector3(3, 0, 0);
        planeTransform = new Transform();
        planeTransform.LocalPosition += new Vector3(0, -5, 0);
        planeTransform.Scale = new Vector3(10, 1, 10);
        transforms.Add(moonTransform);
        transforms.Add(earthTransform);
        transforms.Add(sunTransform);
        transforms.Add(mercuryTransform);
        transforms.Add(planeTransform);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        sunModel = Content.Load<Model>("Sphere");
        mercuryModel = Content.Load<Model>("Sphere");
        earthModel = Content.Load<Model>("Sphere");
        moonModel = Content.Load<Model>("Sphere");
        planeModel = Content.Load<Model>("Plane");
        sunTex = Content.Load<Texture2D>("sun");
        mercuryTex = Content.Load<Texture2D>("mercury");
        earthTex = Content.Load<Texture2D>("earth");
        moonTex = Content.Load<Texture2D>("moon");
        sunRenderer = new Renderer(sunModel, sunTransform, defaultCamera, Content, GraphicsDevice, light, "Shader", 1, 20, sunTex);
        mercuryRenderer = new Renderer(mercuryModel, mercuryTransform, defaultCamera, Content, GraphicsDevice, light, "Shader", 1, 20,
            mercuryTex);
        earthRenderer = new Renderer(earthModel, earthTransform, defaultCamera, Content, GraphicsDevice, light, "Shader", 1, 20,
            earthTex);
        moonRenderer = new Renderer(moonModel, moonTransform, defaultCamera, Content, GraphicsDevice, light, "Shader", 1, 20,
            moonTex);
        planeRenderer = new Renderer(sunModel, fpsTransform, defaultCamera, Content, GraphicsDevice, light,
            "Shader", 2, 20,
            null);
        renderers.Add(sunRenderer);
        renderers.Add(mercuryRenderer);
        renderers.Add(earthRenderer);
        renderers.Add(moonRenderer); 
        renderers.Add(planeRenderer);
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        Time.Update(gameTime);
        InputManager.Update();

        Camera activeCam = renderers[0].Camera;

        if (InputManager.IsKeyPressed(Keys.Tab))
        {
            if (activeCam == defaultCamera)
            {
                for (int i = 0; i < renderers.Count; i++) renderers[i].Camera = fpsCamera;
            }
            else
            {
                for (int i = 0; i < renderers.Count; i++) renderers[i].Camera = defaultCamera;
            }
        }

        if (activeCam == fpsCamera)
        {
            if (InputManager.IsKeyDown(Keys.W))
                activeCam.Transform.LocalPosition +=
                    new Vector3((activeCam.Transform.Forward * Time.ElapsedGameTime * 5).X, 0,
                        (activeCam.Transform.Forward * Time.ElapsedGameTime * 5).Z);
            if (InputManager.IsKeyDown(Keys.S))
                activeCam.Transform.LocalPosition +=
                    new Vector3((activeCam.Transform.Backward * Time.ElapsedGameTime * 5).X, 0,
                        (activeCam.Transform.Backward * Time.ElapsedGameTime * 5).Z);
            if (InputManager.IsKeyDown(Keys.A))
                activeCam.Transform.LocalPosition +=
                    new Vector3((activeCam.Transform.Left * Time.ElapsedGameTime * 5).X, 0,
                        (activeCam.Transform.Left * Time.ElapsedGameTime * 5).Z);
            if (InputManager.IsKeyDown(Keys.D))
                activeCam.Transform.LocalPosition +=
                    new Vector3((activeCam.Transform.Right * Time.ElapsedGameTime * 5).X, 0,
                        (activeCam.Transform.Right * Time.ElapsedGameTime * 5).Z);
        }
        else
        {
            if (InputManager.IsKeyDown(Keys.W))
                activeCam.Transform.LocalPosition +=
                    new Vector3((activeCam.Transform.Forward * Time.ElapsedGameTime * 5).X, 
                        (activeCam.Transform.Forward * Time.ElapsedGameTime * 5).Y, 0);
            if (InputManager.IsKeyDown(Keys.S))
                activeCam.Transform.LocalPosition +=
                    new Vector3((activeCam.Transform.Backward * Time.ElapsedGameTime * 5).X, 
                        (activeCam.Transform.Backward * Time.ElapsedGameTime * 5).Y, 0);
            if (InputManager.IsKeyDown(Keys.A))
                activeCam.Transform.LocalPosition +=
                    new Vector3((activeCam.Transform.Left * Time.ElapsedGameTime * 5).X, 
                        (activeCam.Transform.Left * Time.ElapsedGameTime * 5).Y, 0);
            if (InputManager.IsKeyDown(Keys.D))
                activeCam.Transform.LocalPosition +=
                    new Vector3((activeCam.Transform.Right * Time.ElapsedGameTime * 5).X, 
                        (activeCam.Transform.Right * Time.ElapsedGameTime * 5).Y, 0);
        }

        if(InputManager.IsKeyDown(Keys.Left))
            activeCam.Transform.Rotate(Vector3.Up, Time.ElapsedGameTime);
        if(InputManager.IsKeyDown(Keys.Right))
            activeCam.Transform.Rotate(Vector3.Down, Time.ElapsedGameTime);
        if(InputManager.IsKeyDown(Keys.Up))
            activeCam.Transform.Rotate(Vector3.Right, Time.ElapsedGameTime);
        if(InputManager.IsKeyDown(Keys.Down))
            activeCam.Transform.Rotate(Vector3.Left, Time.ElapsedGameTime);

        if (InputManager.IsKeyDown(Keys.OemPlus))
            animSpeed += 0.25f;
        if (InputManager.IsKeyDown(Keys.OemMinus))
            animSpeed -= 0.25f;
        if (InputManager.IsKeyDown(Keys.P))
            activeCam.FieldOfView -= 0.01f;
        if (InputManager.IsKeyDown(Keys.O))
            activeCam.FieldOfView += 0.01f;

        if (InputManager.getMouseState().X - InputManager.getPrevMouseState().X > 0.5f)
        {
            activeCam.Transform.Rotate(Vector3.Down, MathHelper.ToRadians(1));
        } else if (InputManager.getMouseState().X - InputManager.getPrevMouseState().X < -0.5f)
        {
            activeCam.Transform.Rotate(Vector3.Up, MathHelper.ToRadians(1));
        }
        
        if (InputManager.getMouseState().Y - InputManager.getPrevMouseState().Y > 0.5f)
        {
            activeCam.Transform.Rotate(Vector3.Left, MathHelper.ToRadians(1));
        } else if (InputManager.getMouseState().Y - InputManager.getPrevMouseState().Y < -0.5f)
        {
            activeCam.Transform.Rotate(Vector3.Right, MathHelper.ToRadians(1));
        }

        if (InputManager.getMouseState().LeftButton == ButtonState.Pressed && activeCam == fpsCamera)
        {
            activeCam.Transform.LocalPosition += new Vector3((activeCam.Transform.Forward * Time.ElapsedGameTime * 5).X,
                0, (activeCam.Transform.Forward * Time.ElapsedGameTime * 5).Z);
        } else if (InputManager.getMouseState().LeftButton == ButtonState.Pressed && activeCam == defaultCamera)
        {
            activeCam.Transform.LocalPosition += new Vector3((activeCam.Transform.Forward * Time.ElapsedGameTime * 5).X,
                (activeCam.Transform.Forward * Time.ElapsedGameTime * 5).Y, 0);
        }

        sunTransform.Rotate(Vector3.Up, Time.ElapsedGameTime * animSpeed);
        earthTransform.Rotate(Vector3.Up, Time.ElapsedGameTime * animSpeed);
        moonTransform.Rotate(Vector3.Up, Time.ElapsedGameTime * 2 * animSpeed);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        for (int i= 0; i<renderers.Count; i++) renderers[i].Draw();

        planeModel.Draw(planeTransform.World, renderers[0].Camera.View, renderers[0].Camera.Projection);

        base.Draw(gameTime);
    }
}

