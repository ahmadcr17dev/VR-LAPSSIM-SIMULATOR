using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class PortReset : MonoBehaviour
{
    private Vector3 startPos;
    private Quaternion startRot;
    private Transform startParent;

    private XRGrabInteractable grab;
    private FixedJoint joint;

    void Awake()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        startParent = transform.parent;

        grab = GetComponent<XRGrabInteractable>();
    }

    public void ResetPort()
    {
        // 🔴 Force release from hands
        if (grab != null && grab.isSelected)
        {
            grab.interactionManager.SelectExit(
                grab.firstInteractorSelecting,
                grab
            );
        }

        // 🔴 Remove joint if snapped
        joint = GetComponent<FixedJoint>();
        if (joint != null)
        {
            Destroy(joint);
        }

        // 🔴 Detach from belly
        transform.SetParent(startParent);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        transform.position = startPos;
        transform.rotation = startRot;

        if (rb != null)
            rb.isKinematic = false;
    }
}
