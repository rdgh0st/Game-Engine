using System;
using System.Collections.Generic;
using System.Threading;
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
    public List<BasicEnemy> otherEnemies { get; set; }
    public float MoveSpeed { get; set; } = 4f;
    public bool selected { get; set; } = false;
    public bool hovered { get; set; } = false;
    private ProgressBar healthBar;
    public BasicEnemy(Model enemyModel, Texture2D enemyTexture, Vector3 enemyPos, ContentManager Content, Camera camera, GraphicsDevice
        graphicsDevice, Light light, GameObject player, List<BasicEnemy> otherEnemies) : base()
    {
        Transform.Position = enemyPos;
        Renderer enemyRenderer = new Renderer(Content.Load<Model>("shipNew"), Transform, camera, Content,
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
        this.otherEnemies = otherEnemies;
        healthBar = new ProgressBar(Content.Load<Texture2D>("Square"), Color.Red);
        healthBar.Scale = new Vector2(10, 1);
        healthBar.Position = new Vector2(Renderer.g.Viewport.Width / 2, 20);
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
            for (int i = 0; i < otherEnemies.Count; i++)
            {
                if (Vector3.Distance(node.Position, otherEnemies[i].Transform.Position) < 1)
                {
                    node.Passable = false;
                }
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
        
        ThreadPool.QueueUserWorkItem(new WaitCallback(FindPath));
    }

    public override void Update()
    {
        if (Get<RigidBody>().Velocity != Vector3.Zero &&
            Vector3.Distance(player.Transform.Position, Transform.Position) <= 4)
        {
            Get<RigidBody>().Velocity = Vector3.Zero;
        } else if (Get<RigidBody>().Velocity == Vector3.Zero && Vector3.Distance(player.Transform.Position, Transform.Position) >= 2)
        {
            Move();
        }

        healthBar.setProgressScale(Get<Health>().CurrentHealth / Get<Health>().MaxHealth);
        
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
        if (selected)
        {
            Renderer.color = Color.MediumVioletRed.ToVector3();
        }
        else if (hovered)
        {
            Renderer.color = Color.DarkRed.ToVector3();
        } else 
        {
            Renderer.color = Color.Red.ToVector3();
        }
        base.Draw();
        /*
        effect.CurrentTechnique.Passes[0].Apply();
        effect.DiffuseColor = Color.Black.ToVector3();
        for (int i = 1; i < path.Count; i++)
        {
            var vertices = new[] { new VertexPositionColor(path[i - 1], Color.Black),  new VertexPositionColor(path[i], Color.Black) };
            Get<Renderer>().g.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, 1);
        } */

        if (selected)
        {
            ScreenManager.SpriteBatch.Begin();
            healthBar.Draw(ScreenManager.SpriteBatch);
            ScreenManager.SpriteBatch.End();
        }
    }
    
    private void Move()
    {
        Vector3 goal = Transform.Position;
        for (int i = 0; i < path.Count - 1; i++)
        {
            if (Vector3.Distance(Transform.Position, path[i]) < 1)
            {
                try
                {
                    goal = path[i + 1];
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
        if (goal == Transform.Position)
        {
            return;
        }
        float distance = Vector3.Distance(Transform.Position, goal);
        float finalSpeed = (distance / MoveSpeed);
        Transform.Position = Vector3.Lerp(Transform.Position, goal, Time.ElapsedGameTime / finalSpeed);
    }

    private void FindPath(Object obj)
    {
        while (this != null)
        {
            Thread.Sleep(500);
            AStarNode enemyNode = search.Start;
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
                for (int i = 0; i < otherEnemies.Count; i++)
                {
                    if (otherEnemies[i] != this && Vector3.Distance(node.Position, otherEnemies[i].Transform.Position) < 4)
                    {
                        node.Passable = false;
                    }
                    else
                    {
                        node.Passable = true;
                    }
                }
            }

            search.Start = enemyNode; 
            search.Start.Passable = true;
            search.End = playerNode;
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
    }
}