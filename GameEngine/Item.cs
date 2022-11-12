using CPI311.GameEngine.Physics;
using Microsoft.Xna.Framework;

namespace CPI311.GameEngine;

public class Item : Component
{
    public Color color;
    public string name;
    public bool drawName;
    public Vector2 drawCoords { get; private set; } = Vector2.Zero;
    public enum Slot
    {
        Boots, Legs, Armor, Hat, Weapon
    }
    /*
     * BOOTS = Turn Speed - Can be modified in main
     * LEGS = Move Speed - Can be modified in main
     * ARMOR = HP - Can be modified in main
     * HAT = Luck - Can be modified in main
     * WEAPON = Damage - Can be modified in main
     * 
     */
    
    public enum Rarity
    {
        Common, Uncommon, Rare, Epic, Legendary
    }

    public Rarity CurrentRarity;
    public Slot CurrentSlot;

    public Item(int slotSeed, int raritySeed)
    {
        CurrentSlot = slotSeed switch
        {
            < 20 => Slot.Armor,
            < 40 => Slot.Boots,
            < 60 => Slot.Hat,
            < 80 => Slot.Legs,
            _ => Slot.Weapon
        };
        CurrentRarity = raritySeed switch
        {
            < 40 => Rarity.Common,
            < 65 => Rarity.Uncommon,
            < 85 => Rarity.Rare,
            < 95 => Rarity.Epic,
            _ => Rarity.Legendary
        };
        color = CurrentRarity switch
        {
            Rarity.Common => Color.White,
            Rarity.Uncommon => Color.Green,
            Rarity.Rare => Color.MediumBlue,
            Rarity.Epic => Color.Purple,
            Rarity.Legendary => Color.Gold
        };
        name += CurrentSlot switch
        {
            Slot.Armor => "Armor Of ",
            Slot.Boots => "Boots Of ",
            Slot.Hat => "Hat Of ",
            Slot.Legs => "Legs Of ",
            Slot.Weapon => "Pistol Of "
        };
        name += CurrentRarity switch
        {
            Rarity.Common => "Average Quality",
            Rarity.Uncommon => "A Seasoned Pirate",
            Rarity.Rare => "A Captain",
            Rarity.Epic => "Indescribable Wonder",
            Rarity.Legendary => "Legend"
        };
    }

    public void setDrawCoords(Vector2 newCoords)
    {
        if (drawCoords == Vector2.Zero)
        {
            drawCoords = newCoords;
        }
    }

}