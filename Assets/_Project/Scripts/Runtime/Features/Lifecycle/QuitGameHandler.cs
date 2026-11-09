using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.Runtime.Features.SaveSystem;
using UnityEngine;

namespace _Project.Scripts.Runtime.Features.Lifecycle
{
    public class QuitGameHandler : MonoBehaviour
    {
        private GameSaveController _gameSaveController;

        private void Construct(GameSaveController gameController)
        {
            _gameSaveController = gameController;
        }
        private void OnApplicationQuit()
        {
            _gameSaveController.SaveGame();
        }
    }
}
