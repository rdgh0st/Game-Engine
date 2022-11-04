using System;
using Microsoft.Xna.Framework;

namespace CPI311.GameEngine;

public class PlayerController : Component, IUpdateable
{
    private Vector3 target;

    public PlayerController(Vector3 t)
    {
        target = t;
    }

    public void Update()
    {
        Vector3 view = target - Transform.Position;
        float angle = (float)Math.Acos(Vector3.Dot(Transform.Forward, view) / Transform.Forward.Length() / view.Length());
        if (Math.Abs(angle) >= 2)
        {
            Transform.Rotate(Vector3.Up, 2 * Time.ElapsedGameTime);
        }
        //Transform.Rotation = Quaternion.CreateFromRotationMatrix(Matrix.CreateFromAxisAngle(Transform.Up, angle));
    }
}