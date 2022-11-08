namespace CPI311.GameEngine;

public class Item : Component
{
    public bool isEquipped { get; set; }

    public enum Slot
    {
        Boots, Legs, Armor, Hat, Weapon
    }
    
    public enum Rarity
    {
        Common, Uncommon, Rare, Epic, Legendary
    }

    private Rarity CurrentRarity;
    private Slot CurrentSlot;

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
    }

}