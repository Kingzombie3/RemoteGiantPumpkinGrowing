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

            if (_thirstSlider != null) { _thirstSlider.minValue = 0f; _thirstSlider.maxValue = 1f; }
            if (_weedSlider != null) { _weedSlider.minValue = 0f; _weedSlider.maxValue = 1f; }

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
                .Subscribe(thirst => _thirstSlider.DOValue(thirst,0.8f).SetEase(Ease.InSine))
                .AddTo(this);

            _pumpkin.PumpkinWeeding
                .Subscribe(weed => _weedSlider.DOValue(weed, 0.8f).SetEase(Ease.InSine))
                .AddTo(this);

            _waterButton.OnClickAsObservable()
                .Subscribe(_ => _growController.Watering())
                .AddTo(this);

            _weedButton.OnClickAsObservable()
                .Subscribe(_ => _growController.Weeding())
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

            // Показываем панель ввода, пока имя не задано

            _pumpkin.IsNamed
                .ObserveOnMainThread()
                .Subscribe(isNamed =>
                {
                    if (_namingPanel != null)
                    {
                        _namingPanel.SetActive(!isNamed);

                        // Принудительно ставим курсор в поле ввода при открытии панели
                        if (!isNamed && _nameInputField != null)
                        {
                            _nameInputField.ActivateInputField();
                            _nameInputField.Select();
                        }
                    }
                })
                .AddTo(this);


            // Обновляем заголовок с именем над размером тыквы
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

        private void ConfirmPumpkinName()
        {
            if (_pumpkin == null) return;

            string enteredName = _nameInputField != null ? _nameInputField.text : "Тыковка";
            _pumpkin.SetPumpkinName(enteredName);

            // Сразу сохраняем игру, чтобы выбор зафиксировался на диске
            _saveController?.SaveGame();
        }

    }
}
