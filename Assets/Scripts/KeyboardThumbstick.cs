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

        // �������� ����������� ��������
        Vector3 direction = GetMovementAxis();

        //��������� ��������������
        if (direction.magnitude < _deadZone) return;

        //���������, ��������� �� �������� �����
        if (Time.time < _timePressed + _inputDelay) return;

        // ���������� ��������� � ��������� �����������
        MovePointer(direction);       
    }

    /// <summary>
    /// ������������� ������� �� ��������� �������
    /// </summary>
    private void AcceptAction(InputAction.CallbackContext context)
    {
        if (_selectedKey != null)
            _selectedKey.InvokeClick();
    }

    private void MovePointer(Vector2 direction)
    {
        // ������� ��������� ������� � ����������� �������� �����
        KeyboardThumbstickNavigation nearestKey = GetNearestKey(direction);

        // ���������� ��������� � ��������� �������
        if (nearestKey != null)
        {
            _selectedKey = nearestKey;
            _pointer.transform.position = _selectedKey.transform.position;

            //������������� �������� �����
            _timePressed = Time.time;
        }
    }

    /// <summary>
    /// ����� ��������� ������� � ��������� �����������
    /// </summary>
    private KeyboardThumbstickNavigation GetNearestKey(Vector2 direction)
    {
        KeyboardThumbstickNavigation closestKey = null;
        float closestDistance = float.MaxValue;

        foreach (var key in _keys)
        {
            if (!key.gameObject.activeInHierarchy || key == _selectedKey) continue;

            // ��������� ������ �� ��������� � �������
            Vector3 directionToKey = key.transform.position - _pointer.transform.position;

            Vector2 keyDirection = directionToKey.normalized;

            // ��������� ���� ����� ������������ ������� � �������� � �������
            float angle = Vector2.Angle(direction, keyDirection);

            // ������� ����������
            float distance = directionToKey.magnitude;

            // ���� ������� ����� � � ���������� �����������
            if (angle < 45f)  // �������� 45 �������� � ��� ���������� ���� ��� ��������
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
    /// ������� ��� ���������� ����������� �����
    /// </summary>
    public virtual Vector2 GetMovementAxis()
    {
        // ���������� ��������� ��������, ������� �� ������ � ����� ������ ������� ������
        Vector3 lastAxisValue = Vector3.zero;

        // ��������� �������� �������������� ������� ������
        if (inputAxis != null)
        {
            for (int i = 0; i < inputAxis.Count; i++)
            {
                Vector3 axisVal = InputBridge.Instance.GetInputAxisValue(inputAxis[i]);

                // ������������� ��� ��������, ���� ��������� �������� ���� 0
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

            // ������������� ��� ��������, ���� ��������� �������� ���� 0
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
