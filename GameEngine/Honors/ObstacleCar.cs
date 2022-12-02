using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CPI311.GameEngine;

public class ObstacleCar : Sprite
{
    private HonorsPlayer _player;
    private int speed;
    public ObstacleCar(Texture2D texture, HonorsPlayer player): base(texture)
    {
        Random random = new Random();
        Scale = new Vector2(0.5f, 0.5f);
        speed = random.Next(316, 320);
        _player = player;
        Rotation = MathHelper.ToRadians(180);
        switch (speed)
        {
            case 316:
                Position = new Vector2(108, 0);
                break;
            case 317:
                Position = new Vector2(198, 0);
                break;
            case 318:
                Position = new Vector2(288, 0);
                break;
            case 319:
                Position = new Vector2(378, 0);
                break;
        }
    }

    public override void Update()
    {
        Position += new Vector2(0, speed * Time.ElapsedGameTime);
        base.Update();
    }
}