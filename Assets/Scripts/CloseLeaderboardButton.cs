using UnityEngine;
using UnityEngine.UI;

public class CloseLeaderboardButton : MonoBehaviour
{
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private bool playSound = true;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private float soundVolume = 1.0f;

    private AudioSource audioSource;
    private Button button;
    private LeaderboardUI leaderboardUI;

    private void Awake()
    {
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && playSound)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Get or add Button component
        button = GetComponent<Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }

        // Add click listener
        button.onClick.AddListener(OnButtonClick);

        // Find the leaderboard panel if not assigned
        if (leaderboardPanel == null)
        {
            leaderboardPanel = GameObject.Find("LeaderboardPanel");
        }

        // Cache the LeaderboardUI component
        if (leaderboardPanel != null)
        {
            leaderboardUI = leaderboardPanel.GetComponent<LeaderboardUI>();
        }
    }

    private void OnButtonClick()
    {
        // Play sound if enabled
        if (playSound && audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound, soundVolume);
        }

        // Hide leaderboard panel
        if (leaderboardPanel != null)
        {
            // Use the cached LeaderboardUI component if available
            if (leaderboardUI != null)
            {
                leaderboardUI.HideLeaderboard();
            }
            else
            {
                // Fallback to directly setting the panel inactive
                leaderboardPanel.SetActive(false);
                
                // Try to find and pause the background cube
                var cubeRotation = FindObjectOfType<CubeRotation>();
                if (cubeRotation != null)
                {
                    cubeRotation.SetPaused(false);
                }
            }
        }
        else
        {
            Debug.LogError("Leaderboard panel reference is missing. Please assign it in the Inspector.");
        }
    }



    private void OnDestroy()
    {
        // Remove listener when destroyed
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClick);
        }
    }
} 