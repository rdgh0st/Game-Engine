using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CPI311.GameEngine;
using CPI311.GameEngine.Physics;

namespace Lab6;

public class Lab06 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    BoxCollider boxCollider;
    SphereCollider sphere1, sphere2;
    Random random;
    List<Transform> transforms;
    List<RigidBody> rigidbodies;
    List<Collider> colliders;
    float numberCollisions;
    Camera camera;
    Transform cameraTransform;
    Model model;

    public Lab06()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        Time.Initialize();
        random = new Random();
        transforms = new List<Transform>();
        rigidbodies = new List<RigidBody>();
        colliders = new List<Collider>();
        boxCollider = new BoxCollider();
        boxCollider.Size = 10;
        cameraTransform = new Transform();
        cameraTransform.LocalPosition = Vector3.Backward * 20;
        camera = new Camera();
        camera.Transform = cameraTransform;
        for (int i =0; i < 2; i++){
            Transform transform = new Transform();
            transform.LocalPosition += Vector3.Right * 4 * i; //avoid overlapping each sphere 
            RigidBody rigidbody = new RigidBody();
            rigidbody.Transform = transform;
            rigidbody.Mass = 1; 
            
            Vector3 direction = new Vector3(
                (float)random.NextDouble(), (float)random.NextDouble(),       
                (float)random.NextDouble());
            direction.Normalize();
            rigidbody.Velocity =
                direction*((float)random.NextDouble()*5 + 5);
            SphereCollider sphereCollider = new SphereCollider();
            sphereCollider.Radius = 1.0f * transform.LocalScale.Y;
            sphereCollider.Transform = transform;
            transforms.Add(transform);
            colliders.Add(sphereCollider);
            rigidbodies.Add(rigidbody);
        }
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        model = Content.Load<Model>("Sphere");
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
        Time.Update(gameTime);

        foreach (RigidBody rigidbody in rigidbodies) rigidbody.Update();
        Vector3 normal; // it is updated if a collision happens
        for (int i = 0; i < transforms.Count; i++)
        {
            if (boxCollider.Collides(colliders[i], out normal))
            {
                numberCollisions++;
                if(Vector3.Dot(normal, rigidbodies[i].Velocity) <0)
                    rigidbodies[i].Impulse += 
                        Vector3.Dot(normal,rigidbodies[i].Velocity)*-2*normal;
            }
            for (int j = i + 1; j < transforms.Count; j++)
            {
                if (colliders[i].Collides(colliders[j], out normal))
                    numberCollisions++;
                Vector3 velocityNormal = Vector3.Dot(normal, 
                    rigidbodies[i].Velocity - rigidbodies[j].Velocity) * -2 * normal * rigidbodies[i].Mass * rigidbodies[j].Mass;
                rigidbodies[i].Impulse += velocityNormal / 2;
                rigidbodies[j].Impulse += -velocityNormal / 2;
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        foreach (Transform transform in transforms)
            model.Draw(transform.World, camera.View, camera.Projection);

        base.Draw(gameTime);
    }
}

