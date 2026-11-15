using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using _Project.Scripts.Runtime.Core.SceneManagement;

namespace _Project.Scripts.Runtime.UI.Transitions
{
    public class SceneTransitionService : MonoBehaviour, ISceneTransitionService
    {
        [Header("2D Image для затемнения")]
        [SerializeField] private Image _faderImage;
        [SerializeField] private float _fadeDuration = 0.6f;

        private bool _isTransitioning;

        private void Awake()
        {
            if (_faderImage != null)
            {
                // Делаем картинку полностью прозрачной на старте
                Color col = _faderImage.color;
                col.a = 0f;
                _faderImage.color = col;
                _faderImage.raycastTarget = false;
            }
        }

        public async UniTask LoadSceneAsync(string sceneName)
        {
            if (_isTransitioning) return;
            _isTransitioning = true;

            if (_faderImage != null)
            {
                _faderImage.raycastTarget = true;
                await FadeToAlphaAsync(1f);
            }

            // Асинхронно грузим новую сцену
            var loadOperation = SceneManager.LoadSceneAsync(sceneName);
            if (loadOperation != null)
            {
                await loadOperation.ToUniTask();
            }

            if (_faderImage != null)
            {
                await FadeToAlphaAsync(0f);
                _faderImage.raycastTarget = false;
            }

            _isTransitioning = false;
        }

        private async UniTask FadeToAlphaAsync(float targetAlpha)
        {
            _faderImage.DOKill();
            var tween = _faderImage.DOFade(targetAlpha, _fadeDuration)
                .SetEase(Ease.InOutQuad)
                .SetUpdate(true); // Работает даже при Time.timeScale == 0

            await UniTask.WaitWhile(() => tween != null && tween.IsActive() && tween.IsPlaying());
        }

        private void OnDestroy()
        {
            _faderImage?.DOKill();
        }
    }
}
