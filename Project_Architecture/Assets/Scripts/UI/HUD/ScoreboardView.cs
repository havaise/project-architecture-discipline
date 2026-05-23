using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class ScoreboardView : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private string scoreFormat = "Score: {0}";
    [SerializeField] private TMP_Text killsText;
    [SerializeField] private string killsFormat = "Kills: {0}";

    public void Render(int score, int kills)
    {
        if (scoreText != null)
        {
            scoreText.text = string.Format(scoreFormat, Mathf.Max(0, score));
        }

        if (killsText != null)
        {
            killsText.text = string.Format(killsFormat, Mathf.Max(0, kills));
        }
    }
}
