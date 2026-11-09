using UnityEngine;
using VContainer;
using _Project.Scripts.Runtime.Features.Farm;
using _Project.Scripts.Runtime.Features.Upgrades;
using _Project.Scripts.Runtime.Features.SaveSystem; // Если контроллер сохранения на слое Feature/Infrastructure
using System.Collections.Generic;
using _Project.Scripts.Runtime.Core.SaveSystem;
using _Project.Scripts.Runtime.Features.Economy;

namespace _Project.Scripts.Runtime.UI.DebugTools
{
    public class DebugProgressReset : MonoBehaviour
    {
        private GameSaveController _saveController;
        private IWalletService _walletService;
        private Pumpkin _pumpkin;
        private UpgradeManager _upgradeManager;

        // VContainer автоматически вольет сюда ВСЕ системы со слоев ниже, так как UI видит их всех!
        [Inject]
        public void Construct(
            GameSaveController saveController,
            IWalletService walletService,
            Pumpkin pumpkin,
            UpgradeManager upgradeManager)
        {
            _saveController = saveController;
            _walletService = walletService;
            _pumpkin = pumpkin;
            _upgradeManager = upgradeManager;
        }

        [ContextMenu("?? СБРОСИТЬ ПРОГРЕСС ИГРЫ")]
        public void ClearGameProgressButton()
        {
            if (Application.isPlaying)
            {
                // 1. Сначала принудительно стираем файл сохранения через контроллер
                if (_saveController != null)
                {
                    _saveController.ClearSaveProgress();
                }

                // 2. Обнуляем живые параметры в оперативной памяти (тоже в одном месте!)
                _walletService?.SetCoins(0);
                if (_pumpkin != null) _pumpkin.PumpkinSize.Value = 0.1f;
                _upgradeManager?.LoadUpgradeLevelsDirectly(new List<UpgradeSaveEntry>());

                Debug.Log("<color=#00FFFF>[Debug]</color> Прогресс успешно сброшен в рантайме!");
                return;
            }

            // Логика работы прямо в редакторе без запуска игры (срабатывает по-прежнему)
            string editorSavePath = System.IO.Path.Combine(Application.persistentDataPath, "pumpkin_save.dat");
            if (System.IO.File.Exists(editorSavePath))
            {
                System.IO.File.Delete(editorSavePath);
                Debug.Log("<color=#FF0000>[Editor]</color> Файл сохранения удален в режиме редактирования.");
            }
            else
            {
                Debug.Log("<color=#999999>[Editor]</color> Файл сохранения не найден.");
            }
        }
    }
}
