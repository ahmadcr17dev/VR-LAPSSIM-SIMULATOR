using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class AnesthesiaManager : MonoBehaviour
{
    [SerializeField]
    public static bool AnesthesiaStarted = false;

    [Header("UI Elements")]
    public GameObject AnesthesiaPanel;
    public GameObject GameEndPanel;
    public GameObject HamburgerMenuIcon;
    public GameObject AnesthesiaTimer;

    //public TMP_InputField anesthesiaInputField; // OLD (kept)
    public TMP_Dropdown anesthesiaDropdown;     // NEW (1–25 dropdown)

    public TextMeshProUGUI anesthesiaTimerText;

    [Header("Settings")]
    public bool pauseOnComplete = true;
    private float anesthesiaTime = 0f;
    private bool anesthesiaActive = false;
    public static bool GameEnded = false;

    [Header("Menu Buttons")]
    public Button PausePlayButton;
    public Button TeleportToOperationRoom;
    public Button StartProcedure;
    public Button MovementInstructionButton;
    public Button ProjectInfoButton;
    public Button TeamInfoButton;
    public Button SurgeryGuidelinessButton;
    public Button CloseMenuButton;

    [Header("System References")]
    public MenuActions menuActions;

    [Header("Scoring Settings")]
    private float idealMin = 10f;
    private float idealMax = 20f;
    private float perfectTime = 15f;
    private int minScore = 1;
    private int maxScore = 5;
    private int anesthesiaScore = 0;

    private float enteredAnesthesiaMinutes = 0f;

    void Start()
    {
        if (anesthesiaTimerText != null && AnesthesiaTimer != null)
        {
            anesthesiaTimerText.gameObject.SetActive(false);
            AnesthesiaTimer.SetActive(false);
        }

        if (GameEndPanel != null)
            GameEndPanel.SetActive(false);

        if (ScoreManager.Instance != null && ScoreManager.Instance.ScorePanel != null)
            ScoreManager.Instance.ScorePanel.SetActive(false);
    }

    void Update()
    {
        if (!anesthesiaActive || GameEnded)
            return;

        if (anesthesiaTime > 0f)
        {
            anesthesiaTime -= Time.deltaTime;

            int minutes = Mathf.FloorToInt(anesthesiaTime / 60f);
            int seconds = Mathf.FloorToInt(anesthesiaTime % 60f);
            anesthesiaTimerText.text = $"{minutes:00}:{seconds:00}";
        }
        else
        {
            EndGame();
        }
    }

    // -------------------------------------------------
    // START ANESTHESIA (UPDATED FOR DROPDOWN)
    // -------------------------------------------------
    public void StartAnesthesia()
    {
        float minutes = 0f;
        bool validInput = false;

        // 🔹 NEW: Prefer dropdown if assigned
        if (anesthesiaDropdown != null && anesthesiaDropdown.options.Count > 0)
        {
            // Dropdown index 0 = "1", so +1
            minutes = anesthesiaDropdown.value + 1;
            validInput = true;
        }
        // 🔹 OLD: Fallback to input field (kept)
        //else if (anesthesiaInputField != null && float.TryParse(anesthesiaInputField.text, out float inputMinutes))
        //{
        //    minutes = inputMinutes;
        //    validInput = true;
        //}

        if (!validInput)
        {
            Debug.LogWarning("Invalid anesthesia time input");
            return;
        }

        // --------------------------
        // Existing logic (UNCHANGED)
        // --------------------------
        anesthesiaTime = minutes * 60f;
        anesthesiaActive = true;
        GameEnded = false;
        AnesthesiaStarted = true;

        enteredAnesthesiaMinutes = minutes;

        if (anesthesiaTimerText != null)
        {
            anesthesiaTimerText.gameObject.SetActive(true);
            AnesthesiaTimer.SetActive(true);
        }

        if (TeleportToOperationRoom != null)
            TeleportToOperationRoom.interactable = false;

        if (StartProcedure != null)
            StartProcedure.interactable = false;

        if (AnesthesiaPanel != null)
            AnesthesiaPanel.SetActive(false);

        anesthesiaScore = CalculateSmoothScore(enteredAnesthesiaMinutes);

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddPoint(anesthesiaScore);

            if (ScoreManager.Instance.ScorePanel != null)
                ScoreManager.Instance.ScorePanel.SetActive(true);
        }

        Debug.Log($"Anesthesia started for {minutes} minutes");
    }

    // -------------------------------------------------
    // END GAME
    // -------------------------------------------------
    void EndGame()
    {
        anesthesiaActive = false;
        GameEnded = true;

        if (anesthesiaTimerText != null)
            anesthesiaTimerText.text = "00:00";

        if (TeleportToOperationRoom != null)
            TeleportToOperationRoom.interactable = false;
        if (StartProcedure != null)
            StartProcedure.interactable = false;

        PausePlayButton.interactable = false;
        MovementInstructionButton.interactable = false;
        ProjectInfoButton.interactable = false;
        TeamInfoButton.interactable = false;
        SurgeryGuidelinessButton.interactable = false;
        CloseMenuButton.interactable = false;

        if (HamburgerMenuIcon != null)
            HamburgerMenuIcon.SetActive(false);

        if (GameEndPanel != null)
            GameEndPanel.SetActive(true);

        Debug.Log($"Game Ended: Anesthesia expired | Score: {anesthesiaScore}");
    }

    int CalculateSmoothScore(float minutes)
    {
        if (minutes < idealMin)
            return Mathf.RoundToInt(Mathf.Lerp(minScore, maxScore - 2, minutes / idealMin));
        else if (minutes >= idealMin && minutes <= idealMax)
        {
            float t = Mathf.Abs(minutes - perfectTime) / (idealMax - perfectTime);
            return Mathf.RoundToInt(Mathf.Lerp(maxScore, maxScore - 2, t));
        }
        else
        {
            float t = (minutes - idealMax) / idealMax;
            return Mathf.RoundToInt(Mathf.Lerp(maxScore - 2, minScore, Mathf.Clamp01(t)));
        }
    }

    public void ResetAnesthesia()
    {
        AnesthesiaStarted = false;
        GameEnded = false;
        PlayerPrefs.SetInt("SpawnInOperationRoom", 1);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void RetryGame()
    {
        Time.timeScale = 1f;
        GameEnded = false;
        anesthesiaScore = 0;
        enteredAnesthesiaMinutes = 0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        ScoreManager.Instance.ResetScore();
    }

    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}