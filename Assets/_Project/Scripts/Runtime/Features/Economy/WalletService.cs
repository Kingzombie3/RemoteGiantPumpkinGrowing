using UnityEngine;
using UniRx;
using _Project.Scripts.Runtime.Features.Economy;

namespace _Project.Scripts.Runtime.Features.Economy
{
    public class WalletService : IWalletService
    {
        public IReadOnlyReactiveProperty<int> Coins => _coins;
        private readonly ReactiveProperty<int> _coins = new ReactiveProperty<int>(0);

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;

            _coins.Value += amount;
        }

        public bool SpendCoins(int amount)
        {
            if (amount <= 0 || _coins.Value < amount) return false;

            _coins.Value -= amount;
            return true;
        }

        public void SetCoins(int amount)
        {
            _coins.Value = amount;
        }
    }
}
