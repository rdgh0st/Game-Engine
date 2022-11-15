using CPI311.GameEngine.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CPI311.GameEngine;

public class FinalBullet : GameObject
{
    public FinalBullet(Model bulletModel, Texture2D bulletTexture, ContentManager Content, Camera camera, GraphicsDevice
        graphicsDevice, Light light) : base()
    {
        Transform.Scale = new Vector3(0.25f, 0.25f, 0.25f);
        Renderer bulletRenderer = new Renderer(Content.Load<Model>("Sphere"), Transform, camera, Content,
            graphicsDevice, light, null, 0, 20f, null);
        bulletRenderer.color = Color.Black.ToVector3();
        Add<Renderer>(bulletRenderer);
        SphereCollider sphereCollider = new SphereCollider();
        sphereCollider.Radius = 1.0f * Transform.LocalScale.Y;
        Add<Collider>(sphereCollider);
        RigidBody rigidbody = new RigidBody();
        rigidbody.Mass = 1;
        Add<RigidBody>(rigidbody);
    }
}