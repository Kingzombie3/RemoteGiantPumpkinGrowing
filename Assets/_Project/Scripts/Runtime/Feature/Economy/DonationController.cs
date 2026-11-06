using UnityEngine;
using UniRx;
using System;
using _Project.Scripts.Runtime.Feature.Farm; // Путь к твоей тыкве
using Random = UnityEngine.Random;
using _Project.Scripts.Runtime.Core;
using _Project.Scripts.Runtime.Features.Upgrades;
using _Project.Scripts.Runtime.Features.Audio;

namespace _Project.Scripts.Runtime.Features.Economy
{
    public class DonationController : MonoBehaviour
    {
        [Header("Ссылки")]
        [SerializeField] private Pumpkin _pumpkin;
        [SerializeField] private WalletManager _walletManager;
        [SerializeField] private UpgradeManager _upgradeManager;

        [Header("Настройки донатов")]
        [SerializeField] private float _checkInterval = 4f; // Раз в 4 секунд
        [SerializeField] private float _donationChance = 0.25f; // 25% шанс
        [SerializeField] private float _minSizeForDonations = 1.0f; // Начиная с 1 метра

        void Start()
        {
            if (_pumpkin == null || _walletManager == null)
            {
                Debug.LogError($"[{nameof(DonationController)}] Не все ссылки назначены!");
                return;
            }

            // Запускаем реактивный таймер проверок
            Observable.Interval(TimeSpan.FromSeconds(_checkInterval))
                .ObserveOnMainThread()
                .Subscribe(_ => TryReceiveDonation())
                .AddTo(this);
        }

        private void TryReceiveDonation()
        {
            float size = _pumpkin.PumpkinSize.Value;

            // Проверяем базовое условие размера
            if (size < _minSizeForDonations)
            {
                //Debug.Log("Не фортануло");
                return;
            }

            float upgradeChanceBonus = _upgradeManager != null ? _upgradeManager.GetEffectValue(UpgradeEffectType.DonationChanceBonus) : 0f;
            float finalDonationChance = _donationChance + upgradeChanceBonus;

            if (Random.value > finalDonationChance) return;

            AudioManager.PlayRandomSound("donate", minVolume: 0.8f, maxVolume: 1.0f, minPitch: 0.9f, maxPitch: 1.1f);

            int donationAmount = CalculateDonation(size);
            _walletManager.AddCoins(donationAmount);
        }


        private int CalculateDonation(float size)
        {
            // Используем формулу: БазовыйРанг + Log(Размер) * Множитель.

            if (size >= 1f && size < 2f)
            {
                return Random.Range(1, 4);
            }
            if (size >= 2f && size < 4f)
            {
                return Random.Range(3, 7);
            }

            // Если размер >= 4 метров, включаем логарифмическое сдерживание:
            // При размере 2м: даст ~4-6 монет.
            // При размере 100м: даст ~15-20 монет.
            // При размере 10,000м: даст ~35-40 монет (рост замедлился неприлично сильно).
            float logBonus = Mathf.Log(size, 2f);

            int minCoins = Mathf.FloorToInt(3f + logBonus);
            int maxCoins = Mathf.FloorToInt(5f + (logBonus * 1.5f));

            //лимит на донат 100 монет
            int finalCoins = Random.Range(minCoins, maxCoins + 1);
            return Mathf.Min(finalCoins, 100);
        }
    }
}
