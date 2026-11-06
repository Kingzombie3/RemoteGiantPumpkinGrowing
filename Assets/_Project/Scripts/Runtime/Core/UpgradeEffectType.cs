namespace _Project.Scripts.Runtime.Core
{
    public enum UpgradeEffectType
    {
        ClickWaterBonus,      // Увеличение клика полива
        ClickWeedBonus,       // Увеличение клика прополки
        BaseGrowthMultiplier, // Множитель роста
        ReduceThirstSpeed,    // Снижение скорости высыхания
        ReduceWeedSpeed,      // Снижение скорости зарастания
        ReduceRainPenalty,    // Защита от дождя
        ReduceHeatPenalty,    // Защита от жары
        DonationBonus,        // Бонус к донатам
        DonationChanceBonus,  // Бонус к шансу доната
        UnlockBalcony,        // Открытие балкона
        TimeWarp              // Ускорение времени
    }
}
