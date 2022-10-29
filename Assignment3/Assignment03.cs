using System;
using System.Collections.Generic;
using System.Threading;
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
    private bool showDiag = true;
    private float currentTimeFactor = 1;
    private int currentTechnique = 1;
    private SpriteFont font;
    bool haveThreadRunning = true;
    int lastSecondCollision = 0;
    private float framesPerSecond;
    private float totalFrames;
    private bool collisionThread = true;

    public Assignment03()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
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
        
        ThreadPool.QueueUserWorkItem(new WaitCallback(CollisionReset));
        ThreadPool.QueueUserWorkItem(new WaitCallback(CollisionCheck));

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        

        objModel = Content.Load<Model>("Sphere");
        texture = Content.Load<Texture2D>("Square");
        font = Content.Load<SpriteFont>("font");
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

        if (InputManager.IsKeyPressed(Keys.Right))
        {
            foreach (GameObject gameObject in gameObjects)
            {
                gameObject.Rigidbody.TimeFactor *= 2;
                currentTimeFactor = gameObject.Rigidbody.TimeFactor;
            }
        }
        
        if (InputManager.IsKeyPressed(Keys.Left))
        {
            foreach (GameObject gameObject in gameObjects)
            {
                gameObject.Rigidbody.TimeFactor /= 2;
                currentTimeFactor = gameObject.Rigidbody.TimeFactor;
            }
        }

        if (InputManager.IsKeyPressed(Keys.LeftShift) || InputManager.IsKeyPressed(Keys.RightShift))
        {
            showDiag = !showDiag;
        }
        
        if (InputManager.IsKeyPressed(Keys.LeftAlt) || InputManager.IsKeyPressed(Keys.RightAlt))
        {
            if (gameObjects[0].Renderer.Material.CurrentTechnique != 1)
            {
                foreach (GameObject gameObject in gameObjects)
                    gameObject.Renderer.Material.CurrentTechnique = 1;
                currentTechnique = 1;
            }
            else
            {
                foreach (GameObject gameObject in gameObjects)
                    gameObject.Renderer.Material.CurrentTechnique = 2;
                currentTechnique = 2;
            }
        }

        if (InputManager.IsKeyPressed(Keys.Space))
        {
            colorChange = !colorChange;
        }

        if (InputManager.IsKeyPressed(Keys.Tab))
        {
            collisionThread = !collisionThread;
            if (collisionThread)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(CollisionCheck));
            }
        }

        if (!collisionThread)
        {
            Vector3 normal;
            for (int i = 0; i < gameObjects.Count; i++)
            {
                if (boxCollider.Collides(gameObjects[i].Get<Collider>(), out normal))
                {
                    numberCollisions++;
                    if (Vector3.Dot(normal, gameObjects[i].Get<RigidBody>().Velocity) < 0)
                        gameObjects[i].Get<RigidBody>().Impulse +=
                            Vector3.Dot(normal, gameObjects[i].Get<RigidBody>().Velocity) * -2 * normal;
                }

                for (int j = i + 1; j < gameObjects.Count; j++)
                {
                    if (gameObjects[i].Get<Collider>().Collides(gameObjects[j].Get<Collider>(), out normal))
                        numberCollisions++;
                    Vector3 velocityNormal =
                        Vector3.Dot(normal,
                            gameObjects[i].Get<RigidBody>().Velocity - gameObjects[j].Get<RigidBody>().Velocity) * -2 *
                        normal * gameObjects[i].Get<RigidBody>().Mass * gameObjects[j].Get<RigidBody>().Mass;
                    gameObjects[i].Get<RigidBody>().Impulse += velocityNormal / 2;
                    gameObjects[j].Get<RigidBody>().Impulse += -velocityNormal / 2;
                }
            }
            foreach (GameObject gameObject in gameObjects)
                gameObject.Update();
        }


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

        if (showDiag)
        {
            _spriteBatch.Begin();
            _spriteBatch.DrawString(font, "Number of Balls: " + gameObjects.Count, new Vector2(20, 20), Color.White);
            _spriteBatch.DrawString(font, "Number of Collisions in last second: " + lastSecondCollision,
                new Vector2(20, 40), Color.White);
            _spriteBatch.DrawString(font, "Frames Per Second: " + framesPerSecond, new Vector2(20, 60), Color.White);
            _spriteBatch.DrawString(font, "Thread Collision (Tab) Enabled: " + collisionThread, new Vector2(20, 80), Color.White);
            _spriteBatch.End();
        }

        totalFrames++;
        base.Draw(gameTime);
    }

    private void AddGameObject()
    {
        GameObject gameObject = new GameObject();
        gameObject.Transform.LocalPosition += Vector3.Right * 10 * (float)random.NextDouble(); //avoid overlapping each sphere
        
        RigidBody rigidbody = new RigidBody();
        rigidbody.Mass = random.NextSingle() + 1;
        Vector3 direction = new Vector3(
            (float)random.NextDouble(), (float)random.NextDouble(),       
            (float)random.NextDouble());
        direction.Normalize();
        rigidbody.Velocity =
            direction*((float)random.NextDouble()*5 + 5);
        rigidbody.TimeFactor = currentTimeFactor;
        gameObject.Add<RigidBody>(rigidbody);
        
        SphereCollider sphereCollider = new SphereCollider();
        sphereCollider.Radius = 1.0f * gameObject.Transform.LocalScale.Y;
        gameObject.Add<Collider> (sphereCollider);
        
        Renderer renderer = new Renderer(objModel, gameObject.Transform, camera, Content, GraphicsDevice, light, "Shader", currentTechnique, 20f, texture);
        gameObject.Add<Renderer>(renderer);
        
        gameObjects.Add(gameObject);
    }
    
    private void CollisionReset(Object obj)
    {
        while (haveThreadRunning)
        {
            lastSecondCollision = numberCollisions;
            numberCollisions = 0;
            framesPerSecond = totalFrames;
            totalFrames = 0;
            System.Threading.Thread.Sleep(1000);
        }
    }

    private void CollisionCheck(Object obj)
    {
        while (collisionThread)
        {
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

            try
            {
                foreach (GameObject gameObject in gameObjects)
                    gameObject.Update();
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e);
                for (int i = 0; i < gameObjects.Count; i++)
                {
                    gameObjects[i].Update();
                }
            }
            Thread.Sleep(16);
        }
    }
}

