using CPI311.GameEngine.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CPI311.GameEngine;

public class BasicEnemy : GameObject
{
    private float iFrameDuration = 0.5f;
    private float iFrameTimer = 0;
    private GameObject player;
    public BasicEnemy(Model enemyModel, Texture2D enemyTexture, Vector3 enemyPos, ContentManager Content, Camera camera, GraphicsDevice
        graphicsDevice, Light light, GameObject player) : base()
    {
        Transform.Position = enemyPos;
        Renderer enemyRenderer = new Renderer(Content.Load<Model>("Sphere"), Transform, camera, Content,
            graphicsDevice, light, null, 0, 20f, enemyTexture);
        Add<Renderer>(enemyRenderer);
        SphereCollider sphereCollider = new SphereCollider();
        sphereCollider.Radius = 1.0f * Transform.LocalScale.Y;
        Add<Collider>(sphereCollider);
        Health h = new Health(100);
        Add(h);
        RigidBody rigidbody = new RigidBody();
        rigidbody.Mass = 1;
        Add<RigidBody>(rigidbody);
        this.player = player;
    }

    public override void Update()
    {
        if (Get<RigidBody>().Velocity != Vector3.Zero &&
            Vector3.Distance(player.Transform.Position, Transform.Position) <= 4)
        {
            Get<RigidBody>().Velocity = Vector3.Zero;
        }
        base.Update();
        if (Tag == "iFrames")
        {
            iFrameTimer += Time.ElapsedGameTime;
            if (iFrameTimer >= iFrameDuration)
            {
                iFrameTimer = 0;
                Tag = "enemy";
            }
        }
    }
}