using UnityEngine;
using UniRx;
using System;
using DG.Tweening;
using _Project.Scripts.Runtime.Core.Upgrades;
using _Project.Scripts.Runtime.Features.Upgrades;
using _Project.Scripts.Runtime.Core.Weather;
using VContainer;

namespace _Project.Scripts.Runtime.Features.Farm
{
    public class Pumpkin : MonoBehaviour
    {
        [Header("Основные параметры")]
        [SerializeField] private float _baseGrowSpeed = 0.01f; // Базовая скорость
        public ReactiveProperty<float> PumpkinSize { get; } = new ReactiveProperty<float>(0.01f);
        public ReactiveProperty<float> PumpkinThirsting { get; } = new ReactiveProperty<float>(1f); // 1 = ок, 0 = сухая
        public ReactiveProperty<float> PumpkinWeeding { get; } = new ReactiveProperty<float>(1f);   // 1 = чисто, 0 = заросла
        public ReactiveProperty<float> PumpkinHappiness { get; } = new ReactiveProperty<float>(1f);
        public ReactiveProperty<string> PumpkinName = new ReactiveProperty<string>("");
        public ReactiveProperty<bool> IsNamed = new ReactiveProperty<bool>(false);
        public readonly ReactiveProperty<float> CurrentTickInterval = new ReactiveProperty<float>(1.0f);


        [SerializeField] private float _size => PumpkinSize.Value;
        

        [Header("Скорость ухудшения параметров (ед/сек)")]
        [SerializeField] private float _thirstDepletionSpeed = 0.1f;
        [SerializeField] private float _weedGrowthSpeed = 0.1f;      

        public WeatherType CurrentWeather = WeatherType.Normal;

        private readonly SerialDisposable _timerDisposable = new SerialDisposable();

        private UpgradeManager _upgradeManager;
        private WeatherController _weatherController;

        [SerializeField] private float _maxPumpkinSize = 1000f;

        [Header("READ ONLY")]
        [SerializeField] private float _totalGrow = 0;
        [SerializeField] private float _totalThirst = 0;
        [SerializeField] private float _totalGrassing = 0;

        [Header("Glasses")]
        [SerializeField] private GameObject _glassesObj;

        [Inject]
        public void Construct(WeatherController weatherController, UpgradeManager upgradeManager)
        {
            _weatherController = weatherController;
            _upgradeManager = upgradeManager;
        }

        private void Start()
        {
            if (_upgradeManager != null)
            {
                _upgradeManager.UpgradeLevels
                    .ObserveEveryValueChanged(dict => _upgradeManager.GetUpgradeLevel("upgrade_clock"))
                    .Subscribe(clockLevel =>
                    {
                        float interval = clockLevel > 0 ? 0.5f : 1.0f;
                        RestartGameTimer(interval);
                    })
                    .AddTo(this);
            }
            else
            {
                RestartGameTimer(1.0f);
            }

            UpdateGlassesVisibility();

            if (_upgradeManager != null)
            {
                Observable.Merge(
                    _upgradeManager.UpgradeLevels.ObserveAdd().AsUnitObservable(),
                    _upgradeManager.UpgradeLevels.ObserveReplace().AsUnitObservable(),
                    _upgradeManager.UpgradeLevels.ObserveReset().AsUnitObservable()
                )
                .Subscribe(_ => UpdateGlassesVisibility())
                .AddTo(this);
            }

            PumpkinSize
                .Subscribe(size => transform.DOScale(Vector3.one * size, 0.8f).SetEase(Ease.InSine))
                .AddTo(this);

            _weatherController.CurrentWeather
                .Subscribe(weather => CurrentWeather = weather)
                .AddTo(this);
        }

        private void ProcessGameTick()
        {
            if (PumpkinSize.Value >= _maxPumpkinSize) return;

            WeatherProcessing(out float thirstModifier, out float weedModifier, out float growthModifier);

            float growthDelta = CalculateGameTick(
                thirstModifier,
                weedModifier,
                growthModifier,
                CurrentWeather
            );

            if (growthDelta > 0f)
            {
                PumpkinSize.Value = Mathf.Min(_maxPumpkinSize, PumpkinSize.Value + growthDelta);
            }
        }
        private void RestartGameTimer(float intervalSeconds)
        {
            CurrentTickInterval.Value = intervalSeconds;

            _timerDisposable.Disposable = Observable.Interval(TimeSpan.FromSeconds(intervalSeconds))
                .Where(_ => IsNamed.Value)
                .ObserveOnMainThread()
                .Subscribe(_ => ProcessGameTick());

            Debug.Log($"[TimeSystem] Игровой цикл переключен на частоту: {intervalSeconds} сек.");
        }
        public void ChangeBehaviour(WeatherType weather)
        {
            CurrentWeather = weather;
        }

