using UnityEngine;

namespace _Project.Scripts.Runtime.Features.Cams
{
    [RequireComponent(typeof(Camera))]
    public class CameraToScreen : MonoBehaviour
    {
        [Header("Настройки экрана")]
        [SerializeField] private MeshRenderer screenRenderer;
        [SerializeField] private string texturePropertyName = "_MainTex"; // Для URP это "_BaseMap", для Built-in — "_MainTex"

        [Header("Разрешение текстуры")]
        [SerializeField] private int width = 1920;
        [SerializeField] private int height = 1080;
        [SerializeField] private int depth = 16;

        private RenderTexture renderTexture;
        private Camera targetCamera;

        void Start()
        {
            targetCamera = GetComponent<Camera>();

            if (screenRenderer == null)
            {
                Debug.LogError("Не назначен Screen Renderer на объекте " + gameObject.name);
                return;
            }

            // Создаем RenderTexture в рантайме
            renderTexture = new RenderTexture(width, height, depth, RenderTextureFormat.ARGB32);
            renderTexture.Create();

            // Направляем вывод камеры в текстуру
            targetCamera.targetTexture = renderTexture;

            // Передаем текстуру в материал монитора
            screenRenderer.material.SetTexture(texturePropertyName, renderTexture);
        }

        private void OnDestroy()
        {
            // Очищаем память при уничтожении объекта
            if (renderTexture != null)
            {
                if (targetCamera != null) targetCamera.targetTexture = null;
                renderTexture.Release();
                Destroy(renderTexture);
            }
        }
    }
}