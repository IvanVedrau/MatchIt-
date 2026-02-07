using TMPro;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.UI;
using System.Collections.Generic;

public class LeaderboardUI : MonoBehaviour
{
    // UI elements for leaderboard display
    [SerializeField] private GameObject leaderboardEntryPrefab;
    [SerializeField] private Transform leaderboardContent;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button closeButton;
    [SerializeField] private int maxEntries = 10;
    [SerializeField] private TMP_Text currentPlayerRankText;
    [SerializeField] private TMP_Text currentPlayerScoreText;

    private CubeRotation backgroundCube;
    private bool isLoading = false;
    private bool leaderboardLoaded = false;

    private void Start()
    {
        // Set up close button listener
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ToggleLeaderboard);
        }

        // Find the background cube
        backgroundCube = FindObjectOfType<CubeRotation>();
        
        // Ensure the panel is initially hidden
        gameObject.SetActive(false);
    }

    // Toggle leaderboard visibility
    public void ToggleLeaderboard()
    {
        bool isNowActive = !gameObject.activeSelf;
        Debug.Log("Toggling leaderboard. Now active: " + isNowActive);
        gameObject.SetActive(isNowActive);

        if (backgroundCube != null)
        {
            backgroundCube.SetPaused(isNowActive);  // Pauses the cube when the leaderboard is active
        }

        if (isNowActive && !isLoading && !leaderboardLoaded)
        {
            LoadLeaderboard();  // Loads the leaderboard if it's being shown for the first time
        }
    }

    // Hide leaderboard panel
    public void HideLeaderboard()
    {
        gameObject.SetActive(false);

        if (backgroundCube != null)
        {
            backgroundCube.SetPaused(false);
        }

        ClearLeaderboardEntries();
    }

    // Clear entries in the next frame
    private System.Collections.IEnumerator ClearEntriesNextFrame()
    {
        yield return null; // wait 1 frame
        ClearLeaderboardEntries();
    }

    private void Awake()
    {
        backgroundCube = FindObjectOfType<CubeRotation>();
    }

    // Clear all leaderboard entries
    public void ClearLeaderboardEntries()
    {
        foreach (Transform child in leaderboardContent)
        {
            Destroy(child.gameObject);
        }

        leaderboardLoaded = false;
    }

    // Load and display leaderboard data
    public async void LoadLeaderboard()
    {
        if (isLoading) return;
        isLoading = true;

        if (titleText != null)
        {
            titleText.text = "Loading leaderboard...";
        }

        try
        {
            var leaderboard = await GetLeaderboard();
            DisplayLeaderboard(leaderboard);
            UpdateCurrentPlayerStats(leaderboard);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading leaderboard: {e.Message}");
            if (titleText != null)
            {
                titleText.text = "Error loading leaderboard";
            }
        }
        finally
        {
            isLoading = false;
        }
    }

    // Get leaderboard data from database
    private async Task<List<LeaderboardEntry>> GetLeaderboard()
    {
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogError("PlayerDataManager instance is null");
            return new List<LeaderboardEntry>();
        }

        try
        {
            var entries = await PlayerDataManager.Instance.GetAllPlayersDataAsync();
            
            // Sort entries by score in descending order
            return entries.OrderByDescending(entry => entry.score).ToList();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error getting leaderboard data: {e.Message}");
            return new List<LeaderboardEntry>();
        }
    }

    // Display leaderboard entries in UI
    private void DisplayLeaderboard(List<LeaderboardEntry> entries)
    {
        leaderboardLoaded = true;
        // Clear existing entries
        foreach (Transform child in leaderboardContent)
        {
            Destroy(child.gameObject);
        }

        // Create new entries
        int displayCount = Mathf.Min(entries.Count, maxEntries);
        for (int i = 0; i < displayCount; i++)
        {
            var entry = entries[i];
            var entryObject = Instantiate(leaderboardEntryPrefab, leaderboardContent);
            var entryUI = entryObject.GetComponent<LeaderboardEntryUI>();
            
            if (entryUI != null)
            {
                // Use email as name if available, otherwise use userId
                string displayName = !string.IsNullOrEmpty(entry.email) ? entry.email : entry.userId;
                entryUI.SetData(i + 1, displayName, entry.score);
                
                // Highlight current player's entry
                if (entry.userId == PlayerDataManager.Instance?.UserId)
                {
                    entryUI.HighlightEntry();
                }
            }
        }

        if (titleText != null)
        {
            titleText.text = "LEADERBOARD";
        }
    }

    // Update current player's rank and score display
    private void UpdateCurrentPlayerStats(List<LeaderboardEntry> entries)
    {
        if (currentPlayerRankText == null || currentPlayerScoreText == null || 
            PlayerDataManager.Instance == null || PlayerDataManager.Instance.CurrentPlayerData == null)
        {
            currentPlayerRankText.text = "Your Rank: Not Ranked";
            currentPlayerScoreText.text = "Your Score: 0";
            return;
        }

        string currentUserId = PlayerDataManager.Instance.UserId;
        var currentPlayerEntry = entries.FirstOrDefault(e => e.userId == currentUserId);
        
        if (currentPlayerEntry != null)
        {
            int rank = entries.FindIndex(e => e.userId == currentUserId) + 1;
            currentPlayerRankText.text = $"Your Rank: #{rank}";
            currentPlayerScoreText.text = $"Your Score: {currentPlayerEntry.score}";
        }
        else
        {
            currentPlayerRankText.text = "Your Rank: Not Ranked";
            currentPlayerScoreText.text = $"Your Score: {PlayerDataManager.Instance.CurrentPlayerData.score}";
        }
    }

    private void OnDisable()
    {
        // Clear entries when disabled
        ClearLeaderboardEntries();
    }
}