using UnityEngine;
using System.Collections;

public class AutoHideAfterVoice : MonoBehaviour
{
    public float extraDelay = 1f;   // 1 seconds after voice ends
    private AudioSource audioSource;

    void OnEnable()
    {
        audioSource = GetComponent<AudioSource>();

        if (audioSource != null && audioSource.clip != null)
        {
            float totalTime = audioSource.clip.length + extraDelay;
            StartCoroutine(HideAfterDelay(totalTime));
        }
        else
        {
            // Safety fallback
            StartCoroutine(HideAfterDelay(extraDelay));
        }
    }

    IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        gameObject.SetActive(false);
    }
}
