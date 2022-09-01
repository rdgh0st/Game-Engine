using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CPI311.GameEngine;

public class Spiral
{
    public Spiral(Sprite sprite, int radius = 160, float speed = 3.5f, double angle = 1)
    {
        MainSprite = sprite;
        Radius = radius;
        Speed = speed;
        Angle = angle;
    }
    
    public Sprite MainSprite { get; set; }
    public int Radius { get; set; }
    public float Speed { get; set; }
    public Double Angle { get; set; }
    public float t { get; set; }

    public void UpdateSpiral()
    {
        t += Time.ElapsedGameTime * Speed;
        float xt = Convert.ToSingle((Radius + 20 * Math.Cos(t * 100)) * Math.Cos(t)) + 400;
        float yt = Convert.ToSingle((Radius + 20 * Math.Cos(t * 100)) * Math.Sin(t)) + 240;
        MainSprite.Position = new Vector2(xt, yt);
    }
    
}