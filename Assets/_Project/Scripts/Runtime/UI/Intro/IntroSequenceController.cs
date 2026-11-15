using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer;
using _Project.Scripts.Runtime.Core.Audio;
using _Project.Scripts.Runtime.UI.Transitions;
using _Project.Scripts.Runtime.Core.SceneManagement;

namespace _Project.Scripts.Runtime.UI.Intro
{
    public class IntroSequenceController : MonoBehaviour
    {
        [Header("Очередь листов на столе")]
        [SerializeField] private PaperSheetView[] _sheets;
        [SerializeField] private float _delayBetweenSheets = 2.0f;

        [Header("Переход в игру")]
        [SerializeField] private string _gameSceneName = "Game";
        [SerializeField] private string _paperSoundID = "paper_rustle";
        [SerializeField] private GameObject _clickToContinueHint;

        private IAudioService _audioService;
        private ISceneTransitionService _transitionService;

        private CancellationTokenSource _lifecycleCts;
        private CancellationTokenSource _currentStepSkipCts;

        [Inject]
        public void Construct(IAudioService audioService, ISceneTransitionService transitionService)
        {
            _audioService = audioService;
            _transitionService = transitionService;
        }

        private void Start()
        {
            if (_clickToContinueHint != null)
                _clickToContinueHint.SetActive(false);

            _lifecycleCts = new CancellationTokenSource();
            RunIntroSequenceAsync(_lifecycleCts.Token).Forget();
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                _currentStepSkipCts?.Cancel();
            }
        }

        private async UniTaskVoid RunIntroSequenceAsync(CancellationToken mainToken)
        {
            for (int i = 0; i < _sheets.Length; i++)
            {
                if (mainToken.IsCancellationRequested) return;

                var sheet = _sheets[i];
                if (sheet == null) continue;

                _currentStepSkipCts?.Dispose();
                _currentStepSkipCts = CancellationTokenSource.CreateLinkedTokenSource(mainToken);

                _audioService?.PlayRandomSound(_paperSoundID, 0.8f, 1.0f, 0.9f, 1.1f);
                await sheet.ShowAsync(_currentStepSkipCts.Token);

                await UniTask.Delay(TimeSpan.FromSeconds(_delayBetweenSheets), cancellationToken: _currentStepSkipCts.Token)
                    .SuppressCancellationThrow();
            }

            if (_clickToContinueHint != null)
                _clickToContinueHint.SetActive(true);

            await UniTask.WaitUntil(() => Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space), cancellationToken: mainToken);

            if (_transitionService != null)
            {
                _transitionService.LoadSceneAsync(_gameSceneName).Forget();
            }
        }

        private void OnDestroy()
        {
            _currentStepSkipCts?.Cancel();
            _currentStepSkipCts?.Dispose();
            _lifecycleCts?.Cancel();
            _lifecycleCts?.Dispose();
        }
    }
}
