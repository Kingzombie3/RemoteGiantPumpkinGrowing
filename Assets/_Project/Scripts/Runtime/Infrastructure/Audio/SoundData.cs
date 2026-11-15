using UnityEngine;
using System;

namespace _Project.Scripts.Runtime.Infrastructure.Audio
{
    [Serializable]
    public struct SoundData
    {
        public string ID;
        public AudioClip[] Clips;
    }
}
