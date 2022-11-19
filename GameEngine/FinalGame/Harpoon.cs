using System;
using System.Collections.Generic;
using System.Windows.Markup;
using CPI311.GameEngine.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CPI311.GameEngine;

public class Harpoon : GameObject
{
    private float duration = 0.5f;
    private GameObject player;
    public float timer { get; set; } = 0.5f;
    public bool activeSelf { get; set; } = false;
    public bool retracting { get; set; } = false;
    public float expirationRange { get; set; } = 20;
    private List<GameObject> enemies;
    private BasicEffect effect;
    public Harpoon(Model aoeModel, Texture2D aoeTexture, Vector3 currentPos, ContentManager Content, Camera camera, GraphicsDevice
        graphicsDevice, Light light, List<GameObject> enemies, GameObject player) : base()
    {
        Transform.Position = currentPos;
        Transform.Scale = new Vector3(0.25f, 0.25f, 0.25f);
        Renderer aoeRenderer = new Renderer(aoeModel, Transform, camera, Content,
            graphicsDevice, light, null, 0, 20f, aoeTexture);
        aoeRenderer.color = Color.Black.ToVector3();
        Add<Renderer>(aoeRenderer);
        SphereCollider sphereCollider = new SphereCollider();
        sphereCollider.Radius = 1.0f * Transform.LocalScale.X;
        Add<Collider>(sphereCollider);
        RigidBody rigidbody = new RigidBody();
        rigidbody.Mass = 1;
        Add<RigidBody>(rigidbody);
        this.enemies = enemies;
        this.player = player;
        effect = new BasicEffect(graphicsDevice);
        effect.View = camera.View;
        effect.Projection = camera.Projection;
    }

    public override void Update()
    {
        if (retracting && Vector3.Distance(Transform.Position, player.Transform.Position) <= 2)
        {
            retracting = false;
            activeSelf = false;
            Get<RigidBody>().Velocity = Vector3.Zero;
            Transform.Position = player.Transform.Position;
        }
        
        if (!activeSelf) return;
        player.Get<PlayerController>().CurrentState = PlayerController.State.Still;
        base.Update();

        if (Vector3.Distance(Transform.Position, player.Transform.Position) >= expirationRange)
        {
            retracting = true;
            Vector3 direction = player.Transform.Position - Transform.Position;
            direction.Normalize();
            Get<RigidBody>().Velocity = direction * 20;
        }
    }

    public override void Draw()
    {
        if (!activeSelf) return;
        base.Draw();
        effect.CurrentTechnique.Passes[0].Apply();
        var vertices = new[] { new VertexPositionColor(Transform.Position, Color.White),  new VertexPositionColor(player.Transform.Position, Color.White) };
        Get<Renderer>().g.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
    }
}