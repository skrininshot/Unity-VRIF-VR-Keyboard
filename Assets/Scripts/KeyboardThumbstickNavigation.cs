using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class KeyboardThumbstickNavigation : MonoBehaviour
{
    [SerializeField] private Button _button;

    private void Reset()
    {
        _button = GetComponent<Button>();
    }

    public void InvokeClick() => _button.onClick.Invoke();
}