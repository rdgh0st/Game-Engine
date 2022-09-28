using Microsoft.Xna.Framework;

namespace CPI311.GameEngine.Physics;

public class Collider : Component
{
    public virtual bool Collides(Collider other, out Vector3 normal)
    {
        normal = Vector3.Zero;
        return false;
    }
}