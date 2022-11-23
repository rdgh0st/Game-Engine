using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CPI311.GameEngine;

public class Agent : GameObject
{
    public AStarSearch search;
    List<Vector3> path;
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
                //Vector3 pos = new Vector3(gridW * i + gridW / 2 - Terrain.size.X / 2, gridH * j + gridH / 2);
                //if (Terrain.GetAltitude(pos) > 1.0)
                search.Nodes[j, i].Passable = false;
            }
    }
}