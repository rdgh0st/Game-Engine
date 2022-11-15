using CPI311.GameEngine.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CPI311.GameEngine;

public class Ship : GameObject
{
    public bool isActive { get; set; }
    public Ship(ContentManager Content, Camera camera, GraphicsDevice 
        graphicsDevice, Light light)
        : base()
    {
        Transform.Scale = new Vector3(0.25f, 0.25f, 0.25f);
        // *** Add Rigidbody
        RigidBody rigidbody = new RigidBody();
        rigidbody.Transform = Transform;
        rigidbody.Mass = 1;
        Add<RigidBody>(rigidbody);
        // *** Add Renderer
        Texture2D texture = Content.Load<Texture2D>("wedge_p1_diff_v1");
        Renderer renderer = new Renderer(Content.Load<Model>("p1_wedge"),
            Transform, camera, Content, graphicsDevice, light, null, 1, 20f, texture);
        Add<Renderer>(renderer);
        // *** Add collider
        SphereCollider sphereCollider = new SphereCollider();
        sphereCollider.Radius = renderer.ObjectModel.Meshes[0].BoundingSphere.Radius * Transform.LocalScale.Y * 0.25f;
        sphereCollider.Transform = Transform;
        Add<Collider>(sphereCollider);
        isActive = true;
    }
    
    public override void Update()
    {
        if (!isActive) return;
        if (InputManager.IsKeyDown(Keys.W))
        {
            Transform.Position += Vector3.Up * Time.ElapsedGameTime * GameConstants.PlayerSpeedAdjustment;
        }
        if (InputManager.IsKeyDown(Keys.A))
        {
            Transform.Position += Vector3.Left * Time.ElapsedGameTime * GameConstants.PlayerSpeedAdjustment;
        }
        if (InputManager.IsKeyDown(Keys.S))
        {
            Transform.Position += Vector3.Down * Time.ElapsedGameTime * GameConstants.PlayerSpeedAdjustment;
        }
        if (InputManager.IsKeyDown(Keys.D))
        {
            Transform.Position += Vector3.Right * Time.ElapsedGameTime * GameConstants.PlayerSpeedAdjustment;
        }
            
        if (Transform.Position.X > GameConstants.PlayfieldSizeX ||
            Transform.Position.X < -GameConstants.PlayfieldSizeX ||
            Transform.Position.Z > GameConstants.PlayfieldSizeY ||
            Transform.Position.Z < -GameConstants.PlayfieldSizeY)
        {
            Rigidbody.Velocity = Vector3.Zero; // stop moving
        } 
            
        base.Update();
    }
}