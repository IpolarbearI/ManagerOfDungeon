// ItemSO.cs - ScriptableObject item definition. Create via Assets > Create > FantasyDungeon > Item.
using UnityEngine;

namespace FantasyDungeonPixelPack
{
    [CreateAssetMenu(fileName = "Item", menuName = "FantasyDungeon/Item")]
    /// <summary>
    /// Item definition asset (Create > FantasyDungeon > Item): icon, type, rarity, stacking, value, equip bonuses and consumable effects.
    /// </summary>
    public class ItemSO : ScriptableObject
    {
        public enum ItemType { Weapon, Armor, Shield, Potion, Consumable, Material, Key, Quest, Accessory, Spell }

        public string itemName = "New Item";
        [TextArea] public string description;
        public Sprite icon;
        public ItemType type = ItemType.Material;
        [Tooltip("0=Common 1=Uncommon 2=Rare 3=Epic 4=Legendary")]
        public int rarity = 0;
        public bool stackable = true;
        public int maxStack = 99;
        public int value = 1;

        [Header("Equip bonuses (Weapon/Armor/Accessory)")]
        public int bonusAttack;
        public int bonusDefense;
        public int bonusMaxHP;
        public int bonusMaxMP;

        [Header("Consumable effect")]
        public int healHP;
        public int restoreMP;
    }
}
