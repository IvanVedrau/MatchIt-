using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundsNoDestroy : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (GameObject.FindGameObjectsWithTag("Sounds").Length == 1)
            DontDestroyOnLoad(gameObject);
        else
            Destroy(gameObject);
    }

    
}
