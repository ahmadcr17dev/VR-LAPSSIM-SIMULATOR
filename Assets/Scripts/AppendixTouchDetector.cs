using UnityEngine;
using UnityEngine.XR;

public class AppendixTouchDetector : MonoBehaviour
{
    [Header("Forceps Reference")]
    public ForcepsDesktop forcepsDesktop;   // Forceps 2 script

    [Header("UI Panels")]
    public GameObject GrabConfirmPanel;           // "Press trigger to grab"
    public GameObject GrabSuccessPanel;         // "Appendix has been grabbed"

    [Header("XR Settings")]
    public XRNode grabController = XRNode.LeftHand;

    private InputDevice controller;
    private bool touchingAppendix = false;
    private bool appendixGrabbed = false;
    private Transform appendixTransform;

    void Start()
    {
        controller = InputDevices.GetDeviceAtXRNode(grabController);

        if (GrabConfirmPanel != null) GrabConfirmPanel.SetActive(false);
        if (GrabSuccessPanel != null) GrabSuccessPanel.SetActive(false);
    }

    void Update()
    {
        if (!touchingAppendix || appendixGrabbed)
            return;

        if (!controller.isValid)
            controller = InputDevices.GetDeviceAtXRNode(grabController);

        // ✅ Trigger press = grab
        if (controller.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed)
            && triggerPressed)
        {
            GrabAppendix();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Appendix") && !appendixGrabbed)
        {
            touchingAppendix = true;
            appendixTransform = other.transform;

            if (GrabConfirmPanel != null)
                GrabConfirmPanel.SetActive(true);

            Debug.Log("Appendix touched → show grab instruction panel.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Appendix") && !appendixGrabbed)
        {
            touchingAppendix = false;
            appendixTransform = null;

            if (GrabConfirmPanel != null)
                GrabConfirmPanel.SetActive(false);
        }
    }

    void GrabAppendix()
    {
        if (appendixTransform == null || forcepsDesktop == null)
            return;

        appendixGrabbed = true;

        // 🔒 Attach appendix to forceps tip
        //appendixTransform.SetParent(forcepsDesktop.tipPoint);
        //appendixTransform.localPosition = Vector3.zero;
        //appendixTransform.localRotation = Quaternion.identity;

        forcepsDesktop.heldAppendix = appendixTransform;
        // 🔒 Lock the forceps after grabbing
        //forcepsDesktop.LockForcepsAfterGrab();

        // 🔁 UI transition
        if (GrabConfirmPanel != null)
            GrabConfirmPanel.SetActive(false);

        if (GrabSuccessPanel != null)
            GrabSuccessPanel.SetActive(true);

        Debug.Log("Appendix successfully grabbed.");
    }

    // Optional OK button hook for success panel
    public void CloseGrabConfirmPanel()
    {
        if (GrabConfirmPanel != null)
            GrabConfirmPanel.SetActive(false);
    }
}