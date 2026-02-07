using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LeaderboardEntryUI : MonoBehaviour
{
    // UI elements for displaying a single leaderboard entry
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Image backgroundImage;
    
    // Colors for highlighting current player's entry
    [SerializeField] private Color highlightColor = new Color(1f, 0.92f, 0.016f, 0.3f);
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.1f);

    private void Awake()
    {
        // Get background image component if not set
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }
    }

    // Set the data for this leaderboard entry
    public void SetData(int rank, string playerName, int score)
    {
        rankText.text = $"#{rank}";
        nameText.text = playerName;
        scoreText.text = score.ToString();
    }

    // Highlight this entry (used for current player's entry)
    public void HighlightEntry()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = highlightColor;
        }
    }
} 