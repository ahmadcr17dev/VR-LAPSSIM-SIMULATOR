using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class SnapToCut : MonoBehaviour
{
    [Header("Snap Settings")]
    public Transform snapPoint;        // Assign CutMarkQuad (or child empty GameObject)
    public float snapDistance = 0.05f; // Only snap when released within this distance

    public XRGrabInteractable grabInteractable;
    private Rigidbody rb;

    public OperationSteps operationSteps;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        rb = GetComponent<Rigidbody>();

        grabInteractable.selectExited.AddListener(OnRelease);
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        Debug.Log("📌 Port released. Checking distance...");

        if (snapPoint == null)
        {
            Debug.LogWarning("⚠️ Snap Point not assigned!");
            return;
        }

        // ✅ Check distance before snapping
        float distance = Vector3.Distance(transform.position, snapPoint.position);

        if (distance <= snapDistance)
        {
            Debug.Log("✅ Within snap zone. Snapping to belly...");

            transform.position = snapPoint.position;
            transform.rotation = Quaternion.Euler(-90f, 0, 0);
            operationSteps.OnPortFixedSuccessfully();
            operationSteps.LockPort(this.gameObject);

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
        }
        else
        {
            Debug.Log("❌ Too far from belly. No snap.");
        }
    }
}