using CPI311.GameEngine.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CPI311.GameEngine;

public class Player : GameObject
{
    public float iFramesDuration { get; set; } = 2;
    private float iFramesTimer;
    private bool drawn = true;
    public float rushDuration { get; set; } = 1.5f;
    private float rushTimer = 0f;
    private ProgressBar healthBar;
    public Player(Model playerModel, Texture2D playerTexture, ContentManager Content, Camera camera, GraphicsDevice 
        graphicsDevice, Light light) : base()
    {
        Transform.Position = new Vector3(0, 0, 0);
        //Transform.Rotate(Vector3.Left, MathHelper.ToRadians(90));
        //Transform.Scale = new Vector3(0.25f, 0.25f, 0.25f);
        PlayerController controller = new PlayerController(new Vector3(20, 0, 20));
        controller.TurnSpeed = 10;
        controller.MoveSpeed = 10;
        controller.TimeToShoot = 1.5f;
        Add<PlayerController>(controller);
        Renderer playerRenderer = new Renderer(playerModel, Transform, camera, Content, graphicsDevice, light, null, 0, 20f, null);
        Add<Renderer>(playerRenderer);
        SphereCollider sphereCollider = new SphereCollider();
        sphereCollider.Radius = 1.0f * Transform.LocalScale.Y;
        Add<Collider>(sphereCollider);
        Health health = new Health(100);
        Add(health);
        RigidBody rigidbody = new RigidBody();
        rigidbody.Mass = 1;
        Add<RigidBody>(rigidbody);
        healthBar = new ProgressBar(Content.Load<Texture2D>("Square"), Color.Red);
        healthBar.Scale = new Vector2(5, 1);
        healthBar.Position = new Vector2(graphicsDevice.Viewport.Width - 200, 60);
    }

    public override void Update()
    {
        if (Rigidbody.Velocity != Vector3.Zero)
        {
            rushTimer += Time.ElapsedGameTime;
            if (rushTimer > rushDuration)
            {
                Get<PlayerController>().target = Transform.Position;
                Rigidbody.Velocity = Vector3.Zero;
                rushTimer = 0;
            }
        }
        healthBar.setProgressScale(Get<Health>().CurrentHealth / Get<Health>().MaxHealth);
        base.Update();
        if (Tag == "iFrames")
        {
            iFramesTimer += Time.ElapsedGameTime;
            drawn = !drawn;
        }

        if (iFramesTimer >= iFramesDuration)
        {
            Tag = "";
            drawn = true;
            iFramesTimer = 0;
        }
    }

    public override void Draw()
    {
        if (drawn)
        {
            Renderer.color = Color.SandyBrown.ToVector3();
            base.Draw();
        }
        
        ScreenManager.SpriteBatch.Begin();
        healthBar.Draw(ScreenManager.SpriteBatch);
        ScreenManager.SpriteBatch.End();
    }
}