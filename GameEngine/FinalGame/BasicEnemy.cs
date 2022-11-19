using System;
using System.Collections.Generic;
using CPI311.GameEngine.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CPI311.GameEngine;

public class BasicEnemy : GameObject
{
    private float iFrameDuration = 0.5f;
    private float iFrameTimer = 0;
    private GameObject player;
    AStarSearch search;
    private AStarNode playerNode;
    List<Vector3> path;
    private BasicEffect effect;
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
        search = new AStarSearch(-45, -30, 60, 90); // size of grid 
        AStarNode enemyNode = null;
        foreach (AStarNode node in search.Nodes)
        {
            if (Vector3.Distance(node.Position, player.Transform.Position) < 1)
            {
                playerNode = node;
            } 
            if (Vector3.Distance(node.Position, Transform.Position) < 1)
            {
                enemyNode = node;
            }
        }

        search.Start = enemyNode; 
        search.Start.Passable = true;
        search.End = playerNode; 
        search.End.Passable = true;
        
        search.Search(); // A search is made here.
        path = new List<Vector3>();
        AStarNode current = search.End;
        while (current != null)
        {
            path.Insert(0, current.Position);
            current = current.Parent;
        }

        effect = new BasicEffect(graphicsDevice);
        effect.View = camera.View;
        effect.Projection = camera.Projection;
    }

    public override void Update()
    {
        if (Get<RigidBody>().Velocity != Vector3.Zero &&
            Vector3.Distance(player.Transform.Position, Transform.Position) <= 4)
        {
            Get<RigidBody>().Velocity = Vector3.Zero;
        }

        if (InputManager.IsKeyPressed(Keys.Space))
        {
            foreach (AStarNode node in search.Nodes)
                if (Vector3.Distance(node.Position, player.Transform.Position) <= 1)
                {
                    playerNode = node;
                }

            search.End = playerNode; // assign a random end node (passable)
            search.End.Passable = true;
            search.Search();
            path.Clear();
            AStarNode current = search.End;
            while (current != null)
            {
                path.Insert(0, current.Position);
                current = current.Parent;
            }
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

    public override void Draw()
    {
        base.Draw();
        
        effect.CurrentTechnique.Passes[0].Apply();
        effect.DiffuseColor = Color.Black.ToVector3();
        for (int i = 1; i < path.Count; i++)
        {
            var vertices = new[] { new VertexPositionColor(path[i - 1], Color.Black),  new VertexPositionColor(path[i], Color.Black) };
            Get<Renderer>().g.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
        }
    }
}