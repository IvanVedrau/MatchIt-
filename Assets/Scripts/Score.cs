using TMPro;
using UnityEngine;

public class Score : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<TMP_Text>().text = PlayerPrefs.GetInt("Score").ToString();
    }
}
