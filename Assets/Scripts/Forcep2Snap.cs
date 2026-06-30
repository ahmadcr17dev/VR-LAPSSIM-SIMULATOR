using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Forcep2Snap : MonoBehaviour
{
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
    public Forcep1Snap forcep1Snap;
    private bool scoreAdded = false;
    private int lastForcep2Score = 0;
    public GameObject Forcep2Control;

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

        // 🔹 Lock physics immediately
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        // 🔹 Optional: disable handle colliders to prevent jerking
        if (handleColliders != null)
        {
            foreach (var col in handleColliders)
                col.enabled = false;
        }

        // 🔹 Parent to snap point
        transform.SetParent(snapPoint, false);

        // 🔹 Apply local offsets (position + rotation)
        transform.localPosition = new Vector3(-0.03f, 0.005f, 0.05f);
        transform.localRotation = Quaternion.Euler(-80f, -140f, 0f);

        AddForcep2Score();
        Debug.Log("Forcep snapped and locked into SILS port.");
    }

    int CalculateForcep2Score(float timeTaken)
    {
        if (timeTaken < 40f) return 5;
        if (timeTaken < 60f) return 4;
        if (timeTaken < 80f) return 3;
        if (timeTaken < 100f) return 2;
        return 1;
    }

    void AddForcep2Score()
    {
        if (scoreAdded) return;

        float rawTime = Time.time - forcep1Snap.Forcep2StartTime;

        float timeTaken = Mathf.Round(rawTime); // 1, 2, 3 seconds

        lastForcep2Score = CalculateForcep2Score(timeTaken);

        ScoreManager.Instance?.AddPoint(lastForcep2Score);
        scoreAdded = true;

        Debug.Log($"Laparo Camera Time: {timeTaken}s | Score: {lastForcep2Score}");
        StartCoroutine(ShowForcep2AfterSuccessPanel());
    }

    IEnumerator ShowForcep2AfterSuccessPanel()
    {
        yield return null;
        Forcep2Control.SetActive(true);
    }
}