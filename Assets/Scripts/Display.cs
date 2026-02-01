using UnityEngine;
using TMPro;
using System.Collections;

public class Display : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI targetText;

    public float refreshRate = 1f;

    private int displayedScore = -1;

    void Awake()
    {
        if (scoreText == null || targetText == null)
        {
            Debug.LogError("Composant TextMeshProUGUI manquant sur cet objet.");
        }
    }

    void Start()
    {
        if (GameplayManager.Instance != null)
        {
            StartCoroutine(RefreshScoreDisplay());
        }
    }

    IEnumerator RefreshScoreDisplay()
    {
        WaitForSeconds delay = new WaitForSeconds(refreshRate);

        while (true)
        {
            int currentScore = GameplayManager.Instance.GetScore();
            int currentTarget = GameplayManager.Instance.GetActiveTargetsCount();

            if (currentScore != displayedScore)
            {
                scoreText.text = $"SCORE: {currentScore:N0}";
                displayedScore = currentScore;
            }
            targetText.text = $"Target Count: {currentTarget:N0}";


            yield return delay;
        }
    }
}