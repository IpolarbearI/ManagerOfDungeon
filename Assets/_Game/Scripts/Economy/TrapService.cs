namespace ManagerOfDungeon
{
    /// <summary>
    /// 함정 구매·철거·재설치 (04-8 + 11-10 개정).
    /// 미발동 함정 철거 = 구매가 100% 환불. 발동한 함정은 전투 후 실루엣(used)으로 남고,
    /// 재설치 비용은 최초 구매가의 10%(가시 바닥 2솔). 자동 재설치 ON이면 정산 때 복구하고,
    /// 솔이 부족하면 실행하지 않고 실패를 알린다.
    /// </summary>
    public static class TrapService
    {
        public static bool TryBuy(GameState state, string trapId, float viewportX, float viewportY, out string message)
        {
            var def = GameDefs.GetTrap(trapId);
            if (def == null)
            {
                message = "알 수 없는 함정입니다.";
                return false;
            }

            if (state.sol < def.BuyPrice)
            {
                message = $"솔이 부족합니다. ({def.DisplayName} {def.BuyPrice}솔)";
                return false;
            }

            state.sol -= def.BuyPrice;
            var trap = new TrapState();
            trap.id = state.NewId("trap");
            trap.trapId = trapId;
            trap.posX = viewportX;
            trap.posY = viewportY;
            trap.used = false;
            trap.autoReinstall = false;
            state.traps.Add(trap);
            message = $"{def.DisplayName} 설치 (-{def.BuyPrice}솔)";
            state.AddLog(message);
            return true;
        }

        public static bool TryRemove(GameState state, string trapStateId, out string message)
        {
            var trap = state.FindTrap(trapStateId);
            if (trap == null)
            {
                message = "함정을 찾을 수 없습니다.";
                return false;
            }

            var def = GameDefs.GetTrap(trap.trapId);
            int refund = !trap.used && def != null ? def.BuyPrice : 0;
            state.sol += refund;
            state.traps.Remove(trap);
            message = refund > 0 ? $"함정 철거 (+{refund}솔 환불)" : "사용된 함정 흔적을 치웠습니다.";
            state.AddLog(message);
            return true;
        }

        public static bool TryReinstall(GameState state, string trapStateId, out string message)
        {
            var trap = state.FindTrap(trapStateId);
            var def = trap != null ? GameDefs.GetTrap(trap.trapId) : null;
            if (trap == null || def == null)
            {
                message = "함정을 찾을 수 없습니다.";
                return false;
            }

            if (!trap.used)
            {
                message = "이미 작동 가능한 함정입니다.";
                return false;
            }

            int price = def.ReinstallPrice;
            if (state.sol < price)
            {
                message = $"솔이 부족합니다. (재설치 {price}솔)";
                return false;
            }

            state.sol -= price;
            trap.used = false;
            message = $"{def.DisplayName} 재설치 (-{price}솔)";
            state.AddLog(message);
            return true;
        }

        public static void ToggleAuto(GameState state, string trapStateId)
        {
            var trap = state.FindTrap(trapStateId);
            if (trap != null)
                trap.autoReinstall = !trap.autoReinstall;
        }

        /// <summary>정산 단계에서 자동 재설치 처리. 복구한 개수를 돌려주고, 솔 부족으로 못 한 개수는 failed.</summary>
        public static int AutoReinstall(GameState state, out int failed)
        {
            int count = 0;
            failed = 0;
            for (int i = 0; i < state.traps.Count; i++)
            {
                var trap = state.traps[i];
                if (!trap.used || !trap.autoReinstall)
                    continue;
                var def = GameDefs.GetTrap(trap.trapId);
                if (def == null)
                    continue;
                if (state.sol < def.ReinstallPrice)
                {
                    failed++;
                    continue;
                }

                state.sol -= def.ReinstallPrice;
                trap.used = false;
                count++;
            }

            return count;
        }
    }
}