        private void WeatherProcessing(
            out float thirstModifier,
            out float weedModifier,
            out float growthModifier)
        {
            switch (CurrentWeather)
            {
                case WeatherType.Rain:
                    PumpkinThirsting.Value = 1f; // Дождь: автополив
                    thirstModifier = 0f;         // Жажда не уменьшается

                    // АПГРЕЙД "Анти-Дождь": снижает штраф бурного роста сорняков
                    float rainDefense = _upgradeManager != null ? _upgradeManager.GetEffectValue(UpgradeEffectType.ReduceRainPenalty) : 0f;
                    weedModifier = Mathf.Max(0.5f, 2.5f - rainDefense);

                    growthModifier = 1.2f;       // Рост +20%
                    break;

                case WeatherType.Heat:
                    // АПГРЕЙД "Кулер": снижает скорость высыхания на жаре
                    float thirstDefense = _upgradeManager != null ? _upgradeManager.GetEffectValue(UpgradeEffectType.ReduceHeatPenalty) : 0f;
                    thirstModifier = Mathf.Max(0.5f, 3f - thirstDefense);

                    weedModifier = 0.5f;

                    // АПГРЕЙД "Кулер": возвращает скорость роста, компенсируя пекло
                    float growthRecovery = _upgradeManager != null ? _upgradeManager.GetEffectValue(UpgradeEffectType.ReduceHeatPenalty) : 0f;
                    growthModifier = Mathf.Min(1.0f, 0.5f + growthRecovery);
                    break;

                case WeatherType.Normal:
                default:
                    thirstModifier = 1f;
                    weedModifier = 1f;
                    growthModifier = 1f;
                    break;
            }
        }

        public void SetPumpkinName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                PumpkinName.Value = "Тыковка";
            }
            else
            {
                string trimmed = newName.Trim();
                PumpkinName.Value = trimmed.Length > 30 ? trimmed.Substring(0, 30) : trimmed;
            }

            IsNamed.Value = true;
        }

        private float CalculateGameTick(
            float thirstMod,
            float weedMod,
            float growthMod,
            WeatherType weather)
        {

            // Запрашиваем уровни автоматизации (если менеджер не назначен, бонусы будут равны 0)
            float autoWaterBonus = _upgradeManager != null ? _upgradeManager.GetEffectValue(UpgradeEffectType.ReduceThirstSpeed) : 0f;
            float autoWeedBonus = _upgradeManager != null ? _upgradeManager.GetEffectValue(UpgradeEffectType.ReduceWeedSpeed) : 0f;

            // Расчет пассивного ухода с учетом «Волшебных часов»
            float finalThirstDepletion = _thirstDepletionSpeed * Mathf.Max(0.1f, 1f - autoWaterBonus);
            float finalWeedDepletion = _weedGrowthSpeed * Mathf.Max(0.1f, 1f - autoWeedBonus);

            _totalThirst = finalThirstDepletion;
            _totalGrassing = finalWeedDepletion;

            if (weather != WeatherType.Rain)
            {
                PumpkinThirsting.Value = Mathf.Max(0f, PumpkinThirsting.Value - (finalThirstDepletion * thirstMod));
            }
            PumpkinWeeding.Value = Mathf.Max(0f, PumpkinWeeding.Value - (finalWeedDepletion * weedMod));

            PumpkinHappiness.Value = (PumpkinThirsting.Value + PumpkinWeeding.Value) / 2f;

            float thirstMultiplier = PumpkinThirsting.Value;
            float weedMultiplier = Mathf.Lerp(0.2f, 1.0f, PumpkinWeeding.Value);

            // fertilizers
            float fertilizerBonus = _upgradeManager != null ? _upgradeManager.GetEffectValue(UpgradeEffectType.BaseGrowthMultiplier) : 0f;
            float finalGrowthMultiplier = growthMod * (1f + fertilizerBonus);

            float totalMultiplier = thirstMultiplier * weedMultiplier * finalGrowthMultiplier;

            _totalGrow = totalMultiplier;

            if (totalMultiplier <= 0f) return 0f;

            return ((_baseGrowSpeed / Mathf.Log(PumpkinSize.Value + 1.1f)) * totalMultiplier);
        }

        private void UpdateGlassesVisibility()
        {
            if (_glassesObj == null) return;

            bool hasGlasses = _upgradeManager != null && _upgradeManager.GetEffectValue(UpgradeEffectType.DonationBonus) > 0f;
            _glassesObj.SetActive(hasGlasses);
        }


        private void OnDestroy()
        {
            _timerDisposable.Dispose();
        }

        private void OnApplicationQuit()
        {
            
        }
    }
}
