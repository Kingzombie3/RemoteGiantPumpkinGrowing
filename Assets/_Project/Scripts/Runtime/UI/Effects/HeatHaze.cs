using UnityEngine;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;
using _Project.Scripts.Runtime.Core.Weather;
using _Project.Scripts.Runtime.Features.Farm;
using VContainer;

namespace _Project.Scripts.Runtime.UI.Effects
{
    public class HeatHazeController : MonoBehaviour
    {
        [Header("Ссылки")]        
        [SerializeField] private Image _hazeImage;

        [Header("Настройки перехода")]
        [SerializeField] private float _transitionDuration = 2.5f;

        private Material _runtimeMaterial;
        private static readonly int IntensityID = Shader.PropertyToID("_Intensity");

        private WeatherController _weatherController;

        [Inject]
        private void Construct(WeatherController weatherController)
        {
            _weatherController = weatherController;
        }

        private void Start()
        {
            if (_hazeImage == null || _weatherController == null) return;

            _runtimeMaterial = Instantiate(_hazeImage.material);
            _hazeImage.material = _runtimeMaterial;

            // Гарантируем, что на старте эффект полностью выключен
            _runtimeMaterial.SetFloat(IntensityID, 0f);
            _hazeImage.enabled = false;

            _weatherController.CurrentWeather
                .ObserveOnMainThread()
                .Subscribe(weather => SetHeatState(weather == WeatherType.Heat))
                .AddTo(this);
        }

        private void SetHeatState(bool isHot)
        {
            if (_runtimeMaterial == null || _hazeImage == null) return;

            _runtimeMaterial.DOKill();

            if (isHot)
            {
                _hazeImage.enabled = true;
                _runtimeMaterial.DOFloat(1f, IntensityID, _transitionDuration)
                    .SetEase(Ease.InOutSine);
            }
            else
            {
                _runtimeMaterial.DOFloat(0f, IntensityID, _transitionDuration)
                    .SetEase(Ease.InOutSine)
                    .OnComplete(() => _hazeImage.enabled = false);
            }
        }

        private void OnDestroy()
        {
            if (_runtimeMaterial != null)
            {
                _runtimeMaterial.DOKill();
                Destroy(_runtimeMaterial);
            }
        }
    }
}
