using UnityEngine;
using UniRx;
using System;
using _Project.Scripts.Runtime.Features.Farm; // Путь к твоей тыкве
using Random = UnityEngine.Random;
using _Project.Scripts.Runtime.Core;
using _Project.Scripts.Runtime.Features.Upgrades;
using _Project.Scripts.Runtime.Features.Audio;
using VContainer;
using _Project.Scripts.Runtime.Features.Economy;

namespace _Project.Scripts.Runtime.Features.Economy
{
    public class DonationController : MonoBehaviour
    {

        private Pumpkin _pumpkin;     
        private UpgradeManager _upgradeManager;
        private IWalletService _walletManager;

        [Header("Настройки донатов")]
        [SerializeField] private float _checkInterval = 4f; // Раз в 4 секунд
        [SerializeField] private float _donationChance = 0.25f; // 25% шанс
        [SerializeField] private float _minSizeForDonations = 0.5f; // Начиная с 0.5 метров

        [Inject]
        private void Construct(IWalletService wallet, UpgradeManager upgrade, Pumpkin pumpkin)
        {
            _walletManager = wallet;
            _upgradeManager = upgrade;
            _pumpkin = pumpkin;
            
        }

        private void Start()
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
                return Random.Range(1, 5);
            }
            if (size >= 2f && size < 4f)
            {
                return Random.Range(4, 10);
            }
            if (size >= 4f && size < 7f)
            {
                return Random.Range(7, 15);
            }
            if (size >= 7f && size < 10f)
            {
                return Random.Range(10, 20);
            }

            float logBonus = Mathf.Log(size - 8f, 1.28f);

            int minCoins = Mathf.FloorToInt(20f + logBonus * 2.2f);
            int maxCoins = Mathf.FloorToInt(25f + logBonus * 3.2f);

            int finalCoins = Random.Range(minCoins, maxCoins + 1);

            // Учитываем апгрейд VR-очков (DonationBonus)
            if (_upgradeManager != null)
            {
                float glassesBonus = _upgradeManager.GetEffectValue(UpgradeEffectType.DonationBonus);
                finalCoins = Mathf.FloorToInt(finalCoins * (1f + glassesBonus));
            }

            return Mathf.Min(finalCoins, 250);
        }
    }
}
