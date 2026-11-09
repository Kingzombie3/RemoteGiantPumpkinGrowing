using UnityEngine;
using TMPro;
using UniRx;
using _Project.Scripts.Runtime.Features.Economy;
using VContainer;

namespace _Project.Scripts.Runtime.UI
{
    public class WalletUI : MonoBehaviour
    {
        private IWalletService _walletManager;

        [Header("UI элементы")]
        [SerializeField] private TMP_Text _coinBalanceText_1, _coinBalanceText_2;

        [Inject]
        private void Construct(IWalletService wallet)
        {
            _walletManager = wallet;
        }

        void Start()
        {
            if (_walletManager == null)
            {
                Debug.LogError("WalletManager не назначен в SecondScreenWalletUI!");
                return;
            }

            _walletManager.Coins
                .ObserveOnMainThread()
                .Subscribe(coins =>
                {
                    _coinBalanceText_1.text = $"Монет: {coins} ";
                    _coinBalanceText_2.text = $"Монет: {coins} ";
                })
                .AddTo(this);
        }
    }
}
