using Microsoft.Xna.Framework.Graphics;

namespace CPI311.GameEngine;

public class Health : Component
{
    public int MaxHealth { get; set; }
    private int CurrentHealth;

    public Health(int h)
    {
        MaxHealth = h;
        CurrentHealth = MaxHealth;
    }

    public bool TakeDamage(int amount)
    {
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