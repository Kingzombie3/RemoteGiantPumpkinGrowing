using UnityEngine;
using UniRx;

namespace _Project.Scripts.Runtime.Features.Economy
{
    public class WalletManager : MonoBehaviour
    {
        public IReadOnlyReactiveProperty<int> Coins => _coins;
        private readonly ReactiveProperty<int> _coins = new ReactiveProperty<int>(0);

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;

            _coins.Value += amount;
            Debug.Log($"[Wallet] Получен донат: +{amount} монет. Баланс: {_coins.Value}");
        }

        public bool SpendCoins(int amount)
        {
            if (amount <= 0 || _coins.Value < amount) return false;

            _coins.Value -= amount;
            return true;
        }
    }
}
