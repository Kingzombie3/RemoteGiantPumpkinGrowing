using UnityEngine;
using TMPro;
using UniRx;

namespace _Project.Scripts.Runtime.Features.Economy
{
    public class SecondScreenWalletUI : MonoBehaviour
    {
        [Header("Ссылки")]
        [SerializeField] private WalletManager _walletManager;

        [Header("UI элементы")]
        [SerializeField] private TMP_Text _coinBalanceText_1, _coinBalanceText_2;

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
