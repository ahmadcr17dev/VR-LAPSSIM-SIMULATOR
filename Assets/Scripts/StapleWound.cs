using TMPro;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class StaplerWoundHandler : MonoBehaviour
{
    [Header("Wound Objects")]
    public GameObject woundObject;      // Cut image
    public GameObject stitchObject;     // Stitches object
    public GameObject ResultPanel;
    public TextMeshProUGUI congratsText;
    public TextMeshProUGUI scoreText;

    private InputDevice leftController;
    private InputDevice rightController;

    private bool touchingWound = false;
    private bool alreadyStapled = false;

    [Header("Grab Rotation Lock")]
    public Vector3 grabRotationEuler = new Vector3(180f, 180f, 0f);
    private float delayBeforeShow = 2f;

    [Header("Stapler Reference")]
    public XRGrabInteractable staplerGrab;

    void Awake()
    {
        if (staplerGrab != null)
        {
            staplerGrab.selectEntered.AddListener(OnGrab);
        }
        else
        {
            Debug.LogError("❌ StaplerGrab NOT assigned in Inspector!");
        }
    }

    void Start()
    {
        leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        if (stitchObject != null)
            stitchObject.SetActive(false);

        if (ResultPanel != null)
        {
            ResultPanel.SetActive(false);
        }
    }

    void Update()
    {
        if (alreadyStapled || !touchingWound)
            return;

        if (!leftController.isValid)
            leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        if (!rightController.isValid)
            rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

        bool leftTriggerPressed = false;
        bool rightTriggerPressed = false;

        leftController.TryGetFeatureValue(CommonUsages.triggerButton, out leftTriggerPressed);
        rightController.TryGetFeatureValue(CommonUsages.triggerButton, out rightTriggerPressed);

        if (leftTriggerPressed || rightTriggerPressed)
        {
            ApplyStitches();
        }
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        // 🔒 Force fixed rotation on grab
        //transform.rotation = Quaternion.Euler(grabRotationEuler);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == woundObject)
        {
            touchingWound = true;
            Debug.Log("Stapler touching wound. Press trigger to stitch.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == woundObject)
        {
            touchingWound = false;
        }
    }

    void OnDisable()
    {
        if (!Application.isPlaying) return;
        Debug.LogError("StaplerWoundHandler disabled unexpectedly!");
    }

    void ApplyStitches()
    {
        alreadyStapled = true;

        // ❌ Hide wound
        if (woundObject != null)
            woundObject.SetActive(false);

        // ✅ Move + show stitches
        if (stitchObject != null)
        {
            stitchObject.transform.position = woundObject.transform.position;
            stitchObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            stitchObject.SetActive(true);
            ScoreManager.Instance.AddPoint(10);
        }

        Debug.Log("✅ Stitching completed.");
        ShowResult();
    }

    public void ShowResult()
    {
        StartCoroutine(ShowResultAfterDelay());
    }

    private System.Collections.IEnumerator ShowResultAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeShow);

        int finalScore = ScoreManager.Instance.GetTotalScore();

        if (congratsText != null)
            congratsText.text = "🎉 Congratulations!\nSurgery Completed Successfully";

        if (scoreText != null)
            scoreText.text = "Your Score: " + finalScore + " / 50 ";

        if (ResultPanel != null)
            ResultPanel.SetActive(true);
    }
}