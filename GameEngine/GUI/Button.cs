using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace CPI311.GameEngine.GUI;

public class Button : GUIElement
{
    public override void Update()
    {
        if (InputManager.isMouseLeftClicked() &&
            Bounds.Contains(InputManager.GetMousePosition()))
            OnAction();
    }
    public override void Draw(SpriteBatch spriteBatch, SpriteFont font)
    {
        base.Draw(spriteBatch, font);
        spriteBatch.DrawString(font, Text,
            new Vector2(Bounds.X, Bounds.Y), Color.Black);
    }
}