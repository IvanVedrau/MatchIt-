using Firebase;
using Firebase.Auth;
using UnityEngine;
using System.Threading.Tasks;

public class FirebaseInitializer : MonoBehaviour
{
    // Singleton instance for global access
    public static FirebaseInitializer Instance { get; private set; }
    public bool IsInitialized { get; private set; }
    public bool IsAuthenticated { get; private set; }

    // Initialization tracking variables
    private bool isInitializing = false;
    private int initializationAttempts = 0;
    private const int MAX_INITIALIZATION_ATTEMPTS = 10;
    private const float RETRY_DELAY = 1f;

    private void Awake()
    {
        // Set up singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[FirebaseInitializer] Instance set");
        }
        else
        {
            Debug.Log("[FirebaseInitializer] Duplicate instance found, destroying");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log("[FirebaseInitializer] Starting initialization");
        InitializeFirebase();
    }

    // Main Firebase initialization method
    private async void InitializeFirebase()
    {
        if (isInitializing)
        {
            Debug.Log("[FirebaseInitializer] Already initializing, skipping");
            return;
        }

        isInitializing = true;
        initializationAttempts++;

        try
        {
            Debug.Log($"[FirebaseInitializer] Attempting to initialize Firebase (Attempt {initializationAttempts}/{MAX_INITIALIZATION_ATTEMPTS})");

            // Check if Firebase is already initialized
            if (FirebaseApp.DefaultInstance != null)
            {
                Debug.Log("[FirebaseInitializer] Firebase already initialized");
                IsInitialized = true;
                CheckAuthentication();
                return;
            }

            // Check and fix Firebase dependencies
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

            if (dependencyStatus == DependencyStatus.Available)
            {
                Debug.Log("[FirebaseInitializer] Firebase initialized successfully");
                IsInitialized = true;
                CheckAuthentication();
            }
            else
            {
                Debug.LogError($"[FirebaseInitializer] Could not resolve Firebase dependencies: {dependencyStatus}");
                IsInitialized = false;
                RetryInitialization();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FirebaseInitializer] Error initializing Firebase: {e.Message}");
            IsInitialized = false;
            RetryInitialization();
        }
        finally
        {
            isInitializing = false;
        }
    }

    // Check if user is already authenticated
    private void CheckAuthentication()
    {
        try
        {
            if (FirebaseAuth.DefaultInstance.CurrentUser != null)
            {
                Debug.Log($"[FirebaseInitializer] User already signed in: {FirebaseAuth.DefaultInstance.CurrentUser.UserId}");
                IsAuthenticated = true;
            }
            else
            {
                Debug.Log("[FirebaseInitializer] No user is signed in");
                IsAuthenticated = false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FirebaseInitializer] Error checking authentication: {e.Message}");
            IsAuthenticated = false;
        }
    }

    // Retry initialization if failed
    private void RetryInitialization()
    {
        if (initializationAttempts < MAX_INITIALIZATION_ATTEMPTS)
        {
            Debug.Log($"[FirebaseInitializer] Retrying initialization in {RETRY_DELAY} seconds...");
            Invoke("InitializeFirebase", RETRY_DELAY);
        }
        else
        {
            Debug.LogError("[FirebaseInitializer] Failed to initialize after maximum attempts");
        }
    }
}
