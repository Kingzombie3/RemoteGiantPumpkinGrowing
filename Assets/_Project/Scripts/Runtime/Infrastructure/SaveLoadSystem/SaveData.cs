using System;
using System.Collections.Generic;

namespace _Project.Scripts.Runtime.Infrastructure.SaveLoadSystem
{
    [Serializable]
    public class SaveData
    {
        public int Coins;

        public float PumpkinSize;

        public List<UpgradeSaveEntry> Upgrades = new List<UpgradeSaveEntry>();

        public bool IsBalconyUnlocked;
    }
}