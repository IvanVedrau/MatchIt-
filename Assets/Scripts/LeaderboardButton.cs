using UnityEngine;
using UnityEngine.UI;

public class LeaderboardButton : MonoBehaviour
{
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private bool playSound = true;
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private float soundVolume = 1.0f;

    private AudioSource audioSource;
    private Button button;

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
    }

    private void OnButtonClick()
    {
        // Play sound if enabled
        if (playSound && audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound, soundVolume);
        }

        // Toggle the leaderboard panel visibility
        if (leaderboardPanel != null)
        {
            bool isActive = leaderboardPanel.activeSelf;
            leaderboardPanel.SetActive(!isActive);  // Toggle the visibility

            // If the leaderboard is being shown, load the leaderboard data
            if (!isActive)
            {
                var leaderboardUI = leaderboardPanel.GetComponent<LeaderboardUI>();
                if (leaderboardUI != null)
                {
                    leaderboardUI.LoadLeaderboard();
                }
                else
                {
                    Debug.LogWarning("LeaderboardUI component not found on the leaderboard panel.");
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
