using UnityEngine;
using UniRx;
using _Project.Scripts.Runtime.Features.Economy;
using _Project.Scripts.Runtime.Features.Audio;
using _Project.Scripts.Runtime.Features.SaveSystem;
using System.Collections.Generic;
using System.Linq;
using VContainer;
using _Project.Scripts.Runtime.Core;
using _Project.Scripts.Runtime.Core.SaveSystem;

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

        [Inject]
        public void Construct(WalletService walletService)
        {
            _walletService = walletService;
        }

        /// <summary>
        /// Принудительно загружает уровни апгрейдов из файла сохранения.
        /// </summary>
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

                AudioManager.PlayRandomSound("buy", minVolume: 0.8f, maxVolume: 1.0f, minPitch: 0.9f, maxPitch: 1.1f);

                Debug.Log($"[Upgrades] Куплен апгрейд {upgrade.UpgradeName}. Новый лвл: {GetUpgradeLevel(upgrade.ID)}");
                return true;
            }

            return false;
        }

/*        private void HandleSpecialUpgradeUnlocks(UpgradeData upgrade, int newLevel)
        {
            if (upgrade.EffectType == UpgradeEffectType.UnlockBalcony && newLevel > 0)
            {
                if (_roomNavigationUI != null)
                {
                    _roomNavigationUI.UnlockBalcony();
                    Debug.Log("[UpgradeTrigger] Навигация на балкон разблокирована!");
                }
            }
        }*/

        // --- ВОЗВРАЩЕННЫЕ МЕТОДЫ МОДИФИКАТОРОВ (Адаптированы под типы эффектов без хардкод-строк) ---

        // Сила клика лейки: базовый 1.0 + динамический бонус от всех улучшений леек
        public float GetClickWaterPower() => 1f + GetEffectValue(UpgradeEffectType.ClickWaterBonus);

        // Множитель роста: базовый 1.0 + динамический бонус от всех купленных удобрений
        public float GetGrowthMultiplier() => 1f + GetEffectValue(UpgradeEffectType.BaseGrowthMultiplier);

        // Проверка активности искривления времени (Магические часы)
        public bool IsTimeWarpActive() => GetEffectValue(UpgradeEffectType.TimeWarp) > 0f;

        // Глобальный расчет силы эффектов по их типу
        public float GetEffectValue(UpgradeEffectType effectType)
        {
            // ИСПРАВЛЕНО: Лекго и быстро берем данные из своего же списка фичи, а не лезем в UI
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
