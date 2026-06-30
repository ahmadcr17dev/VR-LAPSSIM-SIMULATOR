using UnityEngine;

public class KnifeCut : MonoBehaviour
{
    private bool CanCut = true;
    public OperationSteps operationSteps;

    [Header("Particle Prefab")]
    public ParticleSystem bloodParticlePrefab;

    [Header("Cut Settings")]
    public string cutTag = "CutZone";

    private ParticleSystem activeBloodParticles;

    private void OnTriggerEnter(Collider other)
    {
        if (!CanCut) return; // 🔹 only allow if re-armed
        if (activeBloodParticles != null) return;

        if (other.CompareTag(cutTag))
        {
            Debug.Log("Cut performed!");
            CanCut = false; // disarm until next retry

            operationSteps.OnKnifeCutSuccessful();

            if (bloodParticlePrefab != null)
            {
                Vector3 spawnPos = other.transform.position;
                activeBloodParticles = Instantiate(bloodParticlePrefab, spawnPos, other.transform.rotation);
                activeBloodParticles.Play();

                if (other.TryGetComponent<BleedController>(out var bc))
                {
                    bc.SetBloodParticles(activeBloodParticles);
                }
            }
        }
    }

    // Optional: external stop
    public void StopBleeding()
    {
        if (activeBloodParticles != null)
        {
            activeBloodParticles.Stop();
            Destroy(activeBloodParticles.gameObject, activeBloodParticles.main.startLifetime.constantMax);
            activeBloodParticles = null;
        }
    }

    public void ResetKnifeCut()
    {
        StopBleeding();
        CanCut = true;
    }
}