using UnityEngine;
using UniRx;
using DG.Tweening;
using _Project.Scripts.Runtime.Feature.Farm; // Твоя папка с тыквой

namespace _Project.Scripts.Runtime.Features.Farm
{
    public class FarmCameraController : MonoBehaviour
    {
        [Header("Ссылки")]
        [SerializeField] private Pumpkin _pumpkin;
        [SerializeField] private RectTransform _monitorCanvas;

        [Header("Точки положения камеры (По возрастанию)")]
        [SerializeField] private Transform[] _cameraStages;

        [Header("Пороги изменения (в метрах)")]
        [SerializeField] private float[] _stageThresholds = { 10f, 100f, 1000f };

        [Header("Настройки UI на камере")]
        [SerializeField] private float _uiDistanceOffset = 0.5f;
        [Tooltip("Базовый скейл Canvas на первом этапе")]
        [SerializeField] private float _baseUiScale = 0.39f;

        private int _currentStageIndex = 0;

        void Start()
        {
            if (_pumpkin == null || _cameraStages == null || _cameraStages.Length == 0)
            {
                Debug.LogError($"[{nameof(FarmCameraController)}] Не настроены ссылки или точки камеры!");
                return;
            }

            ApplyStagePosition(_currentStageIndex, immediate: true);

            _pumpkin.PumpkinSize
                .Subscribe(size => CheckCameraStage(size))
                .AddTo(this);
        }

        private void CheckCameraStage(float currentSize)
        {
            int targetStage = 0;

            for (int i = 0; i < _stageThresholds.Length; i++)
            {
                if (currentSize >= _stageThresholds[i])
                {
                    targetStage = i + 1;
                }
                else
                {
                    break;
                }
            }

            targetStage = Mathf.Min(targetStage, _cameraStages.Length - 1);

            if (targetStage != _currentStageIndex)
            {
                _currentStageIndex = targetStage;
                ApplyStagePosition(_currentStageIndex, immediate: false);
                Debug.Log($"[FarmCamera] Этап изменен на: {_currentStageIndex}");
            }
        }

        private void ApplyStagePosition(int index, bool immediate)
        {
            Transform targetPoint = _cameraStages[index];

            if (immediate)
            {
                transform.DOKill();
                transform.position = targetPoint.position;
                transform.rotation = targetPoint.rotation;

                UpdateUILayout(targetPoint, index, 0f, immediate: true);
            }
            else
            {
                float duration = 1.5f; // Базовое время по умолчанию

                int zPos = Mathf.RoundToInt(targetPoint.position.z);
                switch (zPos)
                {
                    case -10:
                        duration = 1.5f;
                        break;
                    case -100:
                        duration = 5f;
                        break;
                    case -1000:
                        duration = 5f;
                        break;
                }

                transform.DOKill();

                transform.DOMove(targetPoint.position, duration).SetEase(Ease.InOutQuad);
                transform.DORotate(targetPoint.eulerAngles, duration).SetEase(Ease.InOutQuad);

                UpdateUILayout(targetPoint, index, duration, immediate: false);

                Debug.Log($"[FarmCamera] Запущен плавный отлет к точке: {targetPoint.name} за {duration} сек.");
            }
        }

        private void UpdateUILayout(Transform cameraAnchor, int stageIndex, float duration, bool immediate)
        {
            if (_monitorCanvas == null) return;

            _monitorCanvas.DOKill();

            float scaleMultiplier = 1f;
            switch (stageIndex)
            {
                case 0: scaleMultiplier = 1f; break;
                case 1: scaleMultiplier = 10f; break;
                case 2: scaleMultiplier = 100f; break;
            }
            Vector3 targetUiScale = Vector3.one * (_baseUiScale * scaleMultiplier);

            Vector3 targetUiPosition = new Vector3(
                _monitorCanvas.position.x,
                cameraAnchor.position.y - scaleMultiplier + 1,
                _monitorCanvas.position.z);
            Vector3 targetUiRotation = cameraAnchor.eulerAngles;

            if (immediate)
            {
                _monitorCanvas.position = targetUiPosition;
                _monitorCanvas.rotation = Quaternion.Euler(targetUiRotation);
                _monitorCanvas.localScale = targetUiScale;
            }
            else
            {
                _monitorCanvas.DOMove(targetUiPosition, duration).SetEase(Ease.InOutQuad);
                _monitorCanvas.DORotate(targetUiRotation, duration).SetEase(Ease.InOutQuad);
                _monitorCanvas.DOScale(targetUiScale, duration).SetEase(Ease.InOutQuad);
            }
        }



    }
}
