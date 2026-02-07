using UnityEngine;
using UnityEngine.Advertisements; 

public class OnClick : MonoBehaviour
{
    public string adUnitId = "Interstitial_Android"; 

    void OnMouseDown()
    {
        
        if (Advertisement.isInitialized && Advertisement.isSupported)
        {
            Advertisement.Load(adUnitId);
            Advertisement.Show(adUnitId); 
        }
        else
        {
            Debug.Log("Реклама не готова к показу.");
        }
    }
}
