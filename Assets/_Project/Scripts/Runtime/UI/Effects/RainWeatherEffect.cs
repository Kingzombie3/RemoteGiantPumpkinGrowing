using UnityEngine;
using UniRx;
using DG.Tweening;
using VContainer;
using _Project.Scripts.Runtime.Core.Weather;
using _Project.Scripts.Runtime.Features.Farm;

namespace _Project.Scripts.Runtime.UI.Effects
{
    public class RainWeatherEffect : MonoBehaviour
    {
        [Header("Раздельные эмиттеры дождя")]
        [SerializeField] private ParticleSystem _farmRainParticles;
        [SerializeField] private ParticleSystem _balconyRainParticles;

        [Header("Настройки интенсивности")]
        [SerializeField] private float _maxEmissionRate = 300f;
        [SerializeField] private float _transitionDuration = 1.5f;

        private WeatherController _weatherController;
        private RoomNavigationUI _roomNavigation;

        private ParticleSystem.EmissionModule _farmEmission;
        private ParticleSystem.EmissionModule _balconyEmission;
        private Tween _farmTween;
        private Tween _balconyTween;

        [Inject]
        public void Construct(WeatherController weatherController, RoomNavigationUI roomNavigation)
        {
            _weatherController = weatherController;
            _roomNavigation = roomNavigation;
        }

        private void Awake()
        {
            SetupEmitter(_farmRainParticles, out _farmEmission);
            SetupEmitter(_balconyRainParticles, out _balconyEmission);
        }

        private void SetupEmitter(ParticleSystem ps, out ParticleSystem.EmissionModule emission)
        {
            emission = default;
            if (ps == null) return;

            emission = ps.emission;
            emission.rateOverTime = 0f;
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void Start()
        {
            if (_weatherController == null || _roomNavigation == null) return;

            Observable.CombineLatest(
                _weatherController.CurrentWeather,
                _roomNavigation.IsOnBalcony,
                (weather, isOnBalcony) => new { IsRaining = weather == WeatherType.Rain, IsOnBalcony = isOnBalcony }
            )
            .ObserveOnMainThread()
            .Subscribe(state =>
            {
                UpdateEmitterState(_farmRainParticles, _farmEmission, ref _farmTween, state.IsRaining && !state.IsOnBalcony);
                UpdateEmitterState(_balconyRainParticles, _balconyEmission, ref _balconyTween, state.IsRaining && state.IsOnBalcony);
            })
            .AddTo(this);
        }

        private void UpdateEmitterState(ParticleSystem ps, ParticleSystem.EmissionModule emission, ref Tween tween, bool shouldActive)
        {
            if (ps == null) return;

            tween?.Kill();

            if (shouldActive)
            {
                if (!ps.isPlaying) ps.Play();

                float currentRate = emission.rateOverTime.constant;
                tween = DOTween.To(() => currentRate, x =>
                {
                    currentRate = x;
                    emission.rateOverTime = x;
                }, _maxEmissionRate, _transitionDuration).SetEase(Ease.InOutQuad);
            }
            else
            {
                float currentRate = emission.rateOverTime.constant;
                tween = DOTween.To(() => currentRate, x =>
                {
                    currentRate = x;
                    emission.rateOverTime = x;
                }, 0f, _transitionDuration * 0.5f)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() => ps.Stop(true, ParticleSystemStopBehavior.StopEmitting));
            }
        }

        private void OnDestroy()
        {
            _farmTween?.Kill();
            _balconyTween?.Kill();
        }
    }
}
