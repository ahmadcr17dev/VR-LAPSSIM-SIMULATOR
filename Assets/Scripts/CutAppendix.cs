using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class CutAppendix : MonoBehaviour
{
    private XRGrabInteractable grab;

    [Header("Forceps References")]
    public ForcepsDesktop forceps1;   // Forceps1 script (cutting)
    public ForcepsDesktop forceps2;   // Forceps2 script (to attach appendix after cut)
    public Forcep2Snap Forcep2;
    public Forcep1Snap Forcep1;
    public LaparoscopeSnap LaparoscopeSnap;
    public OperationSteps operationSteps;
    public GameObject SilsPort;

    [Header("UI Panels")]
    public GameObject CutConfirmPanel;  // "Press trigger to cut infected appendix"
    public GameObject CutSuccessPanel;  // "Appendix has been cut"

    [Header("XR Settings")]
    public XRNode cutController = XRNode.RightHand; // Forceps1 controller (trigger)

    [Header("Cut Settings")]
    public Vector3 cutScale = new Vector3(0.05f, 0.05f, 0.05f); // scale after cutting

    private InputDevice controller;
    private bool touchingAppendix = false;
    private bool appendixCut = false;
    private Transform appendixTransform;

    void Start()
    {
        controller = InputDevices.GetDeviceAtXRNode(cutController);

        if (CutConfirmPanel != null) CutConfirmPanel.SetActive(false);
        if (CutSuccessPanel != null) CutSuccessPanel.SetActive(false);
    }

    void Update()
    {
        if (!touchingAppendix || appendixCut)
            return;

        if (!controller.isValid)
            controller = InputDevices.GetDeviceAtXRNode(cutController);

        // ✅ Trigger press = cut
        if (controller.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerPressed) && triggerPressed)
        {
            CutAppendixAction();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Appendix") && !appendixCut)
        {
            touchingAppendix = true;
            appendixTransform = other.transform;

            if (CutConfirmPanel != null)
                CutConfirmPanel.SetActive(true);

            Debug.Log("Forceps1 touching appendix → show cut panel.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Appendix") && !appendixCut)
        {
            touchingAppendix = false;
            appendixTransform = null;

            if (CutConfirmPanel != null)
                CutConfirmPanel.SetActive(false);
        }
    }

    void CutAppendixAction()
    {
        if (appendixTransform == null || forceps2 == null)
            return;

        appendixCut = true;

        // 🔹 Shrink appendix
        appendixTransform.localScale = cutScale;

        // 🔹 Attach to forceps2 tip
        appendixTransform.SetParent(forceps2.tipPoint, true);
        appendixTransform.localPosition = Vector3.zero;
        appendixTransform.localRotation = Quaternion.identity;

        forceps2.heldAppendix = appendixTransform;
        forceps2.isLocked = false;                 // unlock internal movement logic
        if (Forcep2.grab != null)
            Forcep2.grab.enabled = true;
        if(Forcep1.grab != null)
            Forcep1.grab.enabled = true;
        if (LaparoscopeSnap != null)
            LaparoscopeSnap.grabInteractable.enabled = true;
        operationSteps.UnlockPort(SilsPort.gameObject);

        // 🔁 UI transition
        if (CutConfirmPanel != null)
            CutConfirmPanel.SetActive(false);
        if (CutSuccessPanel != null)
            CutSuccessPanel.SetActive(true);

        // 🔒 Optionally lock Forceps1 after cutting
        //forceps1.LockForcepsAfterGrab();

        Debug.Log("Appendix successfully cut and attached to Forceps2.");
    }

    // Optional OK button for CutSuccessPanel
    public void CloseCutConfirmPanel()
    {
        if (CutConfirmPanel != null)
            CutConfirmPanel.SetActive(false);
    }
}