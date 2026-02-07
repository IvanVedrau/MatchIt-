using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseTasks : Singleton<FirebaseTasks>
{
    // Firebase initialization tracking
    private bool isInitialized = false;
    private FirebaseAuth auth;

    // Initialize Firebase connection
    public async Task InitializeFirebase()
    {
        if (isInitialized) return;

        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyStatus == DependencyStatus.Available)
        {
            auth = FirebaseAuth.DefaultInstance;
            FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false); // Disable for simplicity
            isInitialized = true;
            Debug.Log("Firebase initialized successfully");
        }
        else
        {
            Debug.LogError($"Could not resolve Firebase dependencies: {dependencyStatus}");
        }
    }

    // Create new user account
    public async Task<FirebaseUser> CreateUser(string email, string password)
    {
        await WaitForInitialization();
        
        try
        {
            Debug.Log($"[FirebaseTasks] Creating user with email: {email}");
            AuthResult result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            
            if (result?.User == null)
            {
                Debug.LogError("[FirebaseTasks] User creation failed: Result or User is null");
                return null;
            }
            
            Debug.Log($"[FirebaseTasks] User created successfully: {result.User.UserId}");
            return result.User;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FirebaseTasks] Error creating user: {e.Message}");
            if (e.InnerException != null)
            {
                Debug.LogError($"[FirebaseTasks] Inner exception: {e.InnerException.Message}");
            }
            throw;
        }
    }

    // Sign in existing user
    public async Task<FirebaseUser> SignIn(string email, string password)
    {
        await WaitForInitialization();
        AuthResult result = await auth.SignInWithEmailAndPasswordAsync(email, password);
        return result?.User;
    }

    // Create initial player data in database
    public async Task CreatePlayerData(string userId, string email)
    {
        await WaitForInitialization();

        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("[FirebaseTasks] Cannot create player data: User ID is null or empty");
            throw new System.ArgumentException("User ID cannot be null or empty");
        }

        if (string.IsNullOrEmpty(email))
        {
            Debug.LogError("[FirebaseTasks] Cannot create player data: Email is null or empty");
            throw new System.ArgumentException("Email cannot be null or empty");
        }

        try
        {
            Debug.Log($"[FirebaseTasks] Checking if player data exists for user {userId}");

            var database = FirebaseDatabase.DefaultInstance;
            var reference = database.GetReference("players").Child(userId);

            var snapshot = await reference.GetValueAsync();

            // If player data already exists, skip creation
            if (snapshot.Exists)
            {
                Debug.Log("[FirebaseTasks] Player data already exists. Skipping creation.");
                return;
            }

            Debug.Log($"[FirebaseTasks] Creating player data for user {userId} with email {email}");

            var playerData = new Dictionary<string, object>
            {
                { "userId", userId },
                { "email", email },
                { "score", 0 },
                { "totalPlayTime", 0f },
                { "lastPlayed", System.DateTime.UtcNow.ToString("o") }
            };

            // Save new player data
            await reference.SetValueAsync(playerData);

            Debug.Log("[FirebaseTasks] Player data created successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FirebaseTasks] Error creating player data: {e.Message}\nStack trace: {e.StackTrace}");
            if (e.InnerException != null)
            {
                Debug.LogError($"[FirebaseTasks] Inner exception: {e.InnerException.Message}");
            }
            throw; // Re-throw the exception to allow caller to handle it
        }
    }

    // Wait for Firebase initialization to complete
    private async Task WaitForInitialization()
    {
        int attempts = 0;
        const int maxAttempts = 50; // 5 seconds total
        while (!isInitialized && attempts < maxAttempts)
        {
            await Task.Delay(100);
            attempts++;
        }
        
        if (!isInitialized)
        {
            Debug.LogError("[FirebaseTasks] Firebase initialization timeout");
            throw new System.TimeoutException("Firebase initialization timeout");
        }
    }
}