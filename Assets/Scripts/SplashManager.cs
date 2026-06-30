using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashManager : MonoBehaviour
{
    [Header("UI Elements")]
    public CanvasGroup blackBG;
    public CanvasGroup unityLogo;
    public CanvasGroup projectName;
    public CanvasGroup supervisedBy;
    public CanvasGroup developedBy;

    [Header("Settings")]
    public float fadeDuration = 1f;
    public float displayTime = 2f;
    public string mainSceneName = "Hospital";

    [Header("Audio Source")]
    private AudioSource IntroAudio;

    private bool isSkipped = false;

    void Awake()
    {
        IntroAudio = GetComponent<AudioSource>();
    }

    void Start()
    {
        blackBG.alpha = 1;
        StartCoroutine(PlaySplashSequence());
    }

    void OnEnable()
    {
        if (IntroAudio != null && !IntroAudio.isPlaying)
            IntroAudio.Play();
    }

    void OnDisable()
    {
        if (IntroAudio != null && IntroAudio.isPlaying)
            IntroAudio.Stop();
    }

    IEnumerator PlaySplashSequence()
    {
        yield return StartCoroutine(FadeIn(unityLogo));
        yield return new WaitForSeconds(displayTime);
        yield return StartCoroutine(FadeOut(unityLogo));

        yield return StartCoroutine(FadeIn(projectName));
        yield return new WaitForSeconds(displayTime);
        yield return StartCoroutine(FadeOut(projectName));

        yield return StartCoroutine(FadeIn(supervisedBy));
        yield return new WaitForSeconds(displayTime);
        yield return StartCoroutine(FadeOut(supervisedBy));

        yield return StartCoroutine(FadeIn(developedBy));
        yield return new WaitForSeconds(displayTime);
        yield return StartCoroutine(FadeOut(developedBy));

        yield return StartCoroutine(FadeOut(blackBG));

        LoadMainScene();
    }

    IEnumerator FadeIn(CanvasGroup cg)
    {
        float elapsed = 0f;
        cg.alpha = 0f;

        while (elapsed < fadeDuration)
        {
            cg.alpha = Mathf.Lerp(0, 1, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cg.alpha = 1;
    }

    IEnumerator FadeOut(CanvasGroup cg)
    {
        float elapsed = 0f;
        cg.alpha = 1f;

        while (elapsed < fadeDuration)
        {
            cg.alpha = Mathf.Lerp(1, 0, elapsed / fadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cg.alpha = 0;
    }

    // -------------------------------------------------
    // SKIP INTRO BUTTON
    // -------------------------------------------------
    public void SkipIntroPanel()
    {
        if (isSkipped) return;
        isSkipped = true;

        StopAllCoroutines();

        if (IntroAudio != null)
            IntroAudio.Stop();

        StartCoroutine(SkipAndLoad());
    }

    IEnumerator SkipAndLoad()
    {
        yield return StartCoroutine(FadeOut(blackBG));
        LoadMainScene();
    }

    void LoadMainScene()
    {
        SceneManager.LoadScene(mainSceneName);
    }
}
