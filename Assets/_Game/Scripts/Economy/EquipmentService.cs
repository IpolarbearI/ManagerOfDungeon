using System.Collections.Generic;

namespace ManagerOfDungeon
{
    /// <summary>
    /// 장비 장착·해제 (04-4). 몬스터마다 무기 1 / 방어구 1. 일상에서만 호출한다.
    /// 해제한 장비는 인벤토리로 돌아간다.
    /// </summary>
    public static class EquipmentService
    {
        public static bool TryEquip(GameState state, string monsterId, string equipmentId, out string message)
        {
            var monster = state.FindMonster(monsterId);
            var def = GameDefs.GetEquipment(equipmentId);
            if (monster == null || def == null)
            {
                message = "장착할 수 없습니다.";
                return false;
            }

            if (!state.inventory.Remove(equipmentId))
            {
                message = $"{def.DisplayName}이(가) 보관함에 없습니다.";
                return false;
            }

            if (def.Slot == EquipmentSlot.Weapon)
            {
                if (!string.IsNullOrEmpty(monster.weaponId))
                    state.inventory.Add(monster.weaponId);
                monster.weaponId = equipmentId;
            }
            else
            {
                if (!string.IsNullOrEmpty(monster.armorId))
                    state.inventory.Add(monster.armorId);
                monster.armorId = equipmentId;
            }

            message = $"{monster.name}: {def.DisplayName} 장착";
            state.AddLog(message);
            return true;
        }

        public static void UnequipWeapon(GameState state, string monsterId)
        {
            var monster = state.FindMonster(monsterId);
            if (monster == null || string.IsNullOrEmpty(monster.weaponId))
                return;
            state.inventory.Add(monster.weaponId);
            state.AddLog($"{monster.name}: 무기 해제");
            monster.weaponId = string.Empty;
        }

        /// <summary>
        /// 무기 순환: 맨손 → 보관함의 무기들 → 맨손. UI 버튼 하나로 교체할 때 쓴다.
        /// </summary>
        public static string CycleWeapon(GameState state, string monsterId)
        {
            var monster = state.FindMonster(monsterId);
            if (monster == null)
                return string.Empty;

            var weapons = new List<string>();
            for (int i = 0; i < state.inventory.Count; i++)
            {
                var def = GameDefs.GetEquipment(state.inventory[i]);
                if (def != null && def.Slot == EquipmentSlot.Weapon && !weapons.Contains(def.Id))
                    weapons.Add(def.Id);
            }

            weapons.Sort(System.StringComparer.Ordinal);
            string current = monster.weaponId ?? string.Empty;
            string next = string.Empty;
            for (int i = 0; i < weapons.Count; i++)
            {
                if (string.CompareOrdinal(weapons[i], current) > 0 || current.Length == 0)
                {
                    next = weapons[i];
                    break;
                }
            }

            if (next.Length == 0)
            {
                UnequipWeapon(state, monsterId);
                return "맨손";
            }

            TryEquip(state, monsterId, next, out var message);
            return message;
        }
    }
}
