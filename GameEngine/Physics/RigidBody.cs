using System;
using Microsoft.Xna.Framework;

namespace CPI311.GameEngine.Physics;

public class RigidBody : Component, IUpdateable
{
    public Vector3 Velocity { get; set; }
    public float Mass { get; set; }
    public Vector3 Acceleration { get; set; }
    public Vector3 Impulse { get; set; }
    public float TimeFactor { get; set; } = 1;
    public void Update()
    {
        Velocity += Acceleration * Time.ElapsedGameTime * TimeFactor + Impulse / Mass;
        Transform.LocalPosition += Velocity * Time.ElapsedGameTime * TimeFactor;
        Impulse = Vector3.Zero;
    }
}