using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.EventSystems;

public class MenuNavigation : MonoBehaviour
{
    public GameObject menuPanel;
    public Button[] menuButtons;      // Assign all your menu buttons here (in order)

    private int selectedIndex = 0;
    //private bool isMenuVisible = false;

    void OnEnable()
    {
        if (menuButtons.Length > 0)
        {
            selectedIndex = 0;
            HighlightButton(selectedIndex);
        }
    }

    void Update()
    {
        if (!menuPanel.activeSelf) return;

        // -------------------------
        // Arrow Key Navigation
        // -------------------------
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedIndex = (selectedIndex + 1) % menuButtons.Length;
            HighlightButton(selectedIndex);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedIndex = (selectedIndex - 1 + menuButtons.Length) % menuButtons.Length;
            HighlightButton(selectedIndex);
        }

        // -------------------------
        // Press Enter Key
        // -------------------------
        if (Input.GetKeyDown(KeyCode.Return))
        {
            PressSelectedButton();
        }

        // -------------------------
        // VR Controller Button (A button on Meta Quest 2)
        // -------------------------
        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out bool pressed) && pressed)
        {
            PressSelectedButton();
        }
    }

    // Highlight currently focused button
    void HighlightButton(int index)
    {
        EventSystem.current.SetSelectedGameObject(menuButtons[index].gameObject);
    }

    // Simulate clicking selected button
    void PressSelectedButton()
    {
        menuButtons[selectedIndex].onClick.Invoke();
        menuPanel.SetActive(false);   // Hide menu after pressing
    }
}
