using UnityEngine;
using System.Collections;
using System.Threading.Tasks;

public class PlayTimeTracker : MonoBehaviour
{
    // Play time tracking variables
    private float playTime = 0f;
    private float lastSaveTime = 0f;
    private const float SAVE_INTERVAL = 30f; // Save every 30 seconds
    private bool isInitialized = false;
    private int initializationAttempts = 0;
    private const int MAX_INITIALIZATION_ATTEMPTS = 10;
    private const float RETRY_DELAY = 1f;
    private bool isSaving = false;

    void Start()
    {
        StartCoroutine(InitializeTracker());
    }

    // Initialize the play time tracker
    private IEnumerator InitializeTracker()
    {
        while (!isInitialized && initializationAttempts < MAX_INITIALIZATION_ATTEMPTS)
        {
            initializationAttempts++;

            if (FirebaseInitializer.Instance == null)
            {
                yield return new WaitForSeconds(RETRY_DELAY);
                continue;
            }

            if (!FirebaseInitializer.Instance.IsInitialized)
            {
                yield return new WaitForSeconds(RETRY_DELAY);
                continue;
            }

            if (!FirebaseInitializer.Instance.IsAuthenticated)
            {
                yield return new WaitForSeconds(RETRY_DELAY);
                continue;
            }

            if (PlayerDataManager.Instance == null)
            {
                yield return new WaitForSeconds(RETRY_DELAY);
                continue;
            }

            isInitialized = true;
            break;
        }
    }

    void Update()
    {
        if (!isInitialized) return;

        playTime += Time.deltaTime;

        // Check if it's time to save
        if (Time.time - lastSaveTime >= SAVE_INTERVAL && !isSaving)
        {
            SavePlayTime();
            lastSaveTime = Time.time;
        }
    }

    // Save accumulated play time to database
    private async void SavePlayTime()
    {
        if (!isInitialized || PlayerDataManager.Instance == null || isSaving) return;

        try
        {
            isSaving = true;
            await PlayerDataManager.Instance.UpdatePlayTime(playTime);
            playTime = 0f; // Reset the counter after successful save
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PlayTimeTracker] Error saving play time: {e.Message}");
        }
        finally
        {
            isSaving = false;
        }
    }

    // Handle application pause
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && isInitialized && !isSaving)
        {
            SavePlayTime();
        }
    }

    // Handle application quit
    void OnApplicationQuit()
    {
        if (isInitialized && !isSaving)
        {
            SavePlayTime();
        }
    }
}