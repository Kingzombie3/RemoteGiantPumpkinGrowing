using System;
using System.IO;
using System.Text;
using UnityEngine;
using _Project.Scripts.Runtime.Core.SaveSystem;

namespace _Project.Scripts.Runtime.Infrastructure.SaveSystem
{
    public class SaveService
    {
        private readonly string _savePath;

        public SaveService()
        {
            _savePath = Path.Combine(Application.persistentDataPath, "pumpkin_save.dat");
        }

        public void WriteToFile(SaveData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, false);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                XorEncrypt(bytes);
                File.WriteAllBytes(_savePath, bytes);
                Debug.Log("<color=#00FF00>[Infrastructure]</color> Бинарный файл сейва успешно обновлен.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Ошибка записи: {e.Message}");
            }
        }

        public SaveData ReadFromFile()
        {
            if (!File.Exists(_savePath)) return null;

            try
            {
                byte[] bytes = File.ReadAllBytes(_savePath);
                XorEncrypt(bytes);
                string json = Encoding.UTF8.GetString(bytes);
                return JsonUtility.FromJson<SaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Ошибка чтения: {e.Message}");
                return null;
            }
        }

        private void XorEncrypt(byte[] bytes)
        {
            byte key = 127;
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)(bytes[i] ^ key);
            }
        }

        public void DeleteSaveFile()
        {
            try
            {
                if (File.Exists(_savePath))
                {
                    File.Delete(_savePath);
                    Debug.Log("<color=#FF0000>[Infrastructure]</color> Бинарный файл сохранения успешно удален с диска.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveService] Не удалось удалить файл сохранения: {e.Message}");
            }
        }
    }
}
