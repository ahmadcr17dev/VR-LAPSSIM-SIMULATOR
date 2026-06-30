using UnityEngine;

public class BleedController : MonoBehaviour
{
    [Header("References")]
    public ParticleSystem bloodParticles; // Will be assigned dynamically from KnifeCut
    public GameObject cutQuad;            // Quad with cut texture (hidden until bleeding stops)

    [Header("Emission / Absorption")]
    public float minEmissionRate = 0f;
    public float maxEmissionRate = 30f;

    private ParticleSystem.EmissionModule emission;
    private bool cutRevealed = false;

    public OperationSteps operationSteps;

    void Start()
    {
        // Ensure cut mark is hidden at start
        if (cutQuad != null)
            cutQuad.SetActive(false);
    }

    void Awake()
    {
        if (bloodParticles != null)
        {
            emission = bloodParticles.emission;
            emission.rateOverTime = maxEmissionRate;
        }
    }

    // 🔴 Called by KnifeCut when new blood prefab is spawned
    public void SetBloodParticles(ParticleSystem ps)
    {
        bloodParticles = ps;
        if (bloodParticles != null)
        {
            emission = bloodParticles.emission;
            emission.rateOverTime = maxEmissionRate;
            bloodParticles.Play();
        }
    }

    // Absorb some blood (called by SoakingPad)
    public float AbsorbAt(Vector3 point, float amount)
    {
        if (bloodParticles == null) return 0f;

        float currentRate = emission.rateOverTime.constant;
        float newRate = Mathf.Max(minEmissionRate, currentRate - amount);
        float absorbed = currentRate - newRate;

        emission.rateOverTime = new ParticleSystem.MinMaxCurve(newRate);

        // 👉 If bleeding is completely stopped, show cut mark
        if (newRate <= minEmissionRate && !cutRevealed)
        {
            ShowCutMark();
        }

        return absorbed;
    }

    // Force stop bleeding (manual)
    public void StopBleeding()
    {
        if (bloodParticles == null) return;

        var emission = bloodParticles.emission;
        emission.rateOverTime = 0;

        var main = bloodParticles.main;
        main.startLifetime = 0.3f; // particles die quickly

        bloodParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        //bloodParticles.Clear();

        Debug.Log("Bleeding stopped smoothly.");
    }

    public bool HasBleeding()
    {
        if (bloodParticles == null) return false;
        return emission.rateOverTime.constant > minEmissionRate + 0.01f;
    }

    // 👉 Show cut mark after soaking finishes
    void ShowCutMark()
    {
        if (cutQuad != null)
            cutQuad.SetActive(true);

        cutRevealed = true;
    }

    public void ResetBlood()
    {
        if (bloodParticles != null)
        {
            bloodParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            emission = bloodParticles.emission;
            emission.rateOverTime = minEmissionRate;
        }

        cutRevealed = false;

        if (cutQuad != null)
            cutQuad.SetActive(false);
    }

    public void ResumeBlood()
    {
        if (bloodParticles != null)
        {
            emission = bloodParticles.emission;
            emission.rateOverTime = maxEmissionRate;
            bloodParticles.Play();
        }
    }
}