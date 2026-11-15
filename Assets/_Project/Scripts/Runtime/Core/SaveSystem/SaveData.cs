using System;
using System.Collections.Generic;

namespace _Project.Scripts.Runtime.Core.SaveSystem
{
    [Serializable]
    public class SaveData
    {
        public int Coins;
        public float PumpkinSize;
        public string PumpkinName = "";
        public bool IsPumpkinNamed = false;
        public List<UpgradeSaveEntry> Upgrades = new List<UpgradeSaveEntry>();
    }
}