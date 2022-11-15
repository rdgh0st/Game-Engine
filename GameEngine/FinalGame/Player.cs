using CPI311.GameEngine.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CPI311.GameEngine;

public class Player : GameObject
{
    public Player(Model playerModel, Texture2D playerTexture, ContentManager Content, Camera camera, GraphicsDevice 
        graphicsDevice, Light light) : base()
    {
        Transform.Position = new Vector3(0, 0, 0);
        PlayerController controller = new PlayerController(new Vector3(20, 0, 20));
        controller.TurnSpeed = 10;
        controller.MoveSpeed = 10;
        controller.TimeToShoot = 1.5f;
        Add<PlayerController>(controller);
        Renderer playerRenderer = new Renderer(playerModel, Transform, camera, Content, graphicsDevice, light, null, 2, 20f, playerTexture);
        Add<Renderer>(playerRenderer);
        SphereCollider sphereCollider = new SphereCollider();
        sphereCollider.Radius = 1.0f * Transform.LocalScale.Y;
        Add<Collider>(sphereCollider);
        Health health = new Health(100);
        Add(health);
    }
}