using UnityEngine;
using UnityEngine.UI;
using TMPro; // Если используешь TextMeshPro, если обычный Text — замени на UnityEngine.UI.Text
using UniRx;
using DG.Tweening;
using _Project.Scripts.Runtime.Features.Farm;
using _Project.Scripts.Runtime.Core.Weather;
using _Project.Scripts.Runtime.Features.SaveSystem;

namespace _Project.Scripts.Runtime.UI
{
    public class FarmMonitorUI : MonoBehaviour
    {
        [Header("Компоненты")]
        [SerializeField] private Pumpkin _pumpkin;
        [SerializeField] private GrowController _growController;
        [SerializeField] private WeatherController _weatherController;

        [Header("Элементы отображения (UI)")]
        [SerializeField] private TMP_Text _sizeText;
        [SerializeField] private Slider _thirstSlider;
        [SerializeField] private Slider _weedSlider;
        [SerializeField] private TMP_Text _weatherText;

        [Header("Ввод и отображение имени")]
        [SerializeField] private GameObject _namingPanel;
        [SerializeField] private TMP_InputField _nameInputField;
        [SerializeField] private Button _confirmNameButton;
        [SerializeField] private TMP_Text _pumpkinNameText;

        private GameSaveController _saveController;
        private Vector3 _defaultThirstScale;
        private Vector3 _defaultWeedScale;

        [Header("Кнопки управления")]
        [SerializeField] private Button _waterButton;
        [SerializeField] private Button _weedButton;

        private void Start()
        {
            if (_pumpkin == null || _growController == null)
            {
                Debug.LogError("Не все ссылки назначены в FarmMonitorUI!");
                return;
            }

            if (_nameInputField != null)
            {
                _nameInputField.characterLimit = 20;
            }

            if (_thirstSlider != null) { _thirstSlider.minValue = 0f; _thirstSlider.maxValue = 1f; _defaultThirstScale = _thirstSlider.transform.localScale; }
            if (_weedSlider != null) { _weedSlider.minValue = 0f; _weedSlider.maxValue = 1f; _defaultWeedScale = _weedSlider.transform.localScale; }

            _pumpkin.PumpkinSize
                .Subscribe(size =>
                {
                    if (size >= 1000f)
                    {
                        _sizeText.text = "Размер: 1000 м - МАКСИМАЛЬНЫЙ!";
                    }
                    else
                    {
                        _sizeText.text = $"Размер: {size:F2} м";
                    }
                })
                .AddTo(this);


            _pumpkin.PumpkinThirsting
                .ObserveOnMainThread()
                .Subscribe(thirst =>
                {
                    _thirstSlider.DOKill();
                    _thirstSlider.DOValue(thirst, _pumpkin.CurrentTickInterval.Value)
                        .SetEase(Ease.Linear);
                })
                .AddTo(this);

            _pumpkin.PumpkinWeeding
                .ObserveOnMainThread()
                .Subscribe(weed =>
                {
                    _weedSlider.DOKill();
                    _weedSlider.DOValue(weed, _pumpkin.CurrentTickInterval.Value)
                        .SetEase(Ease.Linear);
                })
                .AddTo(this);

            _pumpkin.PumpkinThirsting
                .ObserveOnMainThread()
                .Subscribe(newThirst => AnimateSliderValue(_thirstSlider, newThirst))
                .AddTo(this);

            _pumpkin.PumpkinWeeding
                .ObserveOnMainThread()
                .Subscribe(newWeeding => AnimateSliderValue(_weedSlider, newWeeding))
                .AddTo(this);

            _waterButton.OnClickAsObservable()
                .Subscribe(_ => 
                { 
                    _growController.Watering(); 
                    PunchSlider(_thirstSlider.transform, _defaultThirstScale);
                })
                .AddTo(this);

            _weedButton.OnClickAsObservable()
                .Subscribe(_ => 
                { 
                    _growController.Weeding();
                    PunchSlider(_weedSlider.transform, _defaultWeedScale);
                }) 
                .AddTo(this);

            _weatherController.CurrentWeather
                            .ObserveOnMainThread()
                            .Subscribe(weather =>
                            {
                                switch (weather)
                                {
                                    case WeatherType.Normal:
                                        _weatherText.text = "Ясно";
                                        break;
                                    case WeatherType.Rain:
                                        _weatherText.text = "ДОЖДЬ";
                                        break;
                                    case WeatherType.Heat:
                                        _weatherText.text = "ЖАРА";
                                        break;
                                }
                            })
                            .AddTo(this);

            _pumpkin.IsNamed
                .ObserveOnMainThread()
                .Subscribe(isNamed =>
                {
                    if (_namingPanel != null)
                    {
                        _namingPanel.SetActive(!isNamed);

                        if (!isNamed && _nameInputField != null)
                        {
                            _nameInputField.ActivateInputField();
                            _nameInputField.Select();
                        }
                    }
                })
                .AddTo(this);


            _pumpkin.PumpkinName
                .ObserveOnMainThread()
                .Subscribe(pumpkinName =>
                {
                    if (_pumpkinNameText != null)
                        _pumpkinNameText.text = pumpkinName;
                })
                .AddTo(this);

            if (_confirmNameButton != null)
            {
                _confirmNameButton.OnClickAsObservable()
                    .Subscribe(_ => ConfirmPumpkinName())
                    .AddTo(this);
            }
        }

        private void AnimateSliderValue(Slider slider, float targetValue)
        {
            if (slider == null) return;

            slider.DOKill();

            bool isIncreasing = targetValue > slider.value;
            float duration = isIncreasing ? 0.25f : _pumpkin.CurrentTickInterval.Value;
            Ease ease = isIncreasing ? Ease.OutCubic : Ease.Linear;

            slider.DOValue(targetValue, duration).SetEase(ease);
        }

        private void PunchSlider(Transform sliderTransform, Vector3 defaultScale)
        {
            if (sliderTransform == null) return;

            sliderTransform.DOKill();
            sliderTransform.localScale = defaultScale;

            Vector3 punchAmount = defaultScale * 0.12f;
            punchAmount.z = 0f;

            sliderTransform.DOPunchScale(punchAmount, 0.25f, vibrato: 1, elasticity: 0.5f);
        }



        private void ConfirmPumpkinName()
        {
            if (_pumpkin == null) return;

            string enteredName = _nameInputField != null ? _nameInputField.text : "Тыковка";
            _pumpkin.SetPumpkinName(enteredName);

            _saveController?.SaveGame();
        }

    }
}
