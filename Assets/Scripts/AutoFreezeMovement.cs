using UnityEngine;

public class AutoFreezeMovement : MonoBehaviour
{
    [Header("Movement Control")]
    public MonoBehaviour[] scriptsToDisable; // XR locomotion / movement scripts

    private float previousTimeScale = 1f;
    void OnEnable()
    {
        // Save current time scale (in case it is not 1)
        previousTimeScale = Time.timeScale;

        FreezeMovement(true);
        Time.timeScale = 0f;   // Freeze game time
    }

    void OnDisable()
    {
        FreezeMovement(false);
        Time.timeScale = previousTimeScale; // Restore time
    }

    void FreezeMovement(bool freeze)
    {
        if (scriptsToDisable != null)
        {
            foreach (MonoBehaviour script in scriptsToDisable)
            {
                if (script != null)
                    script.enabled = !freeze;
            }
        }
    }
}