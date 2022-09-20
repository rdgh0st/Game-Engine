using System.Collections.Generic;
using System.Data;
using Microsoft.Xna.Framework.Graphics;

namespace CPI311.GameEngine;

public class AnimatedSprite : Sprite
{
    private List<Texture2D> _textures = new List<Texture2D>();
    private int pointer;
    
    public AnimatedSprite(List<Texture2D> textures) : base(textures[0])
    {
        foreach (Texture2D t in textures)
        {
            _textures.Add(t);
        }
    }

    public override void Update()
    {
        if (pointer >= _textures.Count - 1)
        {
            pointer = 0;
        }
        else
        {
            pointer++;
        }
        base.Texture = _textures[pointer];
    }
}