namespace _Project.Scripts.Runtime.Core.Audio
{
    public interface IAudioService
    {
        void PlaySound(string id, float volume = 1f, float pitch = 1f);
        void PlayRandomSound(string id, float minVolume = 0.9f, float maxVolume = 1f, float minPitch = 0.92f, float maxPitch = 1.08f);
        void PlayRandomBackgroundMusic();
        void SetRainAmbience(bool isRainy);
    }
}
