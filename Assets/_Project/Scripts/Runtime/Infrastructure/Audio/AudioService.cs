using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using DG.Tweening;
using _Project.Scripts.Runtime.Core.Audio;

namespace _Project.Scripts.Runtime.Infrastructure.Audio
{
    public class AudioService : MonoBehaviour, IAudioService
    {
        [Header("База звуков")]
        [SerializeField] private List<SoundData> _sounds = new List<SoundData>();
        [SerializeField] private List<AudioClip> _backgroundMusic = new List<AudioClip>();

        [Header("Погода")]
        [SerializeField] private string _rainSoundID = "rain";

        private AudioSource _sfxSource;
        private AudioSource _musicSource;
        private AudioSource _rainSource;

        private int _lastMusicIndex = -1;

        private void Awake()
        {
            _sfxSource = gameObject.AddComponent<AudioSource>();

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = false;

            _rainSource = gameObject.AddComponent<AudioSource>();
            _rainSource.loop = true;
            _rainSource.volume = 0f;
        }

        private void Start()
        {
            // Запускаем первый трек
            PlayRandomBackgroundMusic();

            // Реактивный трекинг окончания музыки
            Observable.Interval(System.TimeSpan.FromSeconds(1f))
                .ObserveOnMainThread()
                .Where(_ => _musicSource != null && !_musicSource.isPlaying)
                .Subscribe(_ => PlayRandomBackgroundMusic())
                .AddTo(this);

            // Инициализируем зацикленный звук дождя на нулевой громкости
            var rainData = _sounds.FirstOrDefault(s => s.ID == _rainSoundID);
            if (rainData.Clips != null && rainData.Clips.Length > 0)
            {
                _rainSource.clip = rainData.Clips[0];
                _rainSource.Play();
            }
        }

        public void PlaySound(string id, float volume = 1f, float pitch = 1f)
        {
            var sound = _sounds.FirstOrDefault(s => s.ID == id);
            if (sound.Clips == null || sound.Clips.Length == 0)
            {
                Debug.LogWarning($"[Audio] Звук с ID '{id}' не найден!");
                return;
            }

            PlayClipInternal(sound.Clips[0], volume, pitch);
        }

        public void PlayRandomSound(string id, float minVolume = 0.9f, float maxVolume = 1f, float minPitch = 0.92f, float maxPitch = 1.08f)
        {
            var sound = _sounds.FirstOrDefault(s => s.ID == id);
            if (sound.Clips == null || sound.Clips.Length == 0)
            {
                Debug.LogWarning($"[Audio] Звук с ID '{id}' не найден!");
                return;
            }

            AudioClip randomClip = sound.Clips[Random.Range(0, sound.Clips.Length)];
            float randomVolume = Random.Range(minVolume, maxVolume);
            float randomPitch = Random.Range(minPitch, maxPitch);

            PlayClipInternal(randomClip, randomVolume, randomPitch);
        }

        public void PlayRandomBackgroundMusic()
        {
            if (_backgroundMusic.Count == 0) return;

            int randomIndex = _lastMusicIndex;

            if (_backgroundMusic.Count > 1)
            {
                while (randomIndex == _lastMusicIndex)
                {
                    randomIndex = Random.Range(0, _backgroundMusic.Count);
                }
            }
            else
            {
                randomIndex = 0;
            }

            _lastMusicIndex = randomIndex;
            _musicSource.clip = _backgroundMusic[randomIndex];
            _musicSource.volume = 0.35f;
            _musicSource.Play();

            Debug.Log($"[Audio-Music] Заиграл следующий трек: {_musicSource.clip.name}");
        }

        public void SetRainAmbience(bool isRainy)
        {
            if (_rainSource == null) return;

            float targetVolume = isRainy ? 0.5f : 0f;
            _rainSource.DOKill();
            _rainSource.DOFade(targetVolume, 2.0f).SetEase(Ease.InOutQuad);
        }

        private void PlayClipInternal(AudioClip clip, float volume, float pitch)
        {
            if (clip == null) return;
            _sfxSource.pitch = pitch;
            _sfxSource.PlayOneShot(clip, volume);
        }

        private void OnDestroy()
        {
            _rainSource?.DOKill();
        }
    }
}
