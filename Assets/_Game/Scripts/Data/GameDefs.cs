using System.Collections.Generic;

namespace ManagerOfDungeon
{
    /// <summary>몬스터 종족 정의 (04-4). 본 게임 전투는 이 값으로 몬스터를 만든다(전투 테스트 씬에서 실시간 조정).</summary>
    public sealed class MonsterDef
    {
        public string Id;
        public string DisplayName;
        public int MaxHealth;
        public int AttackPower;
        public float AttackSpeed;
        public float MoveSpeed;
        public int AssetValue;

        public Monster CreateMonster(string name)
        {
            return new Monster(name, MaxHealth, AttackPower, AttackSpeed, MoveSpeed, AssetValue);
        }
    }

    /// <summary>용사 직업 정의 (04-9).</summary>
    public sealed class HeroJobDef
    {
        public string Id;
        public string DisplayName;
        public int MaxHealth;
        public int AttackPower;
        public float AttackSpeed;
        public float MoveSpeed;
        public bool IsRanged;
        public int Mana;
        public int BaseValue;
    }

    public enum EquipmentSlot
    {
        Weapon,
        Armor
    }

    /// <summary>장비 정의 (04-15).</summary>
    public sealed class EquipmentDef
    {
        public string Id;
        public string DisplayName;
        public EquipmentSlot Slot;
        public int FixedValue;
        public int BuyPrice;
        public int SellPrice;

        public Weapon CreateWeapon()
        {
            switch (Id)
            {
                case GameDefs.OldWoodBow: return new OldWoodBow();
                case GameDefs.ScrapSword: return new ScrapSword();
                default: return null;
            }
        }
    }

    /// <summary>함정 정의 (04-8).</summary>
    public sealed class TrapDef
    {
        public string Id;
        public string DisplayName;
        public int AssetValue;
        public int BuyPrice;
        public bool Consumable;

        /// <summary>소모형 함정 재설치 비용 = 구매가 × 10% (최소 1솔).</summary>
        public int ReinstallPrice
        {
            get
            {
                int price = (int)System.Math.Round(BuyPrice * BalanceConfig.TrapReinstallRate);
                return price < 1 ? 1 : price;
            }
        }
    }

    /// <summary>
    /// 모든 정의 테이블. 나중에 ScriptableObject로 옮길 때 이 클래스의 조회 API만 유지하면 된다.
    /// </summary>
    public static class GameDefs
    {
        public const string Goblin = "goblin";
        public const string Orc = "orc";
        public const string Werewolf = "werewolf";

        public const string Warrior = "warrior";
        public const string Archer = "archer";
        public const string Mage = "mage";

        public const string ScrapSword = "scrap_sword";
        public const string OldWoodBow = "old_wood_bow";

        public const string SpikeFloor = "spike_floor";
        public const string SpiritTrap = "spirit_trap";
        public const string Mine = "mine";
        public const string Wall = "wall";
        public const string Sticky = "sticky";

        // 용사 강화 누적치 (04-9): Lv.0~4
        // [결정 필요] AI 0 기본 스탯이 크게 낮아졌는데 강화 누적치는 기존값 그대로다(전사 HP 470에 +100~+1000).
        public static readonly int[] HeroHpBonus = { 0, 100, 300, 550, 1000 };
        public static readonly int[] HeroAtkBonus = { 0, 10, 25, 50, 100 };
        public static readonly float[] HeroAsBonus = { 0f, 0.3f, 0.7f, 1.2f, 2.0f };

        // Alpha 2.01에서 등장하는 직업 (06-1: 전사·궁수만)
        public static readonly string[] AlphaHeroJobs = { Warrior, Archer };

        static readonly Dictionary<string, MonsterDef> Monsters = new Dictionary<string, MonsterDef>
        {
            { Goblin, new MonsterDef { Id = Goblin, DisplayName = "고블린", MaxHealth = 420, AttackPower = 45, AttackSpeed = 1.2f, MoveSpeed = 10f, AssetValue = 50 } },
            { Orc, new MonsterDef { Id = Orc, DisplayName = "오크", MaxHealth = 720, AttackPower = 65, AttackSpeed = 0.65f, MoveSpeed = 4f, AssetValue = 70 } },
            { Werewolf, new MonsterDef { Id = Werewolf, DisplayName = "웨어울프", MaxHealth = 550, AttackPower = 80, AttackSpeed = 1.0f, MoveSpeed = 6f, AssetValue = 120 } },
        };

