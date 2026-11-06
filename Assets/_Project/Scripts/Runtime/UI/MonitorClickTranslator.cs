using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class MonitorClickTranslator : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private Camera _playerCamera; 
    [SerializeField] private Canvas _targetCanvas;  

    private GraphicRaycaster _canvasRaycaster;
    private PointerEventData _pointerEventData;
    private EventSystem _eventSystem;
    private RectTransform _canvasRect;

    void Start()
    {
        if (_playerCamera == null) _playerCamera = Camera.main;
        
        if (_targetCanvas != null)
        {
            _canvasRaycaster = _targetCanvas.GetComponent<GraphicRaycaster>();
            _canvasRect = _targetCanvas.GetComponent<RectTransform>();
        }
        
        _eventSystem = EventSystem.current;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryClickThroughMonitor();
        }
    }

    private void TryClickThroughMonitor()
    {
        if (_canvasRaycaster == null || _eventSystem == null || _canvasRect == null) return;

        Ray ray = _playerCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject != gameObject) return;

            Vector2 texCoord = hit.textureCoord;

            Vector2 localPoint = new Vector2(
                (texCoord.x - _canvasRect.pivot.x) * _canvasRect.rect.width,
                (texCoord.y - _canvasRect.pivot.y) * _canvasRect.rect.height
            );

            _pointerEventData = new PointerEventData(_eventSystem);
            
            if (_targetCanvas.worldCamera != null)
            {
                Vector3 worldPos = _canvasRect.TransformPoint(localPoint);
                _pointerEventData.position = _targetCanvas.worldCamera.WorldToScreenPoint(worldPos);
            }
            else
            {
                _pointerEventData.position = localPoint;
            }

            List<RaycastResult> results = new List<RaycastResult>();
            _canvasRaycaster.Raycast(_pointerEventData, results);

            foreach (RaycastResult result in results)
            {
                Button button = result.gameObject.GetComponentInParent<Button>();
                if (button != null && button.interactable)
                {
                    button.onClick.Invoke();
                    Debug.Log($"[Транслятор] Клик попал точно в кнопку: {result.gameObject.name}");
                    break;
                }
            }
        }
    }
}
