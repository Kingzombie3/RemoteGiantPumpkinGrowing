using UnityEngine;
using UniRx;
using System;
using Random = UnityEngine.Random;
using _Project.Scripts.Runtime.Core.Weather;
using _Project.Scripts.Runtime.Core.Audio;
using VContainer;

namespace _Project.Scripts.Runtime.Features.Farm
{
    public class WeatherController : MonoBehaviour
    {
        [Header("Ссылки")]
        [SerializeField] private Pumpkin _pumpkin;

        [Header("Настройки времени (в секундах)")]
        [SerializeField] private float _minWeatherDuration = 90f; //120
        [SerializeField] private float _maxWeatherDuration = 240f; //240

        private int badWeatherCount;

        public ReactiveProperty<WeatherType> CurrentWeather = new ReactiveProperty<WeatherType>(WeatherType.Normal);

        private IAudioService _audioService;

        [Inject]
        public void Construct(IAudioService audioService)
        {
            _audioService = audioService;
        }

        void Start()
        {
            badWeatherCount = 0;

            if (_pumpkin == null)
            {
                Debug.LogError($"[{nameof(WeatherController)}] Не назначена ссылка на Pumpkin!");
                return;
            }

            CurrentWeather
                .ObserveOnMainThread()
                .Subscribe(weather => _audioService?.SetRainAmbience(weather == WeatherType.Rain))
                .AddTo(this);

            _pumpkin.IsNamed
                .Where(isNamed => isNamed)
                .First()
                .ObserveOnMainThread()
                .Subscribe(_ => StartWeatherCycle())
                .AddTo(this);
        }

        private void StartWeatherCycle()
        {
            float duration = Random.Range(_minWeatherDuration, _maxWeatherDuration);

            Observable.Timer(TimeSpan.FromSeconds(duration))
                .ObserveOnMainThread()
                .Subscribe(_ =>
                {
                    ChangeToRandomWeather();
                    StartWeatherCycle();
                })
                .AddTo(this);
        }

        private void ChangeToRandomWeather()
        {
            Array values = Enum.GetValues(typeof(WeatherType));

            WeatherType randomWeather = (WeatherType)values.GetValue(Random.Range(0, values.Length));

            Debug.Log($"Weather: {randomWeather}");

            if (randomWeather == WeatherType.Rain 
                || randomWeather == WeatherType.Heat)
            {
                badWeatherCount++;

                if (badWeatherCount > 2)
                {
                    badWeatherCount = 0;
                    randomWeather = WeatherType.Normal;
                }

                Debug.Log($"Bad weather count in a row: {badWeatherCount}");
            }
            else
            {
                badWeatherCount = 0;
                Debug.Log($"Bad weather count nulled");
            }

            CurrentWeather.Value = randomWeather;
            _pumpkin.ChangeBehaviour(randomWeather);
        }
    }
}
