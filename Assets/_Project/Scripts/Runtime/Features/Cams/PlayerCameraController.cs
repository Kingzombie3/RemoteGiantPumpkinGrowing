using UnityEngine;
using DG.Tweening;

namespace _Project.Scripts.Runtime.Features.Cams
{
    public class PlayerCameraController : MonoBehaviour
    {
        [Header("Точки ракурсов игрока")]
        [SerializeField] private Transform[] _playerViews;

        [Header("Настройки полета")]
        [SerializeField] private float _moveDuration = 1.2f;
        [SerializeField] private Ease _moveEase = Ease.InOutQuad;

        private int _currentViewIndex = -1;

        void Start()
        {
            if (_playerViews == null || _playerViews.Length == 0)
            {
                Debug.LogError($"[{nameof(PlayerCameraController)}] Не назначены точки ракурсов в массиве Player Views!");
                return;
            }
            
            SwitchView(0, immediate: true);
        }

        public void MoveToView(int viewIndex)
        {
            if (viewIndex < 0 || viewIndex >= _playerViews.Length)
            {
                Debug.LogWarning($"[{nameof(PlayerCameraController)}] Индекс ракурса {viewIndex} вне диапазона!");
                return;
            }

            if (viewIndex == _currentViewIndex) return;

            SwitchView(viewIndex, immediate: false);
        }

        private void SwitchView(int index, bool immediate)
        {
            _currentViewIndex = index;
            Transform targetView = _playerViews[index];

            transform.DOKill();

            if (immediate)
            {
                transform.position = targetView.position;
                transform.rotation = targetView.rotation;
            }
            else
            {
                transform.DOMove(targetView.position, _moveDuration).SetEase(_moveEase);
                transform.DORotate(targetView.eulerAngles, _moveDuration).SetEase(_moveEase);
            }
        }
    }
}

