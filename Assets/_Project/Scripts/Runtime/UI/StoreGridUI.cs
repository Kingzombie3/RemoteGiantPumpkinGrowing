using UnityEngine;
using TMPro;
using UniRx;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Runtime.Features.Upgrades;
using _Project.Scripts.Runtime.Features.Economy;
using _Project.Scripts.Runtime.Core;
using _Project.Scripts.Runtime.Features.UI;
using VContainer;
using VContainer.Unity;

namespace _Project.Scripts.Runtime.UI
{
    public class StoreGridUI : MonoBehaviour
    {
        [Header("Ссылки на логику")]
        [SerializeField] private UpgradeManager _upgradeManager;
        [SerializeField] private WalletService _walletManager;

        [Header("База данных апгрейдов")]
        [Tooltip("Перетащи сюда все созданные ScriptableObjects апгрейдов")]
        [SerializeField] public List<UpgradeData> _allUpgrades;
        public List<UpgradeData> AllUpgrades => _allUpgrades;

        [Header("Компоненты интерфейса")]
        [SerializeField] private TMP_Dropdown _categoryDropdown;
        [SerializeField] private Transform _itemsContainer; // Объект с компонентом Vertical или Grid Layout Group

        [Header("Префаб")]
        [SerializeField] private UpgradeItemUI _upgradeItemPrefab;

        private IObjectResolver _container;

        private readonly List<GameObject> _spawnedItems = new List<GameObject>();

        [Inject]
        private void Construct(IObjectResolver container)
        {
            _container = container;
        }
        void Start()
        {
            if (_upgradeManager == null || _categoryDropdown == null || _itemsContainer == null || _upgradeItemPrefab == null)
            {
                Debug.LogError($"[{nameof(StoreGridUI)}] Не все ссылки настроены в магазине!");
                return;
            }

            InitializeDropdown();

            // РЕАКТИВНО: Слушаем смену категории в выпадающем списке
            _categoryDropdown.onValueChanged.AsObservable()
                .Subscribe(index => RefreshStoreGrid((UpgradeCategory)index))
                .AddTo(this);

            // Показываем первую категорию при старте
            RefreshStoreGrid(0);
        }

        private void InitializeDropdown()
        {
            _categoryDropdown.ClearOptions();

            // Автоматически заполняем Dropdown именами категорий из Enum
            List<string> options = System.Enum.GetNames(typeof(UpgradeCategory))
                .Select(name => TranslateCategory(name))
                .ToList();

            _categoryDropdown.AddOptions(options);
        }

        private void RefreshStoreGrid(UpgradeCategory selectedCategory)
        {
            // 1. Очищаем старые префабы
            foreach (var item in _spawnedItems)
            {
                Destroy(item);
            }
            _spawnedItems.Clear();

            // 2. Фильтруем апгрейды по выбранной категории
            var filteredUpgrades = _allUpgrades.Where(u => u.Category == selectedCategory).ToList();

            // 3. Спавним префабы для отфильтрованных апгрейдов
            foreach (var upgradeData in filteredUpgrades)
            {
                UpgradeItemUI newItem = _container.Instantiate(_upgradeItemPrefab, _itemsContainer);
                newItem.Setup(upgradeData, _upgradeManager);
                _spawnedItems.Add(newItem.gameObject);
            }
        }

        // Вспомогательный метод для красивого отображения категорий в интерфейсе
        private string TranslateCategory(string enumName)
        {
            return enumName switch
            {
                "FarmBase" => "Грядка",
                "BioHacking" => "Био-хакинг",
                "Automation" => "Удаленный софт",
                "Climate" => "Климат-контроль",
                "StreamMedia" => "Стрим-Медиа",
                _ => enumName
            };
        }
    }
}
