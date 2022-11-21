using Microsoft.Xna.Framework.Graphics;

namespace CPI311.GameEngine;

public class Health : Component
{
    public float MaxHealth { get; set; }
    public float CurrentHealth { get; private set; }

    public Health(float h)
    {
        MaxHealth = h;
        CurrentHealth = MaxHealth;
    }

    // return true if dead
    public bool TakeDamage(float amount)
    {
        if (GameObject.Tag == "iFrames")
        {
            return false;
        }
        GameObject.Tag = "iFrames";
        CurrentHealth -= amount;
        return CurrentHealth <= 0;
    }

    public void Heal(int amount)
    {
        CurrentHealth += amount;
        if (CurrentHealth > MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }
    }

}