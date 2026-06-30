using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;    // Root Canvas for Menu
    public GameObject menuPanel;     // Menu panel to toggle
    public GameObject hamburgerMenu; // Hamburger button

    public string mainSceneName = "Hospital"; // Main VR scene
    private bool hamburgerShown = false;

    void Awake()
    {
        // Hide menu if we are NOT in the main scene
        if (SceneManager.GetActiveScene().name != mainSceneName)
        {
            if (menuCanvas != null) menuCanvas.SetActive(false);
            return;
        }

        // Otherwise, make sure canvas is active
        if (menuCanvas != null) menuCanvas.SetActive(true);

        // Hide panel initially
        if (menuPanel != null) menuPanel.SetActive(false);

        // Hide hamburger temporarily
        if (hamburgerMenu != null) hamburgerMenu.SetActive(false);
    }

    void Start()
    {
        // Show hamburger immediately in main scene
        ShowHamburger();
    }

    private void ShowHamburger()
    {
        if (hamburgerMenu != null)
        {
            hamburgerMenu.SetActive(true);

            // Anchor to top-right
            RectTransform rt = hamburgerMenu.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(1, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(1, 1);
                rt.anchoredPosition = new Vector2(-20f, -20f);
            }

            hamburgerShown = true;
        }
    }

    void Update()
    {
        if (!hamburgerShown) return;

        // Keyboard toggle: X key
        if (Input.GetKeyDown(KeyCode.X))
            ToggleMenu();

        // VR controller toggle
        InputDevice rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out bool pressed) && pressed)
            ToggleMenu();
    }

    public void ToggleMenu()
    {
        if (menuPanel != null)
            menuPanel.SetActive(!menuPanel.activeSelf);
    }
}
