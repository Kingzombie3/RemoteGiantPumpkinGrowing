using _Project.Scripts.Runtime.Core;
using UnityEngine;

namespace _Project.Scripts.Runtime.Core
{
    [CreateAssetMenu(fileName = "Upgrade_", menuName = "Farm/New Upgrade")]
    public class UpgradeData : ScriptableObject
    {
        public string ID;                  // Уникальный ID (например, "upgrade_cooler")
        public string UpgradeName;         // Название для UI
        [TextArea] public string Description;
        public UpgradeCategory Category;   // К какой группе относится
        public UpgradeEffectType EffectType;

        [Header("Экономика апгрейда")]
        public int BasePrice = 10;
        public float PriceMultiplier = 1.5f; // На сколько дороже следующий уровень
        public int MaxLevel = 5;

        [Header("Сила эффекта")]
        public float ValuePerLevel = 0.1f;   // Значение бонуса за каждый уровень

        public int GetPrice(int currentLevel)
        {
            return Mathf.FloorToInt(BasePrice * Mathf.Pow(PriceMultiplier, currentLevel));
        }
    }
}
