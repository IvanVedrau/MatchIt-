using Firebase.Database;
using UnityEngine;
using System.Threading.Tasks;
using System;
using Firebase.Auth;
using System.Collections.Generic;
using System.Linq;

public class PlayerDataManager : MonoBehaviour
{
    // Singleton instance for global access
    public static PlayerDataManager Instance { get; private set; }
    public PlayerData CurrentPlayerData { get; private set; }
    public string UserId => FirebaseAuth.DefaultInstance?.CurrentUser?.UserId;

    // Database reference and initialization tracking
    private DatabaseReference databaseReference;
    private bool isInitialized = false;
    private int initializationAttempts = 0;
    private const int MAX_INITIALIZATION_ATTEMPTS = 10;
    private const float RETRY_DELAY = 1f;
    private string lastUserId = null;

    private void Awake()
    {
        // Set up singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CurrentPlayerData = new PlayerData();
            Debug.Log("[PlayerDataManager] Instance initialized");
        }
        else
        {
            Debug.Log("[PlayerDataManager] Duplicate instance found, destroying");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log("[PlayerDataManager] Starting initialization");
        TryInitializeDatabase();
    }

    private void Update()
    {
        // Check for user changes
        string currentUserId = UserId;
        if (currentUserId != lastUserId)
        {
            Debug.Log($"[PlayerDataManager] User changed from {lastUserId} to {currentUserId}");
            lastUserId = currentUserId;
            if (currentUserId != null)
            {
                TryInitializeDatabase();
            }
        }
    }

    // Try to initialize database connection
    private void TryInitializeDatabase()
    {
        if (isInitialized && UserId == lastUserId) return;

        initializationAttempts++;

        if (FirebaseInitializer.Instance == null)
        {
            if (initializationAttempts < MAX_INITIALIZATION_ATTEMPTS)
            {
                Invoke("TryInitializeDatabase", RETRY_DELAY);
            }
            return;
        }

        if (!FirebaseInitializer.Instance.IsInitialized)
        {
            if (initializationAttempts < MAX_INITIALIZATION_ATTEMPTS)
            {
                Invoke("TryInitializeDatabase", RETRY_DELAY);
            }
            return;
        }

        if (!FirebaseInitializer.Instance.IsAuthenticated)
        {
            if (initializationAttempts < MAX_INITIALIZATION_ATTEMPTS)
            {
                Invoke("TryInitializeDatabase", RETRY_DELAY);
            }
            return;
        }

        InitializeDatabase();
    }

