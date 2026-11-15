using _Project.Scripts.Runtime.Core;
using UnityEngine;

namespace _Project.Scripts.Runtime.Core.Upgrades
{
    [CreateAssetMenu(fileName = "Upgrade_", menuName = "Farm/New Upgrade")]
    public class UpgradeData : ScriptableObject
    {
        public string ID;              
        public string UpgradeName;    
        [TextArea] public string Description;
        public UpgradeCategory Category; 
        public UpgradeEffectType EffectType;

        [Header("Экономика апгрейда")]
        public int BasePrice = 10;
        public float PriceMultiplier = 1.5f; 
        public int MaxLevel = 5;

        [Header("Сила эффекта")]
        public float ValuePerLevel = 0.1f;

        [Header("Картинка")]
        public Sprite image;

        public int GetPrice(int currentLevel)
        {
            return Mathf.FloorToInt(BasePrice * Mathf.Pow(PriceMultiplier, currentLevel));
        }
    }
}
