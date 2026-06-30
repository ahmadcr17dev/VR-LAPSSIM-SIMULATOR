using UnityEngine;
using System.Collections;

public class SoakingPad : MonoBehaviour
{
    private Coroutine soakingRoutine;

    [Header("Pad Settings")]
    public float soakCapacity = 100f;       // Max blood the pad can absorb
    public float absorbRate = 10f;          // How fast the pad absorbs per second
    public BleedController bleedController; // assign in Inspector if possible
    public Renderer padRenderer;            // Renderer of the pad mesh
    public Color dryColor = Color.white;    // Default color
    public Color soakedColor = Color.red;   // Fully soaked color

    private float currentSoak = 0f;
    public bool isSoaking = false;
    public bool isFullySoaked = false;     // track final state

    void Start()
    {
        if (padRenderer == null)
            padRenderer = GetComponentInChildren<Renderer>();

        UpdatePadColor();
    }

    void Awake()
    {
        // If not assigned, try to find it in the scene dynamically
        if (bleedController == null)
        {
            // Example 1: Find it under the CutZone in the scene
            GameObject cutZone = GameObject.FindWithTag("CutZone");
            if (cutZone != null)
            {
                bleedController = cutZone.GetComponent<BleedController>();
            }

            if (bleedController == null)
            {
                Debug.LogError("BleedController not found! Assign it in Inspector.");
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("CutZone")) return;
        if (bleedController == null) return;
        if (isFullySoaked) return;

        if (!isSoaking)
        {
            isSoaking = true;
            StartCoroutine(StopBleedingSmooth());
            Debug.Log("Soaking started.");
        }
    }

    //private void OnTriggerExit(Collider other)
    //{
    //    if (!other.CompareTag("CutZone")) return;

    //    isSoaking = false;
    //}

    private void UpdatePadColor()
    {
        if (padRenderer == null) return;

        // Only clamp between 0 and 1
        float t = Mathf.Clamp01(currentSoak / soakCapacity);
        padRenderer.material.color = Color.Lerp(dryColor, soakedColor, t);
    }

    private IEnumerator StopBleedingSmooth()
    {
        float fadeSpeed = absorbRate;

        while (bleedController.HasBleeding())
        {
            float absorbed = bleedController.AbsorbAt(
                transform.position,
                fadeSpeed * Time.deltaTime
            );

            currentSoak += absorbed;
            UpdatePadColor();

            yield return null;
        }

        // Force stop emission completely
        bleedController.StopBleeding();

        // Wait until all particles die visually
        yield return new WaitWhile(() =>
            bleedController.bloodParticles != null &&
            bleedController.bloodParticles.particleCount > 0
        );

        isFullySoaked = true;
        isSoaking = false;

        // Fire success only once
        bleedController.operationSteps.OnSoakingPadSuccessful();

        Debug.Log("Blood soaked completely and smoothly.");
    }
}