using UnityEngine;
using System.Collections;

public class FirebaseSetupChecker : MonoBehaviour
{
    // Setup tracking variables
    private bool isSetupComplete = false;
    private int setupAttempts = 0;
    private const int MAX_SETUP_ATTEMPTS = 10;

    private void Awake()
    {
        Debug.Log("FirebaseSetupChecker: Starting Firebase setup check");
        StartCoroutine(CheckFirebaseSetup());
    }

    // Main setup checking coroutine
    private IEnumerator CheckFirebaseSetup()
    {
        // Wait a frame to ensure all Awake methods have been called
        yield return null;

        setupAttempts++;
        Debug.Log($"FirebaseSetupChecker: Setup attempt {setupAttempts}/{MAX_SETUP_ATTEMPTS}");

        // Check if FirebaseInitializer exists
        if (FirebaseInitializer.Instance == null)
        {
            Debug.LogWarning("FirebaseSetupChecker: FirebaseInitializer.Instance is null. Looking for FirebaseInitializer in scene...");
            
            // Try to find FirebaseInitializer in the scene
            FirebaseInitializer[] initializers = FindObjectsOfType<FirebaseInitializer>();
            if (initializers.Length > 0)
            {
                Debug.Log($"FirebaseSetupChecker: Found {initializers.Length} FirebaseInitializer(s) in scene");
            }
            else
            {
                Debug.LogError("FirebaseSetupChecker: No FirebaseInitializer found in scene. Creating one...");
                
                // Create a new GameObject with FirebaseInitializer
                GameObject firebaseInitializerObj = new GameObject("FirebaseInitializer");
                firebaseInitializerObj.AddComponent<FirebaseInitializer>();
                DontDestroyOnLoad(firebaseInitializerObj);
                
                Debug.Log("FirebaseSetupChecker: Created new FirebaseInitializer GameObject");
            }
        }
        else
        {
            Debug.Log("FirebaseSetupChecker: FirebaseInitializer.Instance is valid");
        }

        // Check if PlayerDataManager exists
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogWarning("FirebaseSetupChecker: PlayerDataManager.Instance is null. Looking for PlayerDataManager in scene...");
            
            // Try to find PlayerDataManager in the scene
            PlayerDataManager[] managers = FindObjectsOfType<PlayerDataManager>();
            if (managers.Length > 0)
            {
                Debug.Log($"FirebaseSetupChecker: Found {managers.Length} PlayerDataManager(s) in scene");
            }
            else
            {
                Debug.LogError("FirebaseSetupChecker: No PlayerDataManager found in scene. Creating one...");
                
                // Create a new GameObject with PlayerDataManager
                GameObject playerDataManagerObj = new GameObject("PlayerDataManager");
                playerDataManagerObj.AddComponent<PlayerDataManager>();
                DontDestroyOnLoad(playerDataManagerObj);
                
                Debug.Log("FirebaseSetupChecker: Created new PlayerDataManager GameObject");
            }
        }
        else
        {
            Debug.Log("FirebaseSetupChecker: PlayerDataManager.Instance is valid");
        }

        // Wait a bit to ensure Firebase has time to initialize
        yield return new WaitForSeconds(2f);

        // Check Firebase initialization status
        if (FirebaseInitializer.Instance != null)
        {
            Debug.Log($"FirebaseSetupChecker: Firebase initialization status - IsInitialized: {FirebaseInitializer.Instance.IsInitialized}, IsAuthenticated: {FirebaseInitializer.Instance.IsAuthenticated}");
            
            if (FirebaseInitializer.Instance.IsInitialized && FirebaseInitializer.Instance.IsAuthenticated)
            {
                isSetupComplete = true;
                Debug.Log("FirebaseSetupChecker: Firebase setup completed successfully");
            }
            else if (setupAttempts < MAX_SETUP_ATTEMPTS)
            {
                Debug.LogWarning($"FirebaseSetupChecker: Firebase not fully initialized yet. Retrying in 2 seconds...");
                StartCoroutine(CheckFirebaseSetup());
            }
            else
            {
                Debug.LogError("FirebaseSetupChecker: Failed to initialize Firebase after maximum attempts");
            }
        }
        else
        {
            Debug.LogError("FirebaseSetupChecker: FirebaseInitializer.Instance is still null after waiting");
            
            if (setupAttempts < MAX_SETUP_ATTEMPTS)
            {
                Debug.LogWarning($"FirebaseSetupChecker: Retrying setup in 2 seconds...");
                StartCoroutine(CheckFirebaseSetup());
            }
        }
    }
} 