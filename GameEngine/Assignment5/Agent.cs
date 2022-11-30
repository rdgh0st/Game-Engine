using System;
using System.Collections.Generic;
using CPI311.GameEngine.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CPI311.GameEngine;

public class Agent : GameObject
{
    public AStarSearch search;
    public List<Vector3> path;
    private  float speed = 5f; //moving speed
    private int gridSize = 20; //grid size
    private TerrainRenderer Terrain;

    public Agent(TerrainRenderer terrain, ContentManager Content,
        Camera camera, GraphicsDevice graphicsDevice, Light light) : base()
    {
        Terrain = terrain;
        path = null;
        search = new AStarSearch(gridSize, gridSize);
        float gridW = Terrain.size.X / gridSize;
        float gridH = Terrain.size.Y / gridSize;
        for (int i = 0; i < gridSize; i++)
            for (int j = 0; j < gridSize; j++)
            {
                Vector3 pos = new Vector3(gridW * i + gridW / 2 - Terrain.size.X / 2, 0, gridH * j + gridH / 2 - Terrain.size.Y/2);
                if (Terrain.GetAltitude(pos) > 1.0f)
                {
                    search.Nodes[j, i].Passable = false;
                }
            }
        RigidBody rigidbody = new RigidBody();
        rigidbody.Transform = Transform;
        rigidbody.Mass = 1;
        Add<RigidBody>(rigidbody);

        SphereCollider sphereCollider = new SphereCollider();
        sphereCollider.Radius = 1f;
        sphereCollider.Transform = Transform;
        Add<Collider>(sphereCollider);

        Texture2D texture2D = Content.Load<Texture2D>("Square");
        Renderer renderer = new Renderer(Content.Load<Model>("Sphere"), Transform, camera, Content, graphicsDevice,
            light, "Shader", 2, 20, texture2D);
        Add<Renderer>(renderer);
    }
    
    public override void Update()
    {
        //Console.WriteLine(Transform.Position);
        if (path != null && path.Count >0)
        {
            // Move to the destination along the path
            Vector3 currentPos = Transform.LocalPosition;
            Vector3 goalPos = GetGridPosition(path[0]);

            currentPos.Y = 0;
            goalPos.Y = 0;

            Vector3 dir = Vector3.Distance(currentPos, goalPos) == 0
                ? Vector3.Zero
                : Vector3.Normalize(goalPos - currentPos);

            Rigidbody.Velocity = new Vector3(dir.X, 0, dir.Z) * speed;
            if (Vector3.Distance(currentPos, goalPos) < 1f) // if it reaches to a point, go to the next in path
            {
                path.RemoveAt(0);
                if (path.Count == 0) // if it reached to the goal
                {
                    path = null;
                    return;
                }
            }
        }
        else
        {
            RandomPathFinding();
            Transform.LocalPosition = GetGridPosition(path[0]);
            //Transform.Position = new Vector3(-12.5f, 1, -8);
        }
        this.Transform.LocalPosition = new Vector3(
            this.Transform.LocalPosition.X,
            Terrain.GetAltitude(this.Transform.LocalPosition),
            this.Transform.LocalPosition.Z) + Vector3.Up;
        Transform.Update();
        base.Update();
    }
    
    private Vector3 GetGridPosition(Vector3 gridPos) 
    {
        float gridW = Terrain.size.X/search.Cols;
        float gridH = Terrain.size.Y/search.Rows;
        return new Vector3(gridW*gridPos.X+gridW/2-Terrain.size.X/2, 0, gridH * gridPos.Z + gridH / 2 - Terrain.size.Y/2);
    }
    
    private void RandomPathFinding()
    {
        Random random = new Random();
        while (!(search.Start = search.Nodes[random.Next(search.Rows), 
                   random.Next(search.Cols)]).Passable) ;
        search.End = search.Nodes[search.Rows / 2, search.Cols / 2];
        search.Search();
        path = new List<Vector3>();
        AStarNode current = search.End;
        var count =0;
        while (current != null)
        {
            count++;
            path.Insert(0, current.Position);
            current = current.Parent;
        }
    }
}