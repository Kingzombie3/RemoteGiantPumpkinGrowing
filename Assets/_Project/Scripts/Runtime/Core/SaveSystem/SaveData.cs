using System;
using System.Collections.Generic;

namespace _Project.Scripts.Runtime.Core.SaveSystem
{
    [Serializable]
    public class SaveData
    {
        public int Coins;

        public float PumpkinSize;

        public List<UpgradeSaveEntry> Upgrades = new List<UpgradeSaveEntry>();
    }
}