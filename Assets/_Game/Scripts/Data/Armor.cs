namespace ManagerOfDungeon
{
    public sealed class Armor
    {
        public string Name { get; }
        public int HealthBonus { get; }

        public Armor(string name, int healthBonus)
        {
            Name = name ?? string.Empty;
            HealthBonus = healthBonus;
        }
    }
}
