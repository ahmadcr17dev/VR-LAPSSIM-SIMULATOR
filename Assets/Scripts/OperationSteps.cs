using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class OperationSteps : MonoBehaviour
{
    [SerializeField]
    public BleedController BloodEffect;
    public KnifeCut KnifeCut;
    public PortReset resetPort;

    [Header("GameObjects")]
    public GameObject KnifePanel;
    public GameObject KnifeSuccessPanel;
    public GameObject SoakingPadPanel;
    public GameObject SoakingPadSuccessPanel;
    public GameObject PortPanel;
    public GameObject PortSuccessPanel;
    public GameObject LaparoscopeCameraPanel;

    // Retry Panels
    public GameObject RetryKnifePanel;
    public GameObject RetryPortPanel;

    // New for CO2 Insufflation
    public GameObject CO2IntroPanel;       // Intro panel explaining insufflation
    public GameObject CO2LevelPanel;       // Panel to input CO2 level
    public GameObject PatientBelly;        // Parent object to scale for insufflation
    public float bellyExpandSpeed = 0.5f;  // Speed of belly expansion
    public float maxBellyScale = 1.2f;     // Maximum scale for belly expansion

    [Header("Delay Settings")]
    public float knifePanelDelay = 5f;
    public float soakingPanelDelay = 5f;
    public float portPanelDelay = 5f;

    // ----------------------------
    // FLAGS
    // ----------------------------
    private bool KnifeStepShown = false;
    private bool knifeCoroutineStarted = false;
    private bool knifeCutCompleted = false;
    private bool waitingForKnifeDecision = false;

    private bool soakingStepShown = false;
    private bool soakingCoroutineStarted = false;
    private bool soakingCompleted = false;

    private bool portStepShown = false;
    private bool portCoroutineStarted = false;
    private bool portFixed = false;
    private bool waitingForPortDecision = false;

    private bool co2StepStarted = false;

    // ----------------------------
    // TIMERS
    // ----------------------------
    private float knifeStartTime;
    private float soakingStartTime;
    private float portStartTime;
    public float LaparoCameraStartTime;

    // ____________________________
    // Scores
    // ____________________________
    private int lastKnifeScore = 0;
    private int lastSoakingScore = 0;
    private int lastPortScore = 0;

    public GameObject Knife;
    public GameObject SoakingPad;
    public GameObject SilsPort;
    private Vector3 knifeInitialPos;
    private Quaternion knifeInitialRot;

    private XRGrabInteractable grabInteractable;
    private Rigidbody rb;

    void Start()
    {
        HideAllProcedurePanels();

        if (PatientBelly != null)
            PatientBelly.transform.localScale = Vector3.one;

        if (Knife != null)
        {
            knifeInitialPos = Knife.transform.position;
            knifeInitialRot = Knife.transform.rotation;
        }
    }

    void Update()
    {
        // 🚫 HARD STOP IF PROCEDURE NOT STARTED
        if (!MenuActions.ProcedureStarted)
        {
            HideAllProcedurePanels();
            ResetAllFlags();
            return; // ⛔ THIS WAS MISSING
        }

        if (AnesthesiaManager.AnesthesiaStarted && !KnifeStepShown && !knifeCoroutineStarted)
        {
            knifeCoroutineStarted = true;
            StartCoroutine(ShowKnifeStepAfterDelay());
        }

        if (KnifeStepShown && knifeCutCompleted && !waitingForKnifeDecision && !soakingStepShown && !soakingCoroutineStarted)
        {
            soakingCoroutineStarted = true;
            StartCoroutine(ShowSoakingStepAfterDelay());
        }

        if (soakingStepShown && soakingCompleted && !portStepShown && !portCoroutineStarted)
        {
            portCoroutineStarted = true;
            StartCoroutine(ShowPortPanelStepsAfterDelay());
        }

        if (portFixed && !co2StepStarted && !PortSuccessPanel.activeSelf)
        {
            co2StepStarted = true;
            ShowCO2IntroPanel();
        }

        if (AnesthesiaManager.GameEnded)
        {
            HideAllProcedurePanels();
        }
    }

    // ================= HELPER METHODS =================

    void HideAllProcedurePanels()
    {
        KnifePanel?.SetActive(false);
        KnifeSuccessPanel?.SetActive(false);
        SoakingPadPanel?.SetActive(false);
        SoakingPadSuccessPanel?.SetActive(false);
        PortPanel?.SetActive(false);
        PortSuccessPanel?.SetActive(false);
        CO2IntroPanel?.SetActive(false);
        CO2LevelPanel?.SetActive(false);
        LaparoscopeCameraPanel?.SetActive(false);
        RetryKnifePanel?.SetActive(false);
        RetryPortPanel?.SetActive(false);
    }

    void ResetAllFlags()
    {
        KnifeStepShown = false;
        knifeCoroutineStarted = false;
        knifeCutCompleted = false;

        soakingStepShown = false;
        soakingCoroutineStarted = false;
        soakingCompleted = false;

        portStepShown = false;
        portCoroutineStarted = false;
        portFixed = false;

        co2StepStarted = false;
    }

    // ============================
    // KNIFE STEP (UNCHANGED)
    // ============================
    IEnumerator ShowKnifeStepAfterDelay()
    {
        yield return new WaitForSecondsRealtime(knifePanelDelay);
        if (AnesthesiaManager.GameEnded) yield break;

        KnifeStepShown = true;
        KnifePanel.SetActive(true);
        knifeStartTime = Time.time;
        knifeCutCompleted = false;
    }

    public void OnKnifeCutSuccessful()
    {
        if (knifeCutCompleted || waitingForKnifeDecision) return;

        float timeTaken = Time.time - knifeStartTime;
        lastKnifeScore = CalculateKnifeScore(timeTaken);

        if (lastKnifeScore >= 4)
        {
            CloseKnifePanel();
            knifeCutCompleted = true;
            //BloodEffect.ResumeBlood();
            ScoreManager.Instance?.AddPoint(lastKnifeScore);
            ShowKnifeSuccessPanel();
        }
        else
        {
            waitingForKnifeDecision = true;
            RetryKnifePanel.SetActive(true);
        }
    }

    int CalculateKnifeScore(float timeTaken)
    {
        if (timeTaken <= 20f) return 5;
        if (timeTaken > 20f && timeTaken <= 40f ) return 4;
        if (timeTaken > 40f && timeTaken <= 60f) return 3;
        if (timeTaken > 60f && timeTaken <= 80f ) return 2;
        return 1;
    }

    public void ShowKnifeSuccessPanel(float duration = 5f)
    {
        StartCoroutine(ShowKnifeSuccessRoutine(duration));
    }

    IEnumerator ShowKnifeSuccessRoutine(float time)
    {
        KnifeSuccessPanel.SetActive(true);
        yield return new WaitForSecondsRealtime(time);
        KnifeSuccessPanel.SetActive(false);
    }

    IEnumerator RestartKnifeStepImmediate()
    {
        yield return null; // wait one frame to let UI & XR settle

        KnifeStepShown = true;
        KnifePanel.SetActive(true);
        knifeStartTime = Time.time;
        knifeCutCompleted = false;
        waitingForKnifeDecision = false;
    }

    // Knife retry
    public void OnRetryKnifeStep()
    {
        // 1️⃣ Reset all knife step flags
        //KnifeStepShown = false;
        //knifeCoroutineStarted = false;
        knifeCutCompleted = false;
        waitingForKnifeDecision = false;

        // 2️⃣ Hide UI
        RetryKnifePanel.SetActive(false);
        KnifePanel.SetActive(false);

        // 3️⃣ Reset blood
        BloodEffect.ResetBlood();

        // 4️⃣ Reset knife position and physics
        if (Knife != null)
        {
            Knife.transform.position = knifeInitialPos + Vector3.up * 0.5f; // lift slightly
            Knife.transform.rotation = knifeInitialRot;

            var rb = Knife.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        // 5️⃣ Reset KnifeCut script
        if (KnifeCut != null)
            KnifeCut.ResetKnifeCut();

        // 6️⃣ Restart knife step exactly like first time
        StartCoroutine(RestartKnifeStepImmediate());
    }

    // Called by "Proceed" button (after failed knife cut)
    public void OnProceedAfterKnifeFail()
    {
        // 1️⃣ Hide retry panel
        RetryKnifePanel.SetActive(false);

        // 2️⃣ Mark step as completed so next step (soaking) can start
        knifeCutCompleted = true;
        waitingForKnifeDecision = false;

        // 3️⃣ Add points for failed knife attempt if desired (optional, e.g., 0)
        ScoreManager.Instance?.AddPoint(lastKnifeScore);
    }

    public void CloseKnifePanel() => KnifePanel.SetActive(false);

    // ============================
    // SOAKING PAD STEP
    // ============================
    IEnumerator ShowSoakingStepAfterDelay()
    {
        yield return new WaitForSecondsRealtime(soakingPanelDelay);
        if (AnesthesiaManager.GameEnded) yield break;

        soakingStepShown = true;
        SoakingPadPanel.SetActive(true);
        soakingStartTime = Time.time;
        soakingCompleted = false;
    }

    public void OnSoakingPadSuccessful()
    {

        float timeTaken = Time.time - soakingStartTime;
        lastSoakingScore = CalculateSoakingScore(timeTaken);
        CloseSoakingPadPanel();
        soakingCompleted = true;
        ScoreManager.Instance?.AddPoint(lastSoakingScore);
        ShowSoakingSuccessPanel();
    }

    int CalculateSoakingScore(float timeTaken)
    {
        if (timeTaken <= 15f) return 5;
        if (timeTaken > 15f && timeTaken <= 25f) return 4;
        if (timeTaken > 25f && timeTaken >= 40f) return 3;
        if (timeTaken > 40f && timeTaken <= 60f) return 2;
        return 1;
    }

    public void ShowSoakingSuccessPanel(float duration = 5f)
    {
        StartCoroutine(ShowSoakingSuccessRoutine(duration));
    }

    IEnumerator ShowSoakingSuccessRoutine(float time)
    {
        SoakingPadSuccessPanel.SetActive(true);
        yield return new WaitForSecondsRealtime(time);
        SoakingPadSuccessPanel.SetActive(false);
    }

    public void CloseSoakingPadPanel() => SoakingPadPanel.SetActive(false);


    // ============================
    // PORT FIXING STEP
    // ============================
    IEnumerator ShowPortPanelStepsAfterDelay()
    {
        yield return new WaitForSecondsRealtime(portPanelDelay);
        if (AnesthesiaManager.GameEnded) yield break;

        portStepShown = true;
        PortPanel.SetActive(true);
        portStartTime = Time.time;
        portFixed = false;

        Debug.Log("Port fixing step started");
    }

    public void OnPortFixedSuccessfully()
    {
        if (portFixed || waitingForPortDecision) return;

        float timeTaken = Time.time - portStartTime;
        lastPortScore = CalculatePortScore(timeTaken);

        // Require "good enough" performance (like knife)
        if (lastPortScore >= 4)   // you can change threshold
        {
            portFixed = true;
            waitingForPortDecision = false;

            PortPanel.SetActive(false);
            ScoreManager.Instance?.AddPoint(lastPortScore);
            ShowPortSuccessPanel();

            Debug.Log($"Port fixed in {timeTaken:F1}s | Score: {lastPortScore}");
        }
        else
        {
            waitingForPortDecision = true;
            RetryPortPanel.SetActive(true);
        }
    }

    int CalculatePortScore(float timeTaken)
    {
        if (timeTaken <= 30f) return 5;
        if (timeTaken > 30f && timeTaken <= 50f) return 4;
        if (timeTaken > 50f && timeTaken <= 70f) return 3;
        if (timeTaken > 70f) return 2;
        return 1;
    }

    public void ShowPortSuccessPanel(float duration = 5f)
    {
        StartCoroutine(ShowPortSuccessRoutine(duration));
    }

    IEnumerator ShowPortSuccessRoutine(float time)
    {
        PortSuccessPanel.SetActive(true);
        yield return new WaitForSecondsRealtime(time);
        PortSuccessPanel.SetActive(false);
    }

    public void ClosePortPanel()
    {
        PortPanel.SetActive(false);
    }

    public void OnRetryPortStep()
    {
        resetPort.ResetPort();
        waitingForPortDecision = false;
        RetryPortPanel.SetActive(false);

        // Restart timing
        portStartTime = Time.time;
        portFixed = false;

        PortPanel.SetActive(true);
    }

    public void OnProceedAfterPortFail()
    {
        waitingForPortDecision = false;
        RetryPortPanel.SetActive(false);

        portFixed = true;
        PortPanel.SetActive(false);

        // Add the low score anyway
        ScoreManager.Instance?.AddPoint(lastPortScore);
    }

    public void LockPort(GameObject port)
    {
        // 1️⃣ Get XRGrabInteractable and Rigidbody
        XRGrabInteractable grab = port.GetComponent<XRGrabInteractable>();
        Rigidbody rb = port.GetComponent<Rigidbody>();

        // 2️⃣ Force release if grabbed
        if (grab != null && grab.isSelected)
        {
            var interactors = grab.interactorsSelecting;
            for (int i = interactors.Count - 1; i >= 0; i--)
            {
                grab.interactionManager.SelectExit(interactors[i], grab);
            }
        }

        // 3️⃣ Disable grab and XR interaction
        if (grab != null)
        {
            grab.enabled = false;
            grab.interactionLayers = 0;
        }

        // 4️⃣ Freeze Rigidbody
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        // 5️⃣ Disable all colliders
        Collider[] cols = port.GetComponentsInChildren<Collider>();
        foreach (var col in cols)
            col.enabled = false;

        Debug.Log("✅ SILS Port locked permanently!");
    }

    public void UnlockPort(GameObject port)
    {
        XRGrabInteractable grab = port.GetComponent<XRGrabInteractable>();
        Rigidbody rb = port.GetComponent<Rigidbody>();

        // 1️⃣ Enable XR grabbing
        if (grab != null)
        {
            grab.enabled = true;
            grab.interactionLayers = InteractionLayerMask.GetMask("Default");

            // 🔄 Refresh XR collider cache
            grab.colliders.Clear();
            grab.colliders.AddRange(port.GetComponentsInChildren<Collider>());
        }

        // 2️⃣ Restore Rigidbody
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.None;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 3️⃣ Re-enable colliders
        Collider[] cols = port.GetComponentsInChildren<Collider>();
        foreach (var col in cols)
            col.enabled = true;

        Debug.Log("✅ SILS Port unlocked and grabbable!");
    }

    // ============================
    // CO2 INSUFFLATION LOGIC
    // ============================
    void ShowCO2IntroPanel()
    {
        if (CO2IntroPanel != null)
        {
            CO2IntroPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("CO2IntroPanel not assigned in Inspector!");
        }
    }

    // Call this on OK button of CO2IntroPanel
    public void OnCO2IntroOkClicked()
    {
        CO2IntroPanel.SetActive(false);
        CO2LevelPanel.SetActive(true);
    }

    // Call this after user inputs CO2 value and presses confirm
    public void OnCO2LevelConfirmed(float co2Level) 
    {
        if (CO2LevelPanel != null) 
            CO2LevelPanel.SetActive(false); 
        if (PatientBelly != null) 
        {
            float targetScale = Mathf.Clamp(1f + co2Level / 100f, 1f, maxBellyScale);
            StartCoroutine(ExpandBellySmoothly(targetScale)); 
        } 
        // show laparoscope camera after 1s
        StartCoroutine(ShowLaparoscopeCameraPanelAfterDelay(1f));
    } 
    
    IEnumerator ExpandBellySmoothly(float targetScale) 
    {
        if (PatientBelly == null) yield break;
        Vector3 initialScale = PatientBelly.transform.localScale;
        Vector3 finalScale = new Vector3(targetScale, targetScale, targetScale);
        float elapsed = 0f; while (elapsed < bellyExpandSpeed)
        {
            PatientBelly.transform.localScale = Vector3.Lerp(initialScale, finalScale, elapsed / bellyExpandSpeed);
            elapsed += Time.deltaTime; yield return null; 
        } 
        PatientBelly.transform.localScale = finalScale;
    }

    // Laparo Camera Logic Code
    IEnumerator ShowLaparoscopeCameraPanelAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if(LaparoscopeCameraPanel != null)
        {
            LaparoscopeCameraPanel.SetActive(true);
            LaparoCameraStartTime = Time.time;
        }
    }

    public void CloseLaparoscopeCameraPanel()
    {
        LaparoscopeCameraPanel.SetActive(false);
    }
}