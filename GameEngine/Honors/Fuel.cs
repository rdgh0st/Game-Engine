using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CPI311.GameEngine;

public class Fuel : Sprite
{
    public Fuel(Texture2D texture, List<ObstacleCar> cars) : base(texture)
    {
        Scale = new Vector2(2, 2);
        Random random = new Random();
        int seed = random.Next(0, 4);
        for (int i = 0; i < cars.Count; i++)
        {
            if (cars[i].Position.Y <= 200 && cars[i].Position.X == 108 && seed == 0)
            {
                seed++;
            } else if (cars[i].Position.Y <= 200 && cars[i].Position.X == 198 && seed == 1)
            {
                seed++;
            } else if (cars[i].Position.Y <= 200 && cars[i].Position.X == 288 && seed == 2)
            {
                seed++;
            } else if (cars[i].Position.Y <= 200 && cars[i].Position.X == 378 && seed == 3)
            {
                seed = 0;
            }
        }
        switch (seed)
        {
            case 0:
                Position = new Vector2(108, 0);
                break;
            case 1:
                Position = new Vector2(198, 0);
                break;
            case 2:
                Position = new Vector2(288, 0);
                break;
            case 3:
                Position = new Vector2(378, 0);
                break;
        }
    }
    
    public override void Update()
    {
        Position += new Vector2(0, 300 * Time.ElapsedGameTime);
        base.Update();
    }
}