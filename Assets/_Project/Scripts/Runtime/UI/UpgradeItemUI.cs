using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using _Project.Scripts.Runtime.Features.Upgrades;
using VContainer;
using _Project.Scripts.Runtime.Features.Economy;
using _Project.Scripts.Runtime.Core.Upgrades;

namespace _Project.Scripts.Runtime.UI
{
    public class UpgradeItemUI : MonoBehaviour
    {
        [Header("UI Элементы префаба")]
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private TMP_Text _costText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private Button _buyButton;
        [SerializeField] private Image _image;

        private UpgradeData _data;
        private UpgradeManager _manager;
        private IWalletService _wallet;

        [Inject]
        private void Constuct(IWalletService wallet, UpgradeManager manager)
        {
            _manager = manager;
            _wallet = wallet;
        }

        public void Setup(UpgradeData data)
        {
            _data = data;

            // Запускаем реактивное обновление этого конкретного префаба
            _manager.UpgradeLevels.ObserveEveryValueChanged(dict => _manager.GetUpgradeLevel(_data.ID))
                .Subscribe(_ => UpdateUI())
                .AddTo(this);

            // Реактивно следим за кошельком, чтобы включать/выключать кнопку покупки
            _wallet.Coins
                .Subscribe(coins => CheckPurchaseAvailability(coins))
                .AddTo(this);

            // Клик по кнопке покупки
            _buyButton.OnClickAsObservable()
                .Subscribe(_ => _manager.TryBuyUpgrade(_data))
                .AddTo(this);

            _image.sprite = _data.image;
        }

        private void UpdateUI()
        {
            int currentLevel = _manager.GetUpgradeLevel(_data.ID);

            _nameText.text = _data.UpgradeName;
            _descriptionText.text = _data.Description;

            if (currentLevel >= _data.MaxLevel)
            {
                _levelText.text = $"УР: МАКС";
                _costText.text = " ";
                _buyButton.interactable = false;
            }
            else
            {
                _levelText.text = $"УР: {currentLevel}/{_data.MaxLevel}";
                _costText.text = $"{_data.GetPrice(currentLevel)} мон.";
                CheckPurchaseAvailability(_wallet.Coins.Value);
            }
        }

        private void CheckPurchaseAvailability(int currentCoins)
        {
            int currentLevel = _manager.GetUpgradeLevel(_data.ID);
            if (currentLevel >= _data.MaxLevel) return;

            // Кнопка активна только если хватает монет
            _buyButton.interactable = currentCoins >= _data.GetPrice(currentLevel);
        }
    }
}
