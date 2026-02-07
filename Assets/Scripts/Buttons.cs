using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buttons : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    public GameObject s_on, s_off;
    public GameObject leaderboardPanel;

    void Start()
    {
        if (gameObject.name=="Sounds")
        {
            if (PlayerPrefs.GetString("Sounds") == "No")
            {
                s_on.SetActive(false);
                s_off.SetActive(true);
            }
            else
            {
                s_on.SetActive(true);
                s_off.SetActive(false);
            }
        }
        _spriteRenderer = GetComponent<SpriteRenderer>();
        leaderboardPanel = GameObject.Find("LeaderboardPanel");
    }

    private void OnMouseDown()
    {
        _spriteRenderer.color = Color.gray;
    }

    private void OnMouseUp()
    {
        _spriteRenderer.color = Color.yellow;
    }

    private void OnMouseUpAsButton()
    {
        if (PlayerPrefs.GetString("Sounds") != "No")
            GameObject.Find("AudioClick").GetComponent<AudioSource>().Play();
            
        switch (gameObject.name)
        {
            case "Start":
                if (leaderboardPanel != null && leaderboardPanel.activeSelf)
                {
                    leaderboardPanel.SetActive(false); //  Force close it
                }
                Application.LoadLevel("play");
                break;


            case "r":
                Application.LoadLevel("play");
                break;

            case "Restart":
                Application.LoadLevel("play");
                break;
                
            case "MainMenu":
                Application.LoadLevel("main");
                break;
                
            case "RulesButton":
                Application.LoadLevel("HowToPlay");
                break;

            


            case "Leaderboard":
                if (leaderboardPanel != null)
                {
                    leaderboardPanel.SetActive(true);
                    var leaderboardUI = leaderboardPanel.GetComponent<LeaderboardUI>();
                    if (leaderboardUI != null)
                    {
                        leaderboardUI.LoadLeaderboard();
                    }
                }
                break;
                
            case "Sounds":
                if (PlayerPrefs.GetString("Sounds") != "No")
                { 
                    PlayerPrefs.SetString("Sounds", "No");
                    s_on.SetActive(false);
                    s_off.SetActive(true);
                }
                else
                { 
                    PlayerPrefs.SetString("Sounds", "Yes");
                    s_on.SetActive(true);
                    s_off.SetActive(false);
                }
                break;
        }
    }
}
