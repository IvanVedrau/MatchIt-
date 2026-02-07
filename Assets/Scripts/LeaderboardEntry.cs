using System;

[Serializable]
public class LeaderboardEntry
{
    // Player identification
    public string userId;
    public string email;
    
    // Player statistics
    public int score;
    public float totalPlayTime;
    public string lastPlayed;
} 