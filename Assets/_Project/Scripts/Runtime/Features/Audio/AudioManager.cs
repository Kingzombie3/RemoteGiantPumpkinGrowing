using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers; // Нужен для UpdateAsObservable
using DG.Tweening;
using _Project.Scripts.Runtime.Features.Farm;
using _Project.Scripts.Runtime.Core;

namespace _Project.Scripts.Runtime.Features.Audio
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager Instance;

        [Header("База звуков")]
        [SerializeField] private List<SoundData> _sounds = new List<SoundData>();
        [SerializeField] private List<AudioClip> _backgroundMusic = new List<AudioClip>();

        [Header("Погода")]
        [SerializeField] private WeatherController _weatherController;
        [SerializeField] private string _rainSoundID = "rain";

        private AudioSource _sfxSource;
        private AudioSource _musicSource;
        private AudioSource _rainSource;

        private int _lastMusicIndex = -1; // Чтобы один и тот же трек не играл дважды подряд

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _sfxSource = gameObject.AddComponent<AudioSource>();

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = false; // Отключаем жесткий Loop, чтобы треки могли меняться!

            _rainSource = gameObject.AddComponent<AudioSource>();
            _rainSource.loop = true;
            _rainSource.volume = 0f;
        }

        private void Start()
        {
            // Запускаем первый трек
            PlayRandomBackgroundMusic();

            // РЕАКТИВНЫЙ ТРЕКИНГ МУЗЫКИ: Каждую секунду проверяем, не доиграла ли музыка
            Observable.Interval(System.TimeSpan.FromSeconds(1f))
                .ObserveOnMainThread()
                .Where(_ => _musicSource != null && !_musicSource.isPlaying)
                .Subscribe(_ => PlayRandomBackgroundMusic())
                .AddTo(this);

            // Реактивно следим за дождем через UniRx
            if (_weatherController != null)
            {
                var rainData = _sounds.FirstOrDefault(s => s.ID == _rainSoundID);
                if (rainData.Clips != null && rainData.Clips.Length > 0)
                {
                    _rainSource.clip = rainData.Clips[0];
                    _rainSource.Play();
                }

                _weatherController.CurrentWeather
                    .ObserveOnMainThread()
                    .Subscribe(weather => HandleRainAudio(weather == WeatherType.Rain))
                    .AddTo(this);
            }
        }

        // --- СТАТИЧЕСКИЕ МЕТОДЫ ---

        /// <summary>
        /// Воспроизводит базовый звук по ID (первый клип в списке).
        /// </summary>
        public static void PlaySound(string id, float volume = 1f, float pitch = 1f)
        {
            if (Instance == null) return;

            var sound = Instance._sounds.FirstOrDefault(s => s.ID == id);
            if (sound.Clips == null || sound.Clips.Length == 0) return;

            InstancePlaySFX(sound.Clips[0], volume, pitch);
        }

        /// <summary>
        /// Выбирает случайный клип из заложенных в ID и проигрывает его со случайным питчем и громкостью в заданных диапазонах.
        /// </summary>
        /// <param name="id">Строковый ID из инспектора</param>
        /// <param name="minVolume">Минимальный порог громкости</param>
        /// <param name="maxVolume">Максимальный порог громкости</param>
        /// <param name="minPitch">Минимальный порог питча (например, 0.9f)</param>
        /// <param name="maxPitch">Максимальный порог питча (например, 1.1f)</param>
        public static void PlayRandomSound(string id, float minVolume = 0.9f, float maxVolume = 1f, float minPitch = 0.92f, float maxPitch = 1.08f)
        {
            if (Instance == null) return;

            var sound = Instance._sounds.FirstOrDefault(s => s.ID == id);
            if (sound.Clips == null || sound.Clips.Length == 0)
            {
                Debug.LogWarning($"[Audio] Звук с ID '{id}' не найден!");
                return;
            }

            // Рандомизируем сам клип из массива
            AudioClip randomClip = sound.Clips[Random.Range(0, sound.Clips.Length)];

            // Рандомизируем параметры по переданным диапазонам
            float randomVolume = Random.Range(minVolume, maxVolume);
            float randomPitch = Random.Range(minPitch, maxPitch);

            InstancePlaySFX(randomClip, randomVolume, randomPitch);
        }


        public static void PlayRandomBackgroundMusic()
        {
            if (Instance == null || Instance._backgroundMusic.Count == 0) return;

            int randomIndex = Instance._lastMusicIndex;

            // Если треков больше одного, ищем индекс, не равный предыдущему
            if (Instance._backgroundMusic.Count > 1)
            {
                while (randomIndex == Instance._lastMusicIndex)
                {
                    randomIndex = Random.Range(0, Instance._backgroundMusic.Count);
                }
            }
            else
            {
                randomIndex = 0;
            }

            Instance._lastMusicIndex = randomIndex;
            Instance._musicSource.clip = Instance._backgroundMusic[randomIndex];
            Instance._musicSource.volume = 0.35f; // Комфортная громкость для фона
            Instance._musicSource.Play();

            Debug.Log($"[Audio-Music] Заиграл следующий трек: {Instance._musicSource.clip.name}");
        }

        // --- ВНУТРЕННЯЯ ЛОГИКА ---

        private static void InstancePlaySFX(AudioClip clip, float volume, float pitch)
        {
            if (clip == null) return;
            Instance._sfxSource.pitch = pitch;
            Instance._sfxSource.PlayOneShot(clip, volume);
        }

        private void HandleRainAudio(bool isRainy)
        {
            float targetVolume = isRainy ? 0.5f : 0f;
            _rainSource.DOKill();
            _rainSource.DOFade(targetVolume, 2.0f).SetEase(Ease.InOutQuad);
        }
    }
}
