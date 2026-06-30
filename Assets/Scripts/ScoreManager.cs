using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("UI")]
    public GameObject ScorePanel;
    public TextMeshProUGUI scoreText;

    private int score = 0;
    private bool procedureStarted = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        if (ScorePanel != null)
            ScorePanel.SetActive(false);

        UpdateUI();
    }

    // ✅ CALL THIS WHEN PROCEDURE STARTS
    public void OnProcedureStarted()
    {
        procedureStarted = true;

        if (ScorePanel != null)
            ScorePanel.SetActive(true);

        UpdateUI();
        Debug.Log("Procedure started → Score panel enabled");
    }

    // ADD POINTS
    public void AddPoint(int points)
    {
        if (!procedureStarted) return;

        score += points;
        UpdateUI();
    }

    public int GetTotalScore()
    {
        return score;
    }

    // REMOVE POINTS (used when retrying a step)
    public void RemovePoint(int points)
    {
        if (!procedureStarted) return;

        score -= points;
        if (score < 0) score = 0;

        UpdateUI();
    }

    // FULL RESET (used when restarting whole procedure)
    public void ResetScore()
    {
        score = 0;
        procedureStarted = false;

        if (ScorePanel != null)
            ScorePanel.SetActive(false);

        UpdateUI();
        Debug.Log("Score reset");
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = score.ToString();
        else
            Debug.LogError("ScoreText is NOT assigned in Inspector!");
    }
}