using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.Runtime.UI.Intro
{
    public class PaperSheetView : MonoBehaviour
    {
        [Header("Кадры внутри листа")]
        [SerializeField] private GameObject _frame1;
        [SerializeField] private GameObject _frame2;
        [SerializeField] private float _frameSwitchInterval = 0.4f;

        [Header("Настройки выезда на стол")]
        [SerializeField] private Vector2 _spawnOffset = new Vector2(0f, -800f);
        [SerializeField] private float _moveDuration = 0.6f;

        private Transform _transform;
        private Vector2 _targetedPosition;
        private CancellationTokenSource _frameLoopCts;

        private void Awake()
        {
            _transform = GetComponent<Transform>();
            // Запоминаем финальную позицию листа на столе, как ты расставил в редакторе
            _targetedPosition = _transform.position;
            gameObject.SetActive(false);
        }

        public async UniTask ShowAsync(CancellationToken skipToken)
        {
            gameObject.SetActive(true);

            _transform.position = _targetedPosition + _spawnOffset;
            StartFrameAnimation();

            var moveTween = _transform.DOMove(_targetedPosition, _moveDuration)
                .SetEase(Ease.InOutSine);

            // Ждем, пока твин активен и проигрывается, отслеживая токен отмены
            bool isCanceled = await UniTask.WaitWhile(() => moveTween != null && moveTween.IsActive() && moveTween.IsPlaying(), cancellationToken: skipToken)
                .SuppressCancellationThrow();

            // Если игрок нажал кнопку пропуска, мгновенно доводим лист в финальную точку
            if (isCanceled && moveTween != null && moveTween.IsActive())
            {
                moveTween.Complete();
            }
        }


        private void StartFrameAnimation()
        {
            _frameLoopCts?.Cancel();
            _frameLoopCts = new CancellationTokenSource();
            AnimateFramesLoopAsync(_frameLoopCts.Token).Forget();
        }

        private async UniTaskVoid AnimateFramesLoopAsync(CancellationToken token)
        {
            bool showFirst = true;

            while (!token.IsCancellationRequested)
            {
                if (_frame1 != null) _frame1.SetActive(showFirst);
                if (_frame2 != null) _frame2.SetActive(!showFirst);

                showFirst = !showFirst;
                await UniTask.Delay(TimeSpan.FromSeconds(_frameSwitchInterval), cancellationToken: token);
            }
        }

        private void OnDestroy()
        {
            _transform?.DOKill();
            _frameLoopCts?.Cancel();
            _frameLoopCts?.Dispose();
        }
    }
}
