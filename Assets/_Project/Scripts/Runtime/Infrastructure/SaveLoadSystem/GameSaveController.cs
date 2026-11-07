using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using _Project.Scripts.Runtime.Features.Farm;
using _Project.Scripts.Runtime.Features.Economy;
using _Project.Scripts.Runtime.Features.Upgrades;
using _Project.Scripts.Runtime.Features.UI;
using _Project.Scripts.Runtime.Features.Economy;
using _Project.Scripts.Runtime.Features.SaveLoadSystem;
using VContainer;

namespace _Project.Scripts.Runtime.Infrastructure.SaveLoadSystem
{
    public class GameSaveController : IStartable, IDisposable
    {
        private Pumpkin _pumpkin;
        private IWalletService _walletService; // Запрашиваем интерфейс кошелька
        private UpgradeManager _upgradeManager;
        private RoomNavigationUI _roomNavigationUI;

        private string _savePath;

        [Inject]
        private void Construct(
            Pumpkin pumpkin,
            IWalletService walletService,
            UpgradeManager upgradeManager,
            RoomNavigationUI roomNavigationUI)
        {
            _pumpkin = pumpkin;
            _walletService = walletService;
            _upgradeManager = upgradeManager;
            _roomNavigationUI = roomNavigationUI;

            _savePath = Path.Combine(Application.persistentDataPath, "pumpkin_save.dat");
        }

        // Автоматическая загрузка при старте сцены
        public void Start()
        {
            LoadGame();
        }

        // Автоматическое сохранение при выходе из игры или смене сцены
        public void Dispose()
        {
            SaveGame();
        }

        public void SaveGame()
        {
            if (_pumpkin == null || _walletService == null || _upgradeManager == null) return;

            SaveData saveData = new SaveData();
            saveData.Coins = _walletService.Coins.Value;
            saveData.PumpkinSize = _pumpkin.PumpkinSize.Value;

            foreach (var kvp in _upgradeManager.UpgradeLevels)
            {
                saveData.Upgrades.Add(new UpgradeSaveEntry(kvp.Key, kvp.Value));
            }

            if (_roomNavigationUI != null)
            {
                saveData.IsBalconyUnlocked = _roomNavigationUI.IsBalconyUnlocked.Value;
            }

            try
            {
                string json = JsonUtility.ToJson(saveData, false);
                byte[] bytesToSave = Encoding.UTF8.GetBytes(json);
                XorEncrypt(bytesToSave);
                File.WriteAllBytes(_savePath, bytesToSave);
                Debug.Log("[VContainer-Save] Игра успешно засейвлена в бинарный файл.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[VContainer-Save] Ошибка при сохранении: {e.Message}");
            }
        }

        public void LoadGame()
        {
            if (!File.Exists(_savePath))
            {
                Debug.Log("[VContainer-Save] Файл сохранения не найден. Начало новой игры.");
                return;
            }

            try
            {
                byte[] loadedBytes = File.ReadAllBytes(_savePath);
                XorEncrypt(loadedBytes);
                string json = Encoding.UTF8.GetString(loadedBytes);
                SaveData data = JsonUtility.FromJson<SaveData>(json);

                // УЧТЕНО: Используем твое имя метода SetCoins!
                _walletService.SetCoins(data.Coins);
                _pumpkin.PumpkinSize.Value = data.PumpkinSize;

                if (_upgradeManager != null)
                {
                    _upgradeManager.LoadUpgradeLevelsDirectly(data.Upgrades);
                }

                if (_roomNavigationUI != null && data.IsBalconyUnlocked)
                {
                    _roomNavigationUI.UnlockBalcony();
                }

                Debug.Log("[VContainer-Save] Прогресс успешно загружен и дешифрован.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[VContainer-Save] Ошибка при загрузке: {e.Message}");
            }
        }
        public void ClearSaveProgress()
        {
            try
            {
                // 1. Физически удаляем файл с диска
                if (File.Exists(_savePath))
                {
                    File.Delete(_savePath);
                    Debug.Log("<color=#FF0000>[SaveSystem]</color> Файл сохранения успешно удален с диска!");
                }

                // 2. Обнуляем текущие значения в рантайме (чтобы не пришлось перезапускать сцену)
                _walletService?.SetCoins(0);
                if (_pumpkin != null) _pumpkin.PumpkinSize.Value = 0.1f; // Стартовый размер тыквы

                // Передаем пустой список, чтобы обнулить уровни в реактивном словаре
                _upgradeManager?.LoadUpgradeLevelsDirectly(new List<UpgradeSaveEntry>());

                Debug.Log("<color=#00FFFF>[SaveSystem]</color> Все рантайм-системы сброшены до стартового состояния.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Ошибка при сбросе прогресса: {e.Message}");
            }
        }


        private void XorEncrypt(byte[] bytes)
        {
            byte key = 127;
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(bytes[i] ^ key);
            }
        }
    }
}
