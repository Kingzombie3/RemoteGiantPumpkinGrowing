using _Project.Scripts.Runtime.Infrastructure.SaveLoadSystem;
using UnityEngine;
using VContainer;

namespace _Project.Scripts.Runtime.Infrastructure.SaveLoadSystem
{
    public class DebugProgressReset : MonoBehaviour
    {
        private GameSaveController _saveController;

        // VContainer сам прокинет сюда контроллер сохранения, как только сцена запустится
        [Inject]
        public void Construct(GameSaveController saveController)
        {
            _saveController = saveController;
        }

        [ContextMenu("?? СБРОСИТЬ ПРОГРЕСС ИГРЫ")]
        public void ClearGameProgressButton()
        {
            if (Application.isPlaying)
            {
                if (_saveController != null)
                {
                    _saveController.ClearSaveProgress();
                }
                else
                {
                    Debug.LogError("[Debug] GameSaveController еще не инициализирован!");
                }
                return;
            }

            // Отрезок кода для работы прямо в редакторе без запуска сцены
            string editorSavePath = System.IO.Path.Combine(Application.persistentDataPath, "pumpkin_save.dat");
            if (System.IO.File.Exists(editorSavePath))
            {
                System.IO.File.Delete(editorSavePath);
                Debug.Log("<color=#FF0000>[Editor]</color> Файл сохранения успешно удален с диска!");
            }
            else
            {
                Debug.Log("<color=#999999>[Editor]</color> Сейв на диске не найден.");
            }
        }
    }
}