using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Forcep1Snap : MonoBehaviour
{
    public LaparoscopeSnap laparoscopeSnap;
    public GameObject Forcep1Control;

    [Header("References")]
    public Transform shaftTip;       // Empty at shaft tip
    public Transform snapPoint;      // Port hole snap point
    public Collider[] handleColliders; // Colliders covering handles (optional)

    [Header("Snap Settings")]
    public float snapDistance = 0.05f;     // How close shaft tip must be to snap
    public Vector3 snapOffset = Vector3.zero;   // Local position offset after snap
    public Vector3 rotationOffset;

    [Header("VR References")]
    public XRGrabInteractable grab;
    private Rigidbody rb;

    private bool snapped = false;
    private int lastForcep1Score = 0;
    private bool scoreAdded = false; // ⭐ prevent double scoring
    public float Forcep2StartTime;

    // Initial transform (for reset)
    private Vector3 initialPos;
    private Quaternion initialRot;
    private Transform initialParent;

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        initialPos = transform.position;
        initialRot = transform.rotation;
        initialParent = transform.parent;
    }

    void Update()
    {
        if (snapped || shaftTip == null || snapPoint == null) return;

        float distance = Vector3.Distance(shaftTip.position, snapPoint.position);

        if (distance <= snapDistance && !grab.isSelected)
        {
            SnapForcep();
        }
    }

    void SnapForcep()
    {
        snapped = true;

        // 🔴 Disable grabbing immediately
        if (grab != null)
            grab.enabled = false;

        // 🔒 Lock physics
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        // 🔹 Disable handle colliders
        if (handleColliders != null)
        {
            foreach (var col in handleColliders)
                col.enabled = false;
        }

        // 🔹 Parent CORRECTLY
        transform.SetParent(snapPoint, false);

        // 🔹 Force exact pose
        transform.localPosition = new Vector3(-0.025f, 0.005f, 0.04f);
        transform.localRotation = Quaternion.Euler(-80f, -140f, 0f);

        AddForcep1Score();
        Debug.Log("Forcep snapped, locked, and permanently fixed in SILS port.");
    }

    int CalculateForcep1Score(float timeTaken)
    {
        if (timeTaken < 40f) return 5;
        if (timeTaken < 60f) return 4;
        if (timeTaken < 80f) return 3;
        if (timeTaken < 100f) return 2;
        return 1;
    }

    void AddForcep1Score()
    {
        if (scoreAdded) return;

        float rawTime = Time.time - laparoscopeSnap.Forcep1StartTime;

        float timeTaken = Mathf.Round(rawTime); // 1, 2, 3 seconds

        lastForcep1Score = CalculateForcep1Score(timeTaken);

        ScoreManager.Instance?.AddPoint(lastForcep1Score);
        scoreAdded = true;

        Debug.Log($"Laparo Camera Time: {timeTaken}s | Score: {lastForcep1Score}");
        StartCoroutine(ShowForcep1AfterSuccessPanelClosed());
    }

    IEnumerator ShowForcep1AfterSuccessPanelClosed()
    {
        yield return null; // wait one frame
        Forcep1Control.SetActive(true);
        Forcep2StartTime = Time.time;
        Debug.Log("Forcep2 panel activated after Forcep1 success panel closed.");
    }
}