using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace _Project.Scripts.Runtime.UI
{
    public class MonitorRaycastForwarder : MonoBehaviour
    {
        [Header("Камеры")]
        [SerializeField] private Camera _playerCamera; // Камера игрока от первого лица
        [SerializeField] private Camera _farmCamera;   // Твоя FarmCamera (с которой висит CameraToScreen)

        [Header("Компоненты UI")]
        [SerializeField] private MeshCollider _screenCollider;        // Коллайдер 3D-экрана монитора
        [SerializeField] private GraphicRaycaster _farmCanvasRaycaster; // Raycaster канваса с кнопками

        private PointerEventData _pointerData;
        private readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();

        private void Awake()
        {
            if (_screenCollider == null)
                _screenCollider = GetComponent<MeshCollider>();

            _pointerData = new PointerEventData(EventSystem.current);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                ForwardClickToFarmCanvas();
            }
        }

        private void ForwardClickToFarmCanvas()
        {
            if (_playerCamera == null || _farmCamera == null || _screenCollider == null || _farmCanvasRaycaster == null)
                return;

            // Пускаем луч от курсора игрока в 3D-экран монитора
            Ray playerRay = _playerCamera.ScreenPointToRay(Input.mousePosition);
            if (_screenCollider.Raycast(playerRay, out RaycastHit hit, 10f))
            {
                // Переводим UV-координаты точки удара по экрану в пиксели FarmCamera
                Vector2 uv = hit.textureCoord;
                Vector3 farmScreenPoint = new Vector3(
                    uv.x * _farmCamera.pixelWidth,
                    uv.y * _farmCamera.pixelHeight,
                    0f
                );

                // Проверяем попадание в интерфейс на стороне фермы
                _pointerData.position = farmScreenPoint;
                _raycastResults.Clear();
                _farmCanvasRaycaster.Raycast(_pointerData, _raycastResults);

                for (int i = 0; i < _raycastResults.Count; i++)
                {
                    GameObject hitObject = _raycastResults[i].gameObject;

                    // 1. Проверяем, не кликнули ли в поле ввода имени тыквы
                    TMP_InputField inputField = hitObject.GetComponentInParent<TMP_InputField>();
                    if (inputField != null && inputField.interactable)
                    {
                        inputField.ActivateInputField();
                        inputField.Select();
                        break;
                    }

                    // 2. Проверяем, не кликнули ли в кнопку (полив, прополка, подтверждение)
                    Button btn = hitObject.GetComponentInParent<Button>();
                    if (btn != null && btn.interactable)
                    {
                        ExecuteEvents.Execute(btn.gameObject, _pointerData, ExecuteEvents.pointerClickHandler);
                        break;
                    }
                }
            }
        }
    }
}
