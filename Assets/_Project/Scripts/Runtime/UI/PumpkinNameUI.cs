using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using VContainer;
using _Project.Scripts.Runtime.Features.Farm;
using _Project.Scripts.Runtime.Features.SaveSystem;

namespace _Project.Scripts.Runtime.UI.Farm
{
    public class PumpkinNamingUI : MonoBehaviour
    {
        [Header("Окно ввода имени")]
        [SerializeField] private GameObject _namingPanel;
        [SerializeField] private TMP_InputField _nameInputField;
        [SerializeField] private Button _confirmButton;

        [Header("Отображение на мониторе")]
        [SerializeField] private TMP_Text _pumpkinNameHeaderText;

        private Pumpkin _pumpkin;
        private GameSaveController _saveController;

        [Inject]
        public void Construct(Pumpkin pumpkin, GameSaveController saveController)
        {
            _pumpkin = pumpkin;
            _saveController = saveController;
        }

        private void Start()
        {
            if (_pumpkin == null) return;

            // Показываем окно ввода только если имя ещё не задано
            _pumpkin.IsNamed
                .ObserveOnMainThread()
                .Subscribe(isNamed =>
                {
                    if (_namingPanel != null)
                        _namingPanel.SetActive(!isNamed);
                })
                .AddTo(this);

            // Обновляем имя тыквы над её размером
            _pumpkin.PumpkinName
                .ObserveOnMainThread()
                .Subscribe(pumpkinName =>
                {
                    if (_pumpkinNameHeaderText != null)
                        _pumpkinNameHeaderText.text = pumpkinName;
                })
                .AddTo(this);

            // Подтверждение по кнопке
            if (_confirmButton != null)
            {
                _confirmButton.OnClickAsObservable()
                    .Subscribe(_ => ConfirmName())
                    .AddTo(this);
            }
        }

        private void ConfirmName()
        {
            string enteredName = _nameInputField != null ? _nameInputField.text : "Тыковка";
            _pumpkin.SetPumpkinName(enteredName);

            // Сразу записываем сейв на диск, чтобы при перезапуске окно больше не открывалось
            _saveController?.SaveGame();
        }
    }
}
