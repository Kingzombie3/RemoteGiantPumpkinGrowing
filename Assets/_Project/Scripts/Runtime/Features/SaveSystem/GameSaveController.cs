using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using VContainer.Unity;
using _Project.Scripts.Runtime.Core.SaveSystem;
using _Project.Scripts.Runtime.Features.Farm;
using _Project.Scripts.Runtime.Infrastructure.SaveSystem;
using _Project.Scripts.Runtime.Features.Economy;
using _Project.Scripts.Runtime.Features.Upgrades;

namespace _Project.Scripts.Runtime.Features.SaveSystem
{
    public class GameSaveController : IStartable, IDisposable
    {
        private Pumpkin _pumpkin;
        private IWalletService _walletService;
        private UpgradeManager _upgradeManager;
        private SaveService _saveService; // Инфраструктурный сервис ниже слоем

        private CancellationTokenSource _autoSaveCts;

        public GameSaveController(
            Pumpkin pumpkin,
            IWalletService walletService,
            UpgradeManager upgradeManager,
            SaveService saveService)
        {
            _pumpkin = pumpkin;
            _walletService = walletService;
            _upgradeManager = upgradeManager;
            _saveService = saveService;
        }

        public void Start()
        {
            LoadGame();

            // Запускаем асинхронный фоновый автосейв через UniTask
            _autoSaveCts = new CancellationTokenSource();
            AutoSaveLoopAsync(_autoSaveCts.Token).Forget();
        }

        public void Dispose()
        {
            // Отменяем автосейв и принудительно сохраняем при выходе
            _autoSaveCts?.Cancel();
            _autoSaveCts?.Dispose();
            SaveGame();
        }

        public void SaveGame()
        {
            if (_pumpkin == null || _walletService == null || _upgradeManager == null) return;

            // Извлекаем главное и приводим к простейшим типам в одном месте!
            SaveData data = new SaveData
            {
                Coins = _walletService.Coins.Value,
                PumpkinSize = _pumpkin.PumpkinSize.Value,
                PumpkinName = _pumpkin.PumpkinName.Value,
                IsPumpkinNamed = _pumpkin.IsNamed.Value
            };

            foreach (var kvp in _upgradeManager.UpgradeLevels)
            {
                data.Upgrades.Add(new UpgradeSaveEntry(kvp.Key, kvp.Value));
            }

            // Передаем плоские данные в инфраструктуру
            _saveService.WriteToFile(data);
        }

        public void LoadGame()
        {
            SaveData data = _saveService.ReadFromFile();
            if (data == null) return;

            // Распределяем примитивы обратно по системам
            _walletService.SetCoins(data.Coins);
            _pumpkin.PumpkinSize.Value = data.PumpkinSize;
            _pumpkin.PumpkinName.Value = data.PumpkinName;
            _pumpkin.IsNamed.Value = data.IsPumpkinNamed;
            _upgradeManager.LoadUpgradeLevelsDirectly(data.Upgrades);
        }

        public void ClearSaveProgress()
        {
            if (_saveService == null) return;

            // Даем команду слою ниже физически стереть бинарник
            _saveService.DeleteSaveFile();
        }

        // Фоновый цикл автосейва на UniTask
        private async UniTaskVoid AutoSaveLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // Ждем 30 секунд незаметно для основного потока игры
                await UniTask.Delay(TimeSpan.FromSeconds(30f), cancellationToken: token);

                SaveGame();
                Debug.Log("<color=#00FFFF>[SaveSystem]</color> Автосейв успешно выполнен.");
            }
        }
    }
}
