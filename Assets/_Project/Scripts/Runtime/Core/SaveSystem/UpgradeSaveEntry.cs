using System;
using System.Collections.Generic;

namespace _Project.Scripts.Runtime.Core.SaveSystem
{
    [Serializable]
    public struct UpgradeSaveEntry
    {
        public string UpgradeID;
        public int Level;

        public UpgradeSaveEntry(string id, int level)
        {
            UpgradeID = id;
            Level = level;
        }
    }

}
