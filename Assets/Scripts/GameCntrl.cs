using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GameCntrl : MonoBehaviour
{
    // Game UI elements
    public GameObject LosePanel;
    public GameObject colBlock;
    public Vector3[] positions;
    private GameObject block;
    private GameObject[] blocks = new GameObject[4];

    // Game state variables
    private int rand, count;
    private float rCol, gCol, bCol;
    public TMP_Text score;
    private static Color aColor;

    [HideInInspector]
    public bool next, lose;

    // Player data management
    private PlayerDataManager playerDataManager;
    private float sessionStartTime;

    void Start()
    {
        // Try to get PlayerDataManager instance
        playerDataManager = PlayerDataManager.Instance;
        
        // If PlayerDataManager is not available, try to find it in the scene
        if (playerDataManager == null)
        {
            Debug.LogWarning("PlayerDataManager.Instance is null. Looking for PlayerDataManager in scene...");
            PlayerDataManager[] managers = FindObjectsOfType<PlayerDataManager>();
            if (managers.Length > 0)
            {
                playerDataManager = managers[0];
                Debug.Log($"Found PlayerDataManager in scene: {playerDataManager.gameObject.name}");
            }
            else
            {
                Debug.LogError("No PlayerDataManager found in scene. Firebase functionality will not work.");
            }
        }
        
        sessionStartTime = Time.time;
        
        // Initialize game state
        count = 0;
        next = false;
        lose = false;
        rand = Random.Range(0, positions.Length);
        
        // Create initial blocks
        for (int i = 0; i < positions.Length; i++)
        {
            blocks[i] = Instantiate(colBlock, positions[i], Quaternion.identity) as GameObject;
            if (rand == i)
                block = blocks[i];
        }
        block.GetComponent<RandCol>().right = true;
    }

    void Update()
    {
        // Check game state
        if (lose)
            playerLose();
        if (next && !lose)
            nextColors();
    }

    // Generate next set of colors
    void nextColors()
    {
        if (PlayerPrefs.GetString("Sounds") != "No")
            GetComponent<AudioSource>().Play();

        count++;
        score.text = count.ToString();
        aColor = new Vector4(Random.Range(0.1f, 1f), Random.Range(0.1f, 1f), Random.Range(0.1f, 1f), 1);
        GetComponent<Renderer>().material.color = aColor;
        next = false;

        // Adjust color difficulty based on score
        if (count < 3)
        {
            rCol = 0.2f;
            gCol = 0.2f;
            bCol = 0.2f;
        }
        else if (count >= 3 && count < 5)
        {
            rCol = 0.1f;
            gCol = 0.1f;
            bCol = 0f;
        }
        else if (count >= 5)
        {
            rCol = 0f;
            gCol = 0f;
            bCol = 0.05f;
        }

        // Generate new colors for blocks
        rand = Random.Range(0, positions.Length);
        for (int i = 0; i < positions.Length; i++)
        {
            if (i == rand)
                blocks[i].GetComponent<Renderer>().material.color = aColor;
            else
            {
                float r = aColor.r + Random.Range(0.1f, rCol) > 1f ? 1f : aColor.r + Random.Range(0.1f, rCol);
                float g = aColor.g + Random.Range(0.1f, gCol) > 1f ? 1f : aColor.g + Random.Range(0.1f, gCol);
                float b = aColor.b + Random.Range(0.1f, bCol) > 1f ? 1f : aColor.b + Random.Range(0.1f, bCol);
                blocks[i].GetComponent<Renderer>().material.color = new Vector4(r, g, b, aColor.a);
            }
        }
    }

    // Handle player loss
    async void playerLose()
    {
        Debug.Log($"Player lost with score: {count}");
        
        // Update local high score
        if (PlayerPrefs.GetInt("Score") < count)
        {
            PlayerPrefs.SetInt("Score", count);
        }
        
        // Update Firebase score
        if (playerDataManager == null)
        {
            Debug.LogError("GameCntrl: PlayerDataManager is null");
            LosePanel.SetActive(true);
            return;
        }

        if (playerDataManager.CurrentPlayerData == null)
        {
            Debug.LogError("GameCntrl: CurrentPlayerData is null");
            LosePanel.SetActive(true);
            return;
        }

        Debug.Log("Updating score in Firebase...");
        try 
        {
            await playerDataManager.UpdateScore(count);
            Debug.Log("Score updated successfully in Firebase");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating Firebase data: {e.Message}");
        }

        // Show lose panel
        LosePanel.SetActive(true);

        if (PlayerPrefs.GetString("Sounds") == "No")
            LosePanel.GetComponent<AudioSource>().mute = true;
    }
}