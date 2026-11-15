using DG.Tweening;
using UnityEngine;
using VContainer;
using UniRx;

namespace _Project.Scripts.Runtime.Features.Farm
{
    public class Grass : MonoBehaviour
    {
        private Pumpkin _pumpkin;

        private float _animDuration = 0.3f;
        private Vector3 _maxScale = Vector3.one;

        [SerializeField] private bool _isTemp;

        // ѕорог зарастани€ (0.3 = 30%), до которого трава не по€вл€етс€
        private const float GrowthStartThreshold = 0.2f;

        [Inject]
        private void Construct(Pumpkin pumpkin)
        {
            _pumpkin = pumpkin;
        }

        private void Start()
        {
            if (_pumpkin == null)
            {
                Debug.LogError("Pumpkin no assigned");
                return;
            }

            _pumpkin.PumpkinWeeding
                .Subscribe(weedingValue => UpdateWeedScale(weedingValue))
                .AddTo(this);
        }

        private void UpdateWeedScale(float weedingValue)
        {
            // ѕереводим чистоту (1..0) в степень зарастани€ (0..1)
            float rawGrowth = 1f - weedingValue;

            // ѕока зарастание меньше 0.3 (чистота > 0.7), InverseLerp вернет 0.
            // ƒалее плавно раст€гивает отрезок [0.3 .. 1.0] в [0 .. 1].
            float growthFactor = Mathf.InverseLerp(GrowthStartThreshold, 1f, rawGrowth);

            Vector3 targetScale = _maxScale * growthFactor;

            transform.DOKill();
            transform.DOScale(targetScale, _animDuration).SetEase(Ease.Linear);
            
            if (_isTemp && _pumpkin.PumpkinSize.Value > 20)
            {
                if (transform.localScale.x < 0.01f)
                {
                    Destroy(this.gameObject);
                }
            }
        }

        private void OnDestroy()
        {
            transform.DOKill();
        }
    }
}
