using UnityEngine;
using UniRx;
using _Project.Scripts.Runtime.Features.Economy;
using _Project.Scripts.Runtime.Infrastructure.Audio;
using _Project.Scripts.Runtime.Features.SaveSystem;
using System.Collections.Generic;
using System.Linq;
using VContainer;
using _Project.Scripts.Runtime.Core.SaveSystem;
using _Project.Scripts.Runtime.Core.Upgrades;
using _Project.Scripts.Runtime.Core.Audio;
using _Project.Scripts.Runtime.Features.Farm;

namespace _Project.Scripts.Runtime.Features.Upgrades
{
    public class UpgradeManager : MonoBehaviour
    {
        [SerializeField] 
        private List<UpgradeData> _allUpgrades = new List<UpgradeData>();

        private WalletService _walletService;

        private readonly ReactiveDictionary<string, int> _upgradeLevels = new ReactiveDictionary<string, int>();
        public IReadOnlyReactiveDictionary<string, int> UpgradeLevels => _upgradeLevels;
        public IReadOnlyList<UpgradeData> AllUpgrades => _allUpgrades;

        private IAudioService _audioService;

        [Inject]
        public void Construct(WalletService walletService, IAudioService audioService)
        {
            _walletService = walletService;
            _audioService = audioService;
        }

        public void LoadUpgradeLevelsDirectly(List<UpgradeSaveEntry> savedUpgrades)
        {
            _upgradeLevels.Clear();
            foreach (var entry in savedUpgrades)
            {
                if (!_upgradeLevels.ContainsKey(entry.UpgradeID))
                {
                    _upgradeLevels.Add(entry.UpgradeID, entry.Level);
                }
            }
            Debug.Log($"[UpgradeManager] Восстановлен прогресс {savedUpgrades.Count} апгрейдов из сохранения!");
        }

        public int GetUpgradeLevel(string id)
        {
            return _upgradeLevels.ContainsKey(id) ? _upgradeLevels[id] : 0;
        }

        public bool TryBuyUpgrade(UpgradeData upgrade)
        {
            if (_walletService == null)
            {
                Debug.LogError("[UpgradeManager] WalletService не внедрен через VContainer!");
                return false;
            }

            int currentLevel = GetUpgradeLevel(upgrade.ID);
            if (currentLevel >= upgrade.MaxLevel) return false;

            int price = upgrade.GetPrice(currentLevel);

            if (_walletService.SpendCoins(price))
            {
                if (_upgradeLevels.ContainsKey(upgrade.ID))
                    _upgradeLevels[upgrade.ID]++;
                else
                    _upgradeLevels.Add(upgrade.ID, 1);

                _audioService.PlayRandomSound("buy", minVolume: 0.8f, maxVolume: 1.0f, minPitch: 0.9f, maxPitch: 1.1f);

                Debug.Log($"[Upgrades] Куплен апгрейд {upgrade.UpgradeName}. Новый лвл: {GetUpgradeLevel(upgrade.ID)}");
                return true;
            }

            return false;
        }

        public float GetClickWaterPower() => 1f + GetEffectValue(UpgradeEffectType.ClickWaterBonus);

        public float GetGrowthMultiplier() => 1f + GetEffectValue(UpgradeEffectType.BaseGrowthMultiplier);

        public bool IsTimeWarpActive() => GetEffectValue(UpgradeEffectType.TimeWarp) > 0f;

        public float GetEffectValue(UpgradeEffectType effectType)
        {
            var matchingUpgrades = _allUpgrades.Where(u => u.EffectType == effectType);
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
