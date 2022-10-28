using System;
using System.Collections.Generic;
using CPI311.GameEngine;
using CPI311.GameEngine.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Assignment3;

public class Assignment03 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    Camera camera;
    Light light;
    BoxCollider boxCollider;
    List<GameObject> gameObjects;
    Random random;
    Model objModel;
    Texture2D texture;
    int numberCollisions;
    bool colorChange = false;

    public Assignment03()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        Time.Initialize();
        InputManager.Initialize();
        random = new Random();
        light = new Light();
        Transform lightTransform = new Transform();
        lightTransform.LocalPosition = Vector3.Backward * 10 + Vector3.Right * 5;
        light.Transform = lightTransform;
        boxCollider = new BoxCollider();
        boxCollider.Size = 10;
        Transform cameraTransform = new Transform();
        cameraTransform.LocalPosition = Vector3.Backward * 20;
        camera = new Camera();
        camera.Transform = cameraTransform;
        gameObjects = new List<GameObject>();

        objModel = Content.Load<Model>("Sphere");
        texture = Content.Load<Texture2D>("Square");
        AddGameObject();
        AddGameObject();
        AddGameObject();
        AddGameObject();
        AddGameObject();
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        Time.Update(gameTime);
        InputManager.Update();

        if (InputManager.IsKeyPressed(Keys.Up))
        {
            AddGameObject();
        }

        if (InputManager.IsKeyPressed(Keys.Down))
        {
            gameObjects.RemoveAt(gameObjects.Count - 1);
        }
        
        if (InputManager.IsKeyPressed(Keys.LeftAlt) || InputManager.IsKeyPressed(Keys.RightAlt))
        {
            if (gameObjects[0].Renderer.Material.CurrentTechnique != 1)
            {
                foreach (GameObject gameObject in gameObjects)
                    gameObject.Renderer.Material.CurrentTechnique = 1;
            }
            else
            {
                foreach (GameObject gameObject in gameObjects)
                    gameObject.Renderer.Material.CurrentTechnique = 2;
            }
        }

        if (InputManager.IsKeyPressed(Keys.Space))
        {
            colorChange = !colorChange;
        }

        Vector3 normal;
        for (int i = 0; i < gameObjects.Count; i++)
        {
            if (boxCollider.Collides(gameObjects[i].Get<Collider>(), out normal))
            {
                numberCollisions++;
                if(Vector3.Dot(normal, gameObjects[i].Get<RigidBody>().Velocity) <0)
                    gameObjects[i].Get<RigidBody>().Impulse += 
                        Vector3.Dot(normal,gameObjects[i].Get<RigidBody>().Velocity)*-2*normal;
            }
            for (int j = i + 1; j < gameObjects.Count; j++)
            {
                if (gameObjects[i].Get<Collider>().Collides(gameObjects[j].Get<Collider>(), out normal))
                    numberCollisions++;
                Vector3 velocityNormal = Vector3.Dot(normal, gameObjects[i].Get<RigidBody>().Velocity - gameObjects[j].Get<RigidBody>().Velocity) * -2 * normal * gameObjects[i].Get<RigidBody>().Mass * gameObjects[j].Get<RigidBody>().Mass;
                gameObjects[i].Get<RigidBody>().Impulse += velocityNormal / 2;
                gameObjects[j].Get<RigidBody>().Impulse += -velocityNormal / 2;
            }
        }

        foreach (GameObject gameObject in gameObjects)
            gameObject.Update();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        if (colorChange)
        {
            foreach (var gameObject in gameObjects)
            {
                float speed = gameObject.Rigidbody.Velocity.Length();
                float speedValue = MathHelper.Clamp(speed / 20f, 0, 1);
                gameObject.Renderer.Material.Diffuse = 
                    new Vector3(speedValue, speedValue, 1);
                gameObject.Draw();
            }
        }
        else
        {
            foreach (GameObject gameObject in gameObjects)
            {
                gameObject.Renderer.Material.Diffuse = Color.White.ToVector3();
                gameObject.Draw();
            }
        }

        base.Draw(gameTime);
    }

    private void AddGameObject()
    {
        GameObject gameObject = new GameObject();
        gameObject.Transform.LocalPosition += Vector3.Right * 10 * (float)random.NextDouble(); //avoid overlapping each sphere
        
        RigidBody rigidbody = new RigidBody();
        rigidbody.Mass = random.Next(1, 3);
        Vector3 direction = new Vector3(
            (float)random.NextDouble(), (float)random.NextDouble(),       
            (float)random.NextDouble());
        direction.Normalize();
        rigidbody.Velocity =
            direction*((float)random.NextDouble()*5 + 5); 
        gameObject.Add<RigidBody>(rigidbody);
        
        SphereCollider sphereCollider = new SphereCollider();
        sphereCollider.Radius = 1.0f * gameObject.Transform.LocalScale.Y;
        gameObject.Add<Collider> (sphereCollider);
        
        Renderer renderer = new Renderer(objModel, gameObject.Transform, camera, Content, GraphicsDevice, light, "Shader", 1, 20f, texture);
        gameObject.Add<Renderer>(renderer);
        
        gameObjects.Add(gameObject);
    }
}

