using BNG;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(UICanvasGroup))]
public class KeyboardThumbstick : MonoBehaviour
{
    [SerializeField] private List<InputAxis> inputAxis = new () { InputAxis.RightThumbStickAxis };

    [SerializeField] private InputActionReference _selectAction;
    [SerializeField] private InputActionReference _acceptSelectionAction;
    [SerializeField] private GameObject _pointer;
    [SerializeField] private float _inputDelay = 0.2f;
    [SerializeField] private float _deadZone = 0.01f;

    private UICanvasGroup _canvasGroup;
    private GameObject _activeCanvas => _canvasGroup.ActiveCanvas;

    private KeyboardThumbstickNavigation _selectedKey;

    private float _timePressed = 0;

    private List<KeyboardThumbstickNavigation> _keys;

    private void Awake()
    {
        _canvasGroup = GetComponent<UICanvasGroup>();
        _keys = transform.GetComponentsInChildren<KeyboardThumbstickNavigation>(true).ToList();
    }

    private void OnEnable()
    {
        _pointer.SetActive(true);
        _acceptSelectionAction.action.performed += AcceptAction;
    }

    private void OnDisable()
    {
        _pointer.SetActive(false);
        _acceptSelectionAction.action.performed -= AcceptAction;
    }

    private void Update()
    {
        if (_canvasGroup == null || _activeCanvas == null) return;

        // Получаем направление движения
        Vector3 direction = GetMovementAxis();

        //Исключаем микроколебания
        if (direction.magnitude < _deadZone) return;

        //Проверяем, действует ли задержка ввода
        if (Time.time < _timePressed + _inputDelay) return;

        // Перемещаем указатель в указанном направлении
        MovePointer(direction);       
    }

    /// <summary>
    /// Подтверждение нажатия на выбранную клавишу
    /// </summary>
    private void AcceptAction(InputAction.CallbackContext context)
    {
        if (_selectedKey != null)
            _selectedKey.InvokeClick();
    }

    private void MovePointer(Vector2 direction)
    {
        // Находим ближайшую клавишу в направлении движения стика
        KeyboardThumbstickNavigation nearestKey = GetNearestKey(direction);

        // Перемещаем указатель к выбранной клавише
        if (nearestKey != null)
        {
            _selectedKey = nearestKey;
            _pointer.transform.position = _selectedKey.transform.position;

            //Устанавливаем задержку ввода
            _timePressed = Time.time;
        }
    }

    /// <summary>
    /// Поиск ближайшей клавиши в указанном направлении
    /// </summary>
    private KeyboardThumbstickNavigation GetNearestKey(Vector2 direction)
    {
        KeyboardThumbstickNavigation closestKey = null;
        float closestDistance = float.MaxValue;

        foreach (var key in _keys)
        {
            if (!key.gameObject.activeInHierarchy || key == _selectedKey) continue;

            // Вычисляем вектор от указателя к клавише
            Vector3 directionToKey = key.transform.position - _pointer.transform.position;

            Vector2 keyDirection = directionToKey.normalized;

            // Вычисляем угол между направлением стикера и вектором к клавише
            float angle = Vector2.Angle(direction, keyDirection);

            // Считаем расстояние
            float distance = directionToKey.magnitude;

            // Если клавиша ближе и в правильном направлении
            if (angle < 45f)  // Примерно 45 градусов – это допустимый угол для движения
            {
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestKey = key;
                }
            }
        }

        return closestKey;
    }

    /// <summary>
    /// Функция для вычисления направления стика
    /// </summary>
    public virtual Vector2 GetMovementAxis()
    {
        // Наибольшее ненулевое значение, которое мы найдем в нашем списке входных данных
        Vector3 lastAxisValue = Vector3.zero;

        // Проверяем привязки необработанных входных данных
        if (inputAxis != null)
        {
            for (int i = 0; i < inputAxis.Count; i++)
            {
                Vector3 axisVal = InputBridge.Instance.GetInputAxisValue(inputAxis[i]);

                // Устанавливаем это значение, если последнее значение было 0
                if (lastAxisValue == Vector3.zero)
                {
                    lastAxisValue = axisVal;
                }
                else if (axisVal != Vector3.zero && axisVal.magnitude > lastAxisValue.magnitude)
                {
                    lastAxisValue = axisVal;
                }
            }
        }
        if (_selectAction != null)
        {
            Vector3 axisVal = _selectAction.action.ReadValue<Vector2>();

            // Устанавливаем это значение, если последнее значение было 0
            if (lastAxisValue == Vector3.zero)
            {
                lastAxisValue = axisVal;
            }
            else if (axisVal != Vector3.zero && axisVal.magnitude > lastAxisValue.magnitude)
            {
                lastAxisValue = axisVal;
            }
        }

        return lastAxisValue;
    }
}
