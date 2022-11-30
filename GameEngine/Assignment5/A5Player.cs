using System;
using CPI311.GameEngine.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CPI311.GameEngine;

public class A5Player : GameObject
{
    public TerrainRenderer Terrain { get; set; }
    public A5Player(TerrainRenderer terrain, ContentManager Content, Camera camera, 
        GraphicsDevice graphicsDevice, Light light ) :base()
    {
        Terrain = terrain;
            
        RigidBody rigidbody = new RigidBody();
        rigidbody.Transform = Transform;
        rigidbody.Mass = 1;
        Add<RigidBody>(rigidbody);

        SphereCollider sphereCollider = new SphereCollider();
        sphereCollider.Radius = 1f;
        sphereCollider.Transform = Transform;
        Add<Collider>(sphereCollider);

        Texture2D texture2D = Content.Load<Texture2D>("Square");
        Renderer renderer = new Renderer(Content.Load<Model>("Sphere"), Transform, camera, Content, graphicsDevice,
            light, "Shader", 2, 20, texture2D);
        Add<Renderer>(renderer);
        // Add other component required for Player
    } 
    
    public override void Update() 
    {
        // Control the player
        if (InputManager.IsKeyDown(Keys.W) && Terrain.GetAltitude(Transform.LocalPosition + Transform.Forward * Time.ElapsedGameTime * 10) < 1) // move forward
            this.Transform.LocalPosition += this.Transform.Forward * Time.ElapsedGameTime * 10;
        if (InputManager.IsKeyDown(Keys.S) && Terrain.GetAltitude(Transform.LocalPosition + Transform.Backward * Time.ElapsedGameTime * 10) < 1) // move backward
            Transform.LocalPosition += Transform.Backward * Time.ElapsedGameTime * 10;
        if (InputManager.IsKeyDown(Keys.A) && Terrain.GetAltitude(Transform.LocalPosition + Transform.Left * Time.ElapsedGameTime * 10) < 1)
            Transform.LocalPosition += Transform.Left * Time.ElapsedGameTime * 10;
        if (InputManager.IsKeyDown(Keys.D) && Terrain.GetAltitude(Transform.LocalPosition + Transform.Right * Time.ElapsedGameTime * 10) < 1)
            Transform.LocalPosition += Transform.Right * Time.ElapsedGameTime * 10;
        // change the Y position corresponding to the terrain (maze)
        this.Transform.LocalPosition = new Vector3(
            this.Transform.LocalPosition.X,
            Terrain.GetAltitude(this.Transform.LocalPosition),
            this.Transform.LocalPosition.Z) + Vector3.Up;
        base.Update();
    }
}