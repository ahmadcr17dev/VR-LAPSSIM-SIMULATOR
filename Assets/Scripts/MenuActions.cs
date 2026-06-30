using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuActions : MonoBehaviour
{
    private bool isPaused = false;

    [Header("UI Panels")]
    public GameObject menuPanel;
    public GameObject MovementInstructions;
    public GameObject ProjectInfoPanel;
    public GameObject TeamInfoPanel;
    public GameObject SurgeryInfoPanel;
    public GameObject ReadyPanel;
    public GameObject Step1Panel;
    public GameObject AnasthesiaPanel;

    [Header("Teleport")]
    public Transform operationRoomPoint;
    public XROrigin xrorigin;

    [Header("XR Camera (HMD)")]
    public Transform xrCamera;

    [Header("Procedure Button")]
    public Button StartProcedureButton;
    public Button TeleportToOperationRoomButton;

    private bool isInsideOperationRoom = false;
    public static bool ProcedureStarted = false;
    [SerializeField]
    private Collider operationRoomTrigger;

    public OperationSteps operationSteps;

    void Start()
    {
        if (operationRoomTrigger == null || !operationRoomTrigger.isTrigger)
        {
            Debug.LogError("Operation Room must have a Trigger Collider!");
        }

        if (StartProcedureButton != null)
            StartProcedureButton.interactable = false;
    }

    void Update()
    {
        CheckIfCameraInsideOperationRoom();
        HandleCtrlX();
    }

    // -------------------------------------------------
    // CAMERA INSIDE OPERATION ROOM CHECK
    // -------------------------------------------------
    void CheckIfCameraInsideOperationRoom()
    {
        if (xrCamera == null || operationRoomTrigger == null)
            return;

        // Use ClosestPoint instead of bounds.Contains (VR-safe)
        Vector3 closestPoint = operationRoomTrigger.ClosestPoint(xrCamera.position);
        bool isInside = Vector3.Distance(closestPoint, xrCamera.position) < 0.01f;

        if (isInside && !isInsideOperationRoom)
        {
            isInsideOperationRoom = true;
            StartProcedureButton.interactable = true;
            TeleportToOperationRoomButton.interactable = false;
            Debug.Log("Entered Operation Room → StartProcedure enabled");
        }
        else if (!isInside && isInsideOperationRoom)
        {
            isInsideOperationRoom = false;
            StartProcedureButton.interactable = false;
            TeleportToOperationRoomButton.interactable = true;
            ReadyPanel.SetActive(false);
            ProcedureStarted = false;
            Debug.Log("Exited Operation Room → StartProcedure disabled");
        }
    }

    // -------------------------------------------------
    // TELEPORT
    // -------------------------------------------------
    public void TeleportToOperationRoom()
    {
        if (xrorigin == null || operationRoomPoint == null)
            return;

        xrorigin.MoveCameraToWorldLocation(operationRoomPoint.position);

        if (menuPanel != null)
            menuPanel.SetActive(false);

        // Force immediate state refresh after teleport
        CheckIfCameraInsideOperationRoom();
    }

    // -------------------------------------------------
    // PROCEDURE
    // -------------------------------------------------
    public void ShowReadyPanel()
    {
        if (StartProcedureButton.interactable)
        {
            ReadyPanel.SetActive(true);
            menuPanel.SetActive(false);
            ProcedureStarted = true;
            ScoreManager.Instance.OnProcedureStarted();
        }
    }

    public void CloseReadyPanel()
    {
        ReadyPanel.SetActive(false);
        menuPanel.SetActive(false);
        Step1Panel.SetActive(true);
    }

    public void CloseStep1Panel()
    {
        Step1Panel.SetActive(false);
        AnasthesiaPanel.SetActive(true);
    }

    // -------------------------------------------------
    // PAUSE / PLAY
    // -------------------------------------------------
    public void TogglePausePlay()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (menuPanel != null)
            menuPanel.SetActive(false);
    }

    // -------------------------------------------------
    // MOVEMENT INSTRUCTIONS
    // -------------------------------------------------
    public void MovementInstruction()
    {
        MovementInstructions.SetActive(true);
        menuPanel.SetActive(false);
    }

    public void CloseMovementInstruction()
    {
        MovementInstructions.SetActive(false);
    }

    // -------------------------------------------------
    // CTRL + X
    // -------------------------------------------------
    void HandleCtrlX()
    {
        bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        if (ctrl && Input.GetKeyDown(KeyCode.X))
        {
            MovementInstructions.SetActive(false);
            ProjectInfoPanel.SetActive(false);
            TeamInfoPanel.SetActive(false);
            SurgeryInfoPanel.SetActive(false);
            menuPanel.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    // -------------------------------------------------
    // INFO PANELS
    // -------------------------------------------------
    public void SurgeryInfoPanels()
    {
        SurgeryInfoPanel.SetActive(true);
        menuPanel.SetActive(false);
        Time.timeScale = 0f;
    }

    public void CloseSurgeryInfoPanel()
    {
        SurgeryInfoPanel.SetActive(false);
        menuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ProjectInfoPanels()
    {
        ProjectInfoPanel.SetActive(true);
        menuPanel.SetActive(false);
        Time.timeScale = 0f;
    }

    public void CloseProjectInfoPanel()
    {
        ProjectInfoPanel.SetActive(false);
        menuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void TeamInfoPanels()
    {
        TeamInfoPanel.SetActive(true);
        menuPanel.SetActive(false);
        Time.timeScale = 0f;
    }

    public void CloseTeamInfoPanel()
    {
        TeamInfoPanel.SetActive(false);
        menuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void CloseMenuPanel()
    {
        menuPanel.SetActive(false);
    }

    // -------------------------------------------------
    // EXIT
    // -------------------------------------------------
    public void ExitApplication()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}