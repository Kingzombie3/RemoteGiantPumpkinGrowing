using UnityEngine;
using UnityEngine.UI;
using UniRx;
using _Project.Scripts.Runtime.Features.Cams;
using VContainer;
using _Project.Scripts.Runtime.Features.Upgrades;

namespace _Project.Scripts.Runtime.UI
{
    public class RoomNavigationUI : MonoBehaviour
    {
        [Header("Ссылка на камеру игрока")]
        [SerializeField] private PlayerCameraController _playerCameraController;

        [Header("Кнопки перемещения")]
        [SerializeField] private Button _toMonitor1Button;
        [SerializeField] private Button _toMonitor1FromBalconyButton;
        [SerializeField] private Button _toMonitor2Button;
        [SerializeField] private Button _toBalconyButton;

        private UpgradeManager _upgradeManager;

        public BoolReactiveProperty IsBalconyUnlocked { get; } = new BoolReactiveProperty(false);

        public readonly ReactiveProperty<bool> IsOnBalcony = new ReactiveProperty<bool>(false);

        [Inject]
        private void Construct(UpgradeManager upgrade)
        {
            _upgradeManager = upgrade;
        }

        void Start()
        {
            if (_playerCameraController == null)
            {
                Debug.LogError($"[{nameof(RoomNavigationUI)}] Не назначен PlayerCameraController!");
                return;
            }

            _toMonitor1Button.OnClickAsObservable()
                .Subscribe(_ => { 
                    _playerCameraController.MoveToView(0);

                    _toMonitor1Button.gameObject.SetActive(false);
                    _toMonitor2Button.gameObject.SetActive(true);

                    if (IsBalconyUnlocked.Value == true)
                    {
                        _toBalconyButton.gameObject.SetActive(true);
                    } 

                })
                .AddTo(this);

            _toMonitor2Button.OnClickAsObservable()
                .Subscribe(_ => { 
                    _playerCameraController.MoveToView(1);

                    _toMonitor2Button.gameObject.SetActive(false);
                    _toMonitor1Button.gameObject.SetActive(true);

                    if (IsBalconyUnlocked.Value == true)
                    {
                        _toBalconyButton.gameObject.SetActive(false);
                    }
                })
                .AddTo(this);

            _toBalconyButton.OnClickAsObservable()
                .Subscribe(_ => { 
                    _playerCameraController.MoveToView(2);

                    _toBalconyButton.gameObject.SetActive(false);                 
                    _toMonitor2Button.gameObject.SetActive(false);
                    _toMonitor1FromBalconyButton.gameObject.SetActive(true);
                    IsOnBalcony.Value = true;
                })
                .AddTo(this);

            _toMonitor1FromBalconyButton.OnClickAsObservable()
                .Subscribe(_ => { 
                    _playerCameraController.MoveToView(0);

                    _toMonitor1FromBalconyButton.gameObject.SetActive(false);
                    _toMonitor2Button.gameObject.SetActive(true);
                    IsOnBalcony.Value = false;

                    if (IsBalconyUnlocked.Value == true)
                    {
                        _toBalconyButton.gameObject.SetActive(true);
                    }

                })
                .AddTo(this);

            if (_upgradeManager != null)
            {
                _upgradeManager.UpgradeLevels
                    .ObserveEveryValueChanged(dict => _upgradeManager.GetUpgradeLevel("upgrade_telescope"))
                    .Subscribe(level =>
                    {
                        if (level > 0)
                        {
                            UnlockBalcony();
                        }
                    })
                    .AddTo(this);
            }
        }

        public void UnlockBalcony()
        {
            IsBalconyUnlocked.Value = true;
        }
    }
}
