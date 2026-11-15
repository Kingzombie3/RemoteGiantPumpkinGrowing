using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Project.Scripts.Runtime.UI
{
    [RequireComponent(typeof(Collider))]
    public class ColliderToButtonProxy : MonoBehaviour
    {
        [Header("Целевая UI-кнопка на ферме")]
        [SerializeField] private Button _targetButton;

        private PointerEventData _pointerData;

        private void Awake()
        {
            // Создаем фейковые данные указателя для симуляции событий UI
            _pointerData = new PointerEventData(EventSystem.current);
        }

        private void OnMouseEnter()
        {
            if (!IsButtonReady()) return;

            // Симулируем наведение курсора (кнопка сменит цвет на Highlighted)
            ExecuteEvents.Execute(_targetButton.gameObject, _pointerData, ExecuteEvents.pointerEnterHandler);
        }

        private void OnMouseExit()
        {
            if (_targetButton == null) return;

            // Симулируем увод курсора (кнопка вернется в состояние Normal)
            ExecuteEvents.Execute(_targetButton.gameObject, _pointerData, ExecuteEvents.pointerExitHandler);
        }

        private void OnMouseDown()
        {
            if (!IsButtonReady()) return;

            // Симулируем зажатие кнопки мыши (кнопка перейдет в состояние Pressed)
            _pointerData.button = PointerEventData.InputButton.Left;
            ExecuteEvents.Execute(_targetButton.gameObject, _pointerData, ExecuteEvents.pointerDownHandler);
        }

        private void OnMouseUp()
        {
            if (_targetButton == null) return;

            // Симулируем отпускание кнопки мыши
            _pointerData.button = PointerEventData.InputButton.Left;
            ExecuteEvents.Execute(_targetButton.gameObject, _pointerData, ExecuteEvents.pointerUpHandler);
        }

        private void OnMouseUpAsButton()
        {
            if (!IsButtonReady()) return;

            // Срабатывает только если игрок нажал и отпустил мышь на этом же коллайдере!
            // Вызывает все подписки onClick (и твои UniRx onClickAsObservable)
            _pointerData.button = PointerEventData.InputButton.Left;
            ExecuteEvents.Execute(_targetButton.gameObject, _pointerData, ExecuteEvents.pointerClickHandler);
        }

        private bool IsButtonReady()
        {
            return _targetButton != null && _targetButton.gameObject.activeInHierarchy && _targetButton.interactable;
        }
    }
}
