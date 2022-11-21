using System;
using System.Collections.Generic;
using CPI311.GameEngine.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CPI311.GameEngine;

public class AOEAttack : GameObject
{
    private float duration = 0.5f;
    public float timer { get; set; } = 0.5f;
    private List<BasicEnemy> enemies;
    public AOEAttack(Model aoeModel, Texture2D aoeTexture, Vector3 currentPos, ContentManager Content, Camera camera, GraphicsDevice
        graphicsDevice, Light light, List<BasicEnemy> enemies) : base()
    {
        Transform.Position = currentPos;
        Transform.Scale = new Vector3(3, 1, 3);
        Renderer aoeRenderer = new Renderer(aoeModel, Transform, camera, Content,
            graphicsDevice, light, null, 0, 20f, aoeTexture);
        aoeRenderer.color = Color.Orange.ToVector3();
        Add<Renderer>(aoeRenderer);
        SphereCollider sphereCollider = new SphereCollider();
        sphereCollider.Radius = 1.0f * Transform.LocalScale.X;
        Add<Collider>(sphereCollider);
        this.enemies = enemies;
    }

    public override void Update()
    {
        if (timer >= duration)
        {
            return;
        }
        base.Update();
        timer += Time.ElapsedGameTime;
    }

    public override void Draw()
    {
        if (timer >= duration)
        {
            return;
        }
        base.Draw();
    }
    
    public void SetPosition(Vector3 pos)
    {
        Transform.Position = pos;
    }

    public bool isActive()
    {
        return timer < duration;
    }
}