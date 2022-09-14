using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CPI311.GameEngine;

namespace Lab4;

public class Lab04 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    private Model parent;
    private Model model;
    private Transform parentTransform;
    private Transform modelTransform;
    private Transform cameraTransform;
    private Camera camera;

    public Lab04()
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
        parent = Content.Load<Model>("Sphere");
        modelTransform = new Transform();
        parentTransform = new Transform();
        cameraTransform = new Transform();
        cameraTransform.LocalPosition = Vector3.Backward * 5;
        camera = new Camera();
        camera.Transform = cameraTransform;

        modelTransform.Parent = parentTransform;
        modelTransform.LocalPosition += new Vector3(3, 0, 0);
        
        foreach (ModelMesh mesh in model.Meshes)
        {
            foreach (BasicEffect effect in mesh.Effects)
            {
                effect.EnableDefaultLighting();
                effect.PreferPerPixelLighting = true;
            }
        }
        foreach (ModelMesh mesh in parent.Meshes)
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
        
        if(InputManager.IsKeyDown(Keys.W))
            cameraTransform.LocalPosition += cameraTransform.Forward * Time.ElapsedGameTime;
        if(InputManager.IsKeyDown(Keys.S))
            cameraTransform.LocalPosition += cameraTransform.Backward * Time.ElapsedGameTime;
        if(InputManager.IsKeyDown(Keys.A))
            cameraTransform.Rotate(Vector3.Up, Time.ElapsedGameTime);
        if(InputManager.IsKeyDown(Keys.D))
            cameraTransform.Rotate(Vector3.Down, Time.ElapsedGameTime);
        
        if(InputManager.IsKeyDown(Keys.Up))
            parentTransform.LocalPosition += cameraTransform.Up * Time.ElapsedGameTime;
        if(InputManager.IsKeyDown(Keys.Down))
            parentTransform.LocalPosition += cameraTransform.Down * Time.ElapsedGameTime;
        if(InputManager.IsKeyDown(Keys.Left))
            parentTransform.LocalPosition += cameraTransform.Left * Time.ElapsedGameTime;
        if(InputManager.IsKeyDown(Keys.Right))
            parentTransform.LocalPosition += cameraTransform.Right * Time.ElapsedGameTime;
        
        parentTransform.Rotate(Vector3.Up, Time.ElapsedGameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        model.Draw(modelTransform.World, camera.View, camera.Projection);
        parent.Draw(parentTransform.World, camera.View, camera.Projection);

        base.Draw(gameTime);
    }
}

