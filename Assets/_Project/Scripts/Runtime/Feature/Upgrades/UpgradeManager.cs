using UnityEngine;
using UniRx;
using _Project.Scripts.Runtime.Features.Economy;
using _Project.Scripts.Runtime.Features.UI;
using _Project.Scripts.Runtime.Core;
using _Project.Scripts.Runtime.UI;
using System.Linq;
using _Project.Scripts.Runtime.Features.Audio;

namespace _Project.Scripts.Runtime.Features.Upgrades
{
    public class UpgradeManager : MonoBehaviour
    {
        [Header("Ññûëêè")]
        [SerializeField] private WalletManager _walletManager;
        [SerializeField] private RoomNavigationUI _roomNavigationUI;
        [SerializeField] private StoreGridUI _storeGridUI;

        private readonly ReactiveDictionary<string, int> _upgradeLevels = new ReactiveDictionary<string, int>();

        public IReadOnlyReactiveDictionary<string, int> UpgradeLevels => _upgradeLevels;

        public int GetUpgradeLevel(string id)
        {
            return _upgradeLevels.ContainsKey(id) ? _upgradeLevels[id] : 0;
        }

        public bool TryBuyUpgrade(UpgradeData upgrade)
        {
            int currentLevel = GetUpgradeLevel(upgrade.ID);

            if (currentLevel >= upgrade.MaxLevel) return false;

            int price = upgrade.GetPrice(currentLevel);

            if (_walletManager.SpendCoins(price))
            {
                if (_upgradeLevels.ContainsKey(upgrade.ID))
                    _upgradeLevels[upgrade.ID]++;
                else
                    _upgradeLevels.Add(upgrade.ID, 1);

                HandleSpecialUpgradeUnlocks(upgrade, currentLevel + 1);

                AudioManager.PlayRandomSound("buy", minVolume: 0.8f, maxVolume: 1.0f, minPitch: 0.9f, maxPitch: 1.1f);

                Debug.Log($"[Upgrades] Êóïëåí àïãðåéä {upgrade.UpgradeName}. Íîâûé ëâë: {GetUpgradeLevel(upgrade.ID)}");
                return true;
            }

            return false;
        }

        private void HandleSpecialUpgradeUnlocks(UpgradeData upgrade, int newLevel)
        {
            if (upgrade.EffectType == UpgradeEffectType.UnlockBalcony && newLevel > 0)
            {
                if (_roomNavigationUI != null)
                {
                    _roomNavigationUI.UnlockBalcony();
                    Debug.Log("[UpgradeTrigger] Íàâèãàöèÿ íà áàëêîí ðàçáëîêèðîâàíà!");
                }
            }
        }

        // --- ÏÓÁËÈ×ÍÛÅ ÌÅÒÎÄÛ ÏÎËÓ×ÅÍÈß ÌÎÄÈÔÈÊÀÒÎÐÎÂ ÄËß ÒÛÊÂÛ ---

        public float GetClickWaterPower() => 1f + (GetUpgradeLevel("upgrade_basket") * 0.5f); // Ïðèìåð ID ëåéêè

        public float GetGrowthMultiplier() => 1f + (GetUpgradeLevel("upgrade_fertilizer") * 0.2f);

        public bool IsTimeWarpActive() => GetUpgradeLevel("upgrade_clock") > 0;

        public float GetEffectValue(UpgradeEffectType effectType)
        {
            var matchingUpgrades = _storeGridUI.AllUpgrades.Where(u => u.EffectType == effectType);

            float totalBonus = 0f;

            foreach (var upgrade in matchingUpgrades)
            {
                int currentLevel = GetUpgradeLevel(upgrade.ID);
                totalBonus += currentLevel * upgrade.ValuePerLevel;
            }

            return totalBonus;
        }
    }
}