        static readonly Dictionary<string, HeroJobDef> Heroes = new Dictionary<string, HeroJobDef>
        {
            // AI 0 기본 스탯 (11-10 개정). 목표: 강화 0 고블린이 1대1에서 근소하게 승리.
            // [결정 필요] BaseValue(파티 가치)는 임시값 — 고블린(50)과 1대1 비슷한 전력으로 설정.
            { Warrior, new HeroJobDef { Id = Warrior, DisplayName = "전사", MaxHealth = 470, AttackPower = 45, AttackSpeed = 1.0f, MoveSpeed = 8f, IsRanged = false, Mana = 350, BaseValue = 50 } },
            { Archer, new HeroJobDef { Id = Archer, DisplayName = "궁수", MaxHealth = 360, AttackPower = 23, AttackSpeed = 1.0f, MoveSpeed = 6.5f, IsRanged = true, Mana = 300, BaseValue = 40 } },
            { Mage, new HeroJobDef { Id = Mage, DisplayName = "마법사", MaxHealth = 315, AttackPower = 27, AttackSpeed = 0.7f, MoveSpeed = 5f, IsRanged = true, Mana = 400, BaseValue = 45 } },
        };

        static readonly Dictionary<string, EquipmentDef> Equipment = new Dictionary<string, EquipmentDef>
        {
            // 자산가치 11-10 개정. [결정 필요] 상인 구매·판매가는 Beta 상인 도입 때 새 가치 기준으로 재계산.
            { ScrapSword, new EquipmentDef { Id = ScrapSword, DisplayName = "고철 검", Slot = EquipmentSlot.Weapon, FixedValue = 10, BuyPrice = 60, SellPrice = 30 } },
            { OldWoodBow, new EquipmentDef { Id = OldWoodBow, DisplayName = "고목 활", Slot = EquipmentSlot.Weapon, FixedValue = 20, BuyPrice = 60, SellPrice = 30 } },
        };

        static readonly Dictionary<string, TrapDef> Traps = new Dictionary<string, TrapDef>
        {
            // 자산가치 11-10 개정. 재설치 = 구매가의 10% (가시 2 / 원령 8 / 지뢰 15솔)
            { SpikeFloor, new TrapDef { Id = SpikeFloor, DisplayName = "가시 바닥", AssetValue = 10, BuyPrice = 20, Consumable = true } },
            // 아래는 Beta 함정·지형. 값만 등록해 두고 A2에서는 설치 UI가 없다.
            { SpiritTrap, new TrapDef { Id = SpiritTrap, DisplayName = "원령의 한", AssetValue = 30, BuyPrice = 80, Consumable = true } },
            { Mine, new TrapDef { Id = Mine, DisplayName = "지뢰", AssetValue = 90, BuyPrice = 150, Consumable = true } },
            { Wall, new TrapDef { Id = Wall, DisplayName = "벽", AssetValue = 40, BuyPrice = 50, Consumable = false } },
            { Sticky, new TrapDef { Id = Sticky, DisplayName = "끈끈이", AssetValue = 35, BuyPrice = 50, Consumable = false } },
        };

        public static MonsterDef GetMonster(string id)
        {
            return id != null && Monsters.TryGetValue(id, out var def) ? def : null;
        }

        public static HeroJobDef GetHero(string id)
        {
            return id != null && Heroes.TryGetValue(id, out var def) ? def : null;
        }

        public static EquipmentDef GetEquipment(string id)
        {
            return id != null && Equipment.TryGetValue(id, out var def) ? def : null;
        }

        public static TrapDef GetTrap(string id)
        {
            return id != null && Traps.TryGetValue(id, out var def) ? def : null;
        }
    }
}
