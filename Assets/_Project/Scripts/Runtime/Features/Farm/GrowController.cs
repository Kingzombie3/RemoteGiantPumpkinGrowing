using _Project.Scripts.Runtime.Core;
using _Project.Scripts.Runtime.Features.Audio;
using _Project.Scripts.Runtime.Features.Upgrades;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace _Project.Scripts.Runtime.Features.Farm
{
    public class GrowController : MonoBehaviour
    {
        [Header("READ ONLY")]
        [SerializeField] private float _totalWater = 0;
        [SerializeField] private float _totalWeed = 0;

        private Pumpkin _pumpkin;
        private UpgradeManager _upgradeManager;

        [Inject]
        public void Construct(Pumpkin pumpkin, UpgradeManager upgradeManager)
        {
            _pumpkin = pumpkin;
            _upgradeManager = upgradeManager;
        }
        void Start()
        {
            // Если забыл перетащить скрипт, ищем его на переданном GameObject
            if (_pumpkin == null && GetComponentInChildren<Pumpkin>() != null)
            {
                _pumpkin = GetComponentInChildren<Pumpkin>();
            }
        }

        private void Update()
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }
        }

        // Вызывается по кнопке "Полить"
        public void Watering()
        {
            if (_pumpkin == null)
            {
                Debug.Log("null!");
                return;
            }

            float baseClickPower = 0.1f;
            float upgradeBonus = _upgradeManager != null ? _upgradeManager.GetEffectValue(UpgradeEffectType.ClickWaterBonus) : 0f;

            float finalWater = baseClickPower + upgradeBonus;

            _totalWater = finalWater;

            _pumpkin.PumpkinThirsting.Value = Mathf.Min(1f, _pumpkin.PumpkinThirsting.Value + finalWater); // watering

            AudioManager.PlayRandomSound("water", minVolume: 0.8f, maxVolume: 1.0f, minPitch: 0.9f, maxPitch: 1.1f);

            if (_pumpkin.PumpkinThirsting.Value == 1)
            {
                Debug.Log("Тыква полита!");
            }
        }

        // Вызывается по кнопке "Прополоть"
        public void Weeding()
        {
            if (_pumpkin == null)
            {
                Debug.Log("null!");
                return;
            }
            
            float baseClickPower = 0.1f;
            float upgradeBonus = _upgradeManager != null ? _upgradeManager.GetEffectValue(UpgradeEffectType.ClickWeedBonus) : 0f;

            float finalWeed = baseClickPower + upgradeBonus;

            _totalWeed = finalWeed;

            _pumpkin.PumpkinWeeding.Value = Mathf.Min(1f, _pumpkin.PumpkinWeeding.Value + 0.1f); // weeding

            AudioManager.PlayRandomSound("weed", minVolume: 0.8f, maxVolume: 1.0f, minPitch: 0.9f, maxPitch: 1.1f);

            if (_pumpkin.PumpkinWeeding.Value == 1) 
            {
                Debug.Log("Грядка прополота!"); 
            } 
        }
    }
}

