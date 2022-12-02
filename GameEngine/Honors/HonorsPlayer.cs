using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CPI311.GameEngine;

public class HonorsPlayer : Sprite
{
    public bool crashed { get; set; }
    public float health { get; set; } = 10;
    private ProgressBar healthBar;
    public HonorsPlayer(Texture2D texture, Texture2D squareTex) : base(texture)
    {
        Scale = new Vector2(0.5f, 0.5f);
        Position = new Vector2(288, ScreenManager.GraphicsDevice.Viewport.Height - texture.Height * Scale.Y);
        healthBar = new ProgressBar(squareTex, Color.Red);
        healthBar.Position = new Vector2(60, 50);
    }

    public override void Update()
    {
        health -= Time.ElapsedGameTime;
        if (health <= 0)
        {
            crashed = true;
        }
        healthBar.setProgressScale(health / 10);
        if (InputManager.IsKeyPressed(Keys.A) && Position.X - 90 >= 50)
        {
            Position -= new Vector2(90, 0);
        } else if (InputManager.IsKeyPressed(Keys.D) && Position.X + 90 <= ScreenManager.GraphicsDevice.Viewport.Width - 50)
        {
            Position += new Vector2(90, 0);
        }

        base.Update();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        healthBar.Draw(spriteBatch);
        base.Draw(spriteBatch);
    }
}