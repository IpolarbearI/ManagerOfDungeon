namespace ManagerOfDungeon
{
    /// <summary>
    /// 자산가치 (11-10 개정). 두 값을 구분한다.
    /// - 방어 전투력 가치 = 몬스터 + 장착 장비 + 설치(미사용) 함정 → 침입 강도 기준
    /// - 총자산가치 = 코어 100 + 방 100 + 방어 전투력 + 보유 솔 (+ 미장착 장비)
    /// 시작: 방어 전투력 130 (고블린 50×2 + 고철 검 10 + 고목 활 20), 총자산 350.
    /// 유대감 레벨당 +50은 폐지.
    /// </summary>
    public static class AssetCalculator
    {
        public static int Total(GameState state)
        {
            if (state == null)
                return 0;

            int total = BalanceConfig.CoreValue + BalanceConfig.RoomValue + DefensePower(state) + state.sol;
            if (BalanceConfig.InventoryCountsAsAsset)
                total += InventoryValue(state);
            return total;
        }

        /// <summary>방어 전투력 가치: 실제 침입 강도에 반영되는 값.</summary>
        public static int DefensePower(GameState state)
        {
            if (state == null)
                return 0;

            int total = 0;
            for (int i = 0; i < state.monsters.Count; i++)
                total += MonsterValue(state.monsters[i]);

            for (int i = 0; i < state.traps.Count; i++)
            {
                var trap = state.traps[i];
                if (trap.used)
                    continue;
                var def = GameDefs.GetTrap(trap.trapId);
                if (def != null)
                    total += def.AssetValue;
            }

            return total;
        }

        /// <summary>
        /// 전투에 실제 배치된 자산(승리 보상 15% 대상) = 코어 + 방 + 방어 전투력.
        /// [결정 필요] 코어·방 포함 여부 — 기존안대로 포함.
        /// </summary>
        public static int Deployed(GameState state)
        {
            if (state == null)
                return 0;
            return BalanceConfig.CoreValue + BalanceConfig.RoomValue + DefensePower(state);
        }

        /// <summary>몬스터 1마리 = 종족 가치 + 장착 장비 가치.</summary>
        public static int MonsterValue(MonsterState monster)
        {
            if (monster == null)
                return 0;

            var def = GameDefs.GetMonster(monster.speciesId);
            int value = def != null ? def.AssetValue : 0;
            value += EquipmentValue(monster.weaponId);
            value += EquipmentValue(monster.armorId);
            return value;
        }

        public static int InventoryValue(GameState state)
        {
            int total = 0;
            for (int i = 0; i < state.inventory.Count; i++)
                total += EquipmentValue(state.inventory[i]);
            return total;
        }

        public static int EquipmentValue(string equipmentId)
        {
            var def = GameDefs.GetEquipment(equipmentId);
            return def != null ? def.FixedValue : 0;
        }
    }
}
