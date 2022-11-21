using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CPI311.GameEngine;

public class ProgressBar : Sprite
{
    private float progressScale { get; set; }
    private Color progressColor { get; set; }
    public ProgressBar(Texture2D texture, Color color) : base(texture)
    {
        progressScale = 1f;
        progressColor = color;
        Scale = new Vector2(2, 1);
        Origin = new Vector2(0, 0);
    }
    // get percent progress, draw background then foreground with scale based on percent
    // for total distance, find distance between current and last position every update and add it

    public override void Draw(SpriteBatch spriteBatch)
    {
        //base.Draw(spriteBatch);
        spriteBatch.Draw(Texture, Position, Source, Color.White, Rotation, Origin, Scale, Effects, Layer);
        spriteBatch.Draw(Texture, Position, Source, progressColor, Rotation, Origin, new Vector2(Scale.X * progressScale, Scale.Y), Effects, Layer);
    }

    public void setProgressScale(float scale)
    {
        progressScale = scale;
    }
    
}