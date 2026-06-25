public static class HudComposition
{
    public static void Configure(HudView hudView, HealthComponent playerHealth, PlayerCombatSystem playerCombatSystem)
    {
        if (hudView == null)
        {
            return;
        }

        hudView.Initialize(playerHealth, playerCombatSystem);
    }
}