    // Initialize Firebase database connection
    private void InitializeDatabase()
    {
        try
        {
            if (FirebaseAuth.DefaultInstance == null) return;

            var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
            if (currentUser == null) return;

            string userId = currentUser.UserId;
            if (string.IsNullOrEmpty(userId)) return;

            var database = FirebaseDatabase.DefaultInstance;
            database.SetPersistenceEnabled(true);

            databaseReference = database.RootReference.Child("players").Child(userId);
            databaseReference.KeepSynced(true);

            isInitialized = true;
            initializationAttempts = 0;
            LoadPlayerData();
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerDataManager] Error initializing database: {e.Message}");
            if (initializationAttempts < MAX_INITIALIZATION_ATTEMPTS)
            {
                Invoke("TryInitializeDatabase", RETRY_DELAY);
            }
        }
    }

    // Load player data from database
    public async Task LoadPlayerData()
    {
        if (!isInitialized || databaseReference == null) return;

        try
        {
            var snapshot = await databaseReference.GetValueAsync();

            if (snapshot.Exists)
            {
                CurrentPlayerData = JsonUtility.FromJson<PlayerData>(snapshot.GetRawJsonValue());
                Debug.Log($"[PlayerDataManager] Loaded player data for user {UserId}: Score={CurrentPlayerData.score}, PlayTime={CurrentPlayerData.totalPlayTime}");
            }
            else
            {
                CurrentPlayerData = new PlayerData();
                await SavePlayerData();
                Debug.Log($"[PlayerDataManager] Created new player data for user {UserId}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerDataManager] Error loading player data: {e.Message}");
        }
    }

    // Save player data to database
    public async Task SavePlayerData()
    {
        if (!isInitialized || databaseReference == null || CurrentPlayerData == null) return;

        try
        {
            var data = new Dictionary<string, object>
            {
                { "userId", FirebaseAuth.DefaultInstance.CurrentUser.UserId },
                { "email", FirebaseAuth.DefaultInstance.CurrentUser.Email ?? "" },
                { "score", CurrentPlayerData.score },
                { "totalPlayTime", CurrentPlayerData.totalPlayTime },
                { "lastPlayed", CurrentPlayerData.lastPlayed }
            };

            var task = databaseReference.UpdateChildrenAsync(data);
            await task;

            if (task.IsFaulted)
            {
                throw task.Exception;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerDataManager] Error saving player data: {e.Message}");
            throw;
        }
    }

    // Update player's score
    public async Task UpdateScore(int newScore)
    {
        if (!isInitialized) return;

        try
        {
            CurrentPlayerData.score = Mathf.Max(CurrentPlayerData.score, newScore);
            await SavePlayerData();
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerDataManager] Error updating score: {e.Message}");
        }
    }

    // Update player's play time
    public async Task UpdatePlayTime(float additionalPlayTime)
    {
        if (!isInitialized) return;

        try
        {
            CurrentPlayerData.totalPlayTime += additionalPlayTime;
            CurrentPlayerData.lastPlayed = System.DateTime.Now.ToString("o");
            await SavePlayerData();
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerDataManager] Error updating play time: {e.Message}");
        }
    }

    // Update player's email
    public async Task UpdateEmail(string newEmail)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[PlayerDataManager] Cannot update email: Database not initialized");
            return;
        }

        try
        {
            Debug.Log($"[PlayerDataManager] Updating email to {newEmail}");
            CurrentPlayerData.email = newEmail;
            await SavePlayerData();
            Debug.Log("[PlayerDataManager] Email updated successfully");
        }
        catch (Exception e)
        {
            Debug.LogError($"[PlayerDataManager] Error updating email: {e.Message}");
        }
    }

    // Get all players' data for leaderboard
    public async Task<List<LeaderboardEntry>> GetAllPlayersDataAsync()
    {
        if (!isInitialized)
        {
            Debug.LogError("[PlayerDataManager] Cannot get all players data: Database not initialized");
            return new List<LeaderboardEntry>();
        }

        try
        {
            var database = FirebaseDatabase.DefaultInstance;
            var playersRef = database.RootReference.Child("players");

            var snapshot = await playersRef.GetValueAsync();

            if (!snapshot.Exists)
            {
                Debug.Log("[PlayerDataManager] No player data found in database");
                return new List<LeaderboardEntry>();
            }

            // Dictionary to track entries by userId to avoid duplicates
            Dictionary<string, LeaderboardEntry> entriesByUserId = new Dictionary<string, LeaderboardEntry>();

            foreach (var childSnapshot in snapshot.Children)
            {
                try
                {
                    var playerData = JsonUtility.FromJson<PlayerData>(childSnapshot.GetRawJsonValue());
                    if (playerData != null)
                    {
                        string userId = childSnapshot.Key;

                        // Check if we already have an entry for this user
                        if (entriesByUserId.TryGetValue(userId, out LeaderboardEntry existingEntry))
                        {
                            // Keep the entry with the higher score
                            if (playerData.score > existingEntry.score)
                            {
                                entriesByUserId[userId] = new LeaderboardEntry
                                {
                                    userId = userId,
                                    email = playerData.email,
                                    score = playerData.score,
                                    totalPlayTime = playerData.totalPlayTime,
                                    lastPlayed = playerData.lastPlayed
                                };
                            }
                        }
                        else
                        {
                            // Add new entry
                            entriesByUserId[userId] = new LeaderboardEntry
                            {
                                userId = userId,
                                email = playerData.email,
                                score = playerData.score,
                                totalPlayTime = playerData.totalPlayTime,
                                lastPlayed = playerData.lastPlayed
                            };
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[PlayerDataManager] Error parsing player data for {childSnapshot.Key}: {e.Message}");
                }
            }

            // Convert dictionary values to list
            return entriesByUserId.Values.ToList();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PlayerDataManager] Error getting all players data: {e.Message}");
            return new List<LeaderboardEntry>();
        }
    }

    // Reset manager state for new user
    public void ResetState()
    {
        Debug.Log("[PlayerDataManager] Resetting state for new user");
        isInitialized = false;
        initializationAttempts = 0;
        lastUserId = null;
        CurrentPlayerData = new PlayerData();
        databaseReference = null;
    }
}

// Player data structure
[System.Serializable]
public class PlayerData
{
    public int score = 0;
    public float totalPlayTime = 0f;
    public string lastPlayed = "";
    public string email = "";

    // Format total play time for display
    public string GetFormattedTotalPlayTime()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(totalPlayTime);
        if (timeSpan.TotalHours >= 1)
        {
            return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
        }
        else if (timeSpan.TotalMinutes >= 1)
        {
            return $"{(int)timeSpan.TotalMinutes}m {timeSpan.Seconds}s";
        }
        else
        {
            return $"{(int)timeSpan.TotalSeconds}s";
        }
    }

    // Format last played time for display
    public string GetFormattedLastPlayed()
    {
        if (string.IsNullOrEmpty(lastPlayed)) return "Never";
        if (System.DateTime.TryParse(lastPlayed, out System.DateTime lastPlayedTime))
        {
            var timeSpan = System.DateTime.Now - lastPlayedTime;
            if (timeSpan.TotalDays >= 1)
                return $"{(int)timeSpan.TotalDays}d ago";
            if (timeSpan.TotalHours >= 1)
                return $"{(int)timeSpan.TotalHours}h ago";
            if (timeSpan.TotalMinutes >= 1)
                return $"{(int)timeSpan.TotalMinutes}m ago";
            return "Just now";
        }
        return lastPlayed;
    }

    // Get detailed play time format
    public string GetDetailedPlayTime()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(totalPlayTime);
        return $"{(int)timeSpan.TotalHours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }
}