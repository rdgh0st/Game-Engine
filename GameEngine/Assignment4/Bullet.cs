using CPI311.GameEngine.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CPI311.GameEngine;

public class Bullet : GameObject
{
    public bool isActive { get; set; }
    
    public Bullet(ContentManager Content, Camera camera, GraphicsDevice 
        graphicsDevice, Light light)
        : base()
    {
        Transform.Scale = new Vector3(0.125f, 0.125f, 0.125f);
        // *** Add Rigidbody
        RigidBody rigidbody = new RigidBody();
        rigidbody.Transform = Transform;
        rigidbody.Mass = 1;
        Add<RigidBody>(rigidbody);
        // *** Add Renderer
        Texture2D texture = Content.Load<Texture2D>("pea_proj");
        Renderer renderer = new Renderer(Content.Load<Model>("bullet"),
            Transform, camera, Content, graphicsDevice, light, null, 1, 20f, texture);
        Add<Renderer>(renderer);
        // *** Add collider
        SphereCollider sphereCollider = new SphereCollider();
        sphereCollider.Radius = renderer.ObjectModel.Meshes[0].BoundingSphere.Radius * Transform.LocalScale.Y;
        sphereCollider.Transform = Transform;
        Add<Collider>(sphereCollider);
        //*** Additional Property (for Asteroid, isActive = true)
        isActive = false;
    }
    
    public override void Update()
    {
        if (!isActive) return;
            
        if (Transform.Position.X > GameConstants.PlayfieldSizeX ||
            Transform.Position.X < -GameConstants.PlayfieldSizeX ||
            Transform.Position.Z > GameConstants.PlayfieldSizeY ||
            Transform.Position.Z < -GameConstants.PlayfieldSizeY)
        {
            isActive = false;
            Rigidbody.Velocity = Vector3.Zero; // stop moving
        } 
            
        base.Update();
    }
    
    public override void Draw()
    {
        if(isActive) base.Draw();
    }
}