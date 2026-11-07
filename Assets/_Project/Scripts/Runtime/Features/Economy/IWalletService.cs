using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace _Project.Scripts.Runtime.Features.Economy
{
    public interface IWalletService
    {
        IReadOnlyReactiveProperty<int> Coins { get; }

        public void AddCoins(int amount);

        public bool SpendCoins(int amount);

        public void SetCoins(int amount);
    }
}

