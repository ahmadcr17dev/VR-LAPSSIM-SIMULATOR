using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class LaparoscopeSnap : MonoBehaviour
{
    public OperationSteps OperationSteps;

    [Header("Snap Settings")]
    public Transform snapPoint;
    public float snapDistance = 0.05f;
    public Vector3 snapOffset = Vector3.zero;

    public XRGrabInteractable grabInteractable;
    private Rigidbody rb;
    private bool snapped = false;

    [Header("UI Settings")]
    public GameObject connectCameraPanel;
    public GameObject monitor;
    public GameObject LaparoCameraSuccessPanel;
    public Camera laparoCamera;
    public bool cameraConnected = false;

    private Renderer monitorRenderer;
    private UnityEngine.UI.RawImage monitorRawImage;

    private int lastCameraScore = 0;
    private bool scoreAdded = false; // ⭐ prevent double scoring

    public float ForcepPanelDelay = 5f;
    public GameObject ForcepPanel;
    public float Forcep1StartTime;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        if (monitor != null)
        {
            monitorRenderer = monitor.GetComponent<Renderer>();
            monitorRawImage = monitor.GetComponent<UnityEngine.UI.RawImage>();
        }
    }

    void Update()
    {
        if (snapped) return;

        if (Vector3.Distance(transform.position, snapPoint.position + snapOffset) <= snapDistance)
        {
            if (!grabInteractable.isSelected)
            {
                SnapToPort();
            }
        }
    }

    void SnapToPort()
    {
        snapped = true;

        transform.position = snapPoint.position + snapOffset;
        transform.rotation = snapPoint.rotation;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.SetParent(snapPoint);

        // ⭐ Start timing here
        //OperationSteps.LaparoCameraStartTime = Time.time;
        scoreAdded = false;

        if (connectCameraPanel != null)
            connectCameraPanel.SetActive(true);

        Debug.Log("Laparoscope camera snapped into port.");
    }

    public void ResetCamera(Transform startParent)
    {
        snapped = false;
        transform.SetParent(startParent);
        if (rb != null)
            rb.isKinematic = false;

        cameraConnected = false;
        scoreAdded = false;

        if (monitorRenderer != null) monitorRenderer.material.mainTexture = null;
        if (monitorRawImage != null) monitorRawImage.texture = null;
    }

    public void OnConnectCameraClicked()
    {
        if (connectCameraPanel != null)
            connectCameraPanel.SetActive(false);

        cameraConnected = true;

        if (laparoCamera == null)
        {
            Debug.LogError("Laparoscope camera not assigned!");
            return;
        }

        if (laparoCamera.targetTexture != null && !laparoCamera.targetTexture.IsCreated())
        {
            laparoCamera.targetTexture.Create();
        }

        StartCoroutine(AssignTextureAndScore(laparoCamera.targetTexture));

        Debug.Log("Laparoscope camera connected.");
    }

    // ⭐ Texture assignment + scoring in ONE coroutine
    IEnumerator AssignTextureAndScore(RenderTexture rt)
    {
        yield return null; // wait one frame

        if (monitorRenderer != null)
            monitorRenderer.material.mainTexture = rt;

        if (monitorRawImage != null)
            monitorRawImage.texture = rt;

        AddLaparoCameraScore(); // ⭐ score ONLY after feed appears
    }

    void AddLaparoCameraScore()
    {
        if (scoreAdded) return;

        float rawTime = Time.time - OperationSteps.LaparoCameraStartTime;

        float timeTaken = Mathf.Round(rawTime); // 1, 2, 3 seconds

        lastCameraScore = CalculateLaparoCameraScore(timeTaken);

        ScoreManager.Instance?.AddPoint(lastCameraScore);
        scoreAdded = true;

        Debug.Log($"Laparo Camera Time: {timeTaken}s | Score: {lastCameraScore}");
        LaparoCameraSuccessPanel.SetActive(true);
        LockLaparoscope();
        StartCoroutine(ShowForcep1AfterSuccessPanelClosed());
    }

    int CalculateLaparoCameraScore(float timeTaken)
    {
        if (timeTaken < 40f) return 5;
        if (timeTaken <= 60f) return 4;
        if (timeTaken <= 80f) return 3;
        if (timeTaken <= 100f) return 2;
        return 1;
    }

    void LockLaparoscope()
    {
        // Prevent player from grabbing again
        grabInteractable.enabled = false;

        // Keep the object in place
        if (rb != null)
        {
            rb.isKinematic = true;    // stop physics motion
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.None; // ❌ don't freeze everything
        }

        Debug.Log("Laparoscope locked permanently.");
    }

    // show forcep1 panel
    IEnumerator ShowForcep1AfterSuccessPanelClosed()
    {
        // Wait until the success panel is inactive
        while (LaparoCameraSuccessPanel.activeSelf)
        {
            yield return null; // wait one frame
        }

        // Now show Forcep1 panel
        ForcepPanel.SetActive(true);
        Forcep1StartTime = Time.time;
        Debug.Log("Forcep1 panel activated after Laparo camera success panel closed.");
    }
}