using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;

public class ForcepsDesktop : MonoBehaviour
{
    public enum ForcepsControlHand
    {
        RightController,
        LeftController
    }

    [Header("Control Ownership")]
    public ForcepsControlHand controlHand;

    [Header("Menu Blocking")]
    public MonoBehaviour menuController; // drag MenuController here

    [Header("Movement")]
    public Transform pivot;
    public Transform tipPoint;
    public Transform shaft;

    public float moveSpeed = 0.05f;
    public float rotationSpeed = 40f;
    public float depthSpeed = 0.02f;
    public float minDepth = 0.02f;
    public float maxDepth = 0.15f;

    [Header("Keys (Desktop)")]
    public KeyCode rotateLeft = KeyCode.J;
    public KeyCode rotateRight = KeyCode.L;
    public KeyCode rotateUp = KeyCode.I;
    public KeyCode rotateDown = KeyCode.K;
    public KeyCode pushIn = KeyCode.O;
    public KeyCode pullOut = KeyCode.U;

    [Header("Quest 2 Settings")]
    public float holdTimeForDepth = 3f;

    // Right controller timers
    private float aHoldTimer = 0f;
    private float bHoldTimer = 0f;

    // Left controller timers
    private float xHoldTimer = 0f;
    private float yHoldTimer = 0f;

    private InputDevice rightController;
    private InputDevice leftController;

    [Header("State")]
    public bool isLocked = false;
    public bool isForcepsControlActive = false;
    public Transform heldAppendix;

    [HideInInspector]
    public float currentDepth = 0.07f;

    // Timing variables for double press
    private float lastAPressTime = 0f;
    private float lastBPressTime = 0f;
    public float doublePressThreshold = 0.3f; // 0.3s between presses to count as double

    private Vector3 currentPivotLocalPos;

    void Start()
    {
        rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
    }

    void Update()
    {
        if (isLocked) return;

        HandleKeyboardInput();
        HandleQuestControllerInput();

        currentDepth = Mathf.Clamp(currentDepth, minDepth, maxDepth);

        if (shaft != null)
            shaft.localPosition = Vector3.forward * currentDepth;
    }

    void DisableMenu()
    {
        if (menuController != null && menuController.enabled)
            menuController.enabled = false;
    }

    void EnableMenu()
    {
        if (menuController != null && !menuController.enabled)
            menuController.enabled = true;
    }

    // -------------------- DESKTOP INPUT --------------------
    void HandleKeyboardInput()
    {
        if (Input.GetKey(rotateLeft))
            pivot.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime, Space.World);
        if (Input.GetKey(rotateRight))
            pivot.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        if (Input.GetKey(pushIn))
            currentDepth += depthSpeed * Time.deltaTime;
        if (Input.GetKey(pullOut))
            currentDepth -= depthSpeed * Time.deltaTime;
    }

    // -------------------- QUEST 2 INPUT (RIGHT + LEFT) --------------------
    void HandleQuestControllerInput()
    {
        if (!rightController.isValid)
            rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (!leftController.isValid)
            leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        bool usingForceps = false;

        // ================= RIGHT CONTROLLER =================

        if (controlHand == ForcepsControlHand.RightController)
        {
            rightController.TryGetFeatureValue(CommonUsages.primaryButton, out bool aPressed);
            rightController.TryGetFeatureValue(CommonUsages.secondaryButton, out bool bPressed);

            // ----- LEFT/RIGHT ROTATION -----
            if (bPressed)
            {
                pivot.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime, Space.World);
                usingForceps = true;
            }

            if (aPressed)
            {
                pivot.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
                usingForceps = true;
            }

            // ----- DOUBLE PRESS FORWARD/BACK -----
            if (bPressed && Time.time - lastBPressTime < doublePressThreshold)
            {
                // Double press B → move forward
                currentPivotLocalPos.z += moveSpeed; // move along Z
                lastBPressTime = 0f; // reset timer
            }
            else if (bPressed)
            {
                lastBPressTime = Time.time;
            }

            if (aPressed && Time.time - lastAPressTime < doublePressThreshold)
            {
                // Double press A → move backward
                currentPivotLocalPos.z -= moveSpeed; // move along Z
                lastAPressTime = 0f; // reset timer
            }
            else if (aPressed)
            {
                lastAPressTime = Time.time;
            }
        }


        // ================= LEFT CONTROLLER =================
        if (controlHand == ForcepsControlHand.LeftController)
        {
            leftController.TryGetFeatureValue(CommonUsages.primaryButton, out bool xPressed);
            leftController.TryGetFeatureValue(CommonUsages.secondaryButton, out bool yPressed);

            if (yPressed)
            {
                pivot.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime, Space.World);
                yHoldTimer += Time.deltaTime;
                usingForceps = true;
            }
            else yHoldTimer = 0f;

            if (xPressed)
            {
                pivot.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
                xHoldTimer += Time.deltaTime;
                usingForceps = true;
            }
            else xHoldTimer = 0f;

            if (yHoldTimer >= holdTimeForDepth)
                currentDepth += depthSpeed * Time.deltaTime;

            if (xHoldTimer >= holdTimeForDepth)
                currentDepth -= depthSpeed * Time.deltaTime;
        }

        // ===== MENU BLOCKING =====
        if (usingForceps)
        {
            isForcepsControlActive = true;
            DisableMenu();
        }
        else
        {
            isForcepsControlActive = false;
            EnableMenu();
        }
    }

    public void LockForcepsAfterGrab()
    {
        isLocked = true;       // Stop movement & rotation
        rotationSpeed = 0f;    // Stop rotation
        moveSpeed = 0f;        // Stop translation
        depthSpeed = 0f;       // Stop depth changes
        Debug.Log("Forceps locked in place after grab.");
    }
}