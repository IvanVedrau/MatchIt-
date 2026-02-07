using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase.Auth;
using TMPro;

public class SignOutManager : MonoBehaviour
{
    // UI elements for sign out functionality
    [SerializeField] private Button signOutButton;
    [SerializeField] private string authSceneName = "Auth";
    [SerializeField] private GameObject confirmationDialog;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private TextMeshProUGUI loadingText;

    private void Start()
    {
        // Check if user is logged in
        signOutButton.gameObject.SetActive(FirebaseAuth.DefaultInstance.CurrentUser != null);
        signOutButton.onClick.AddListener(ShowConfirmationDialog);

        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);

        if (loadingText != null)
            loadingText.gameObject.SetActive(false);
    }

    // Show confirmation dialog before signing out
    private void ShowConfirmationDialog()
    {
        if (confirmationDialog != null)
        {
            confirmationDialog.SetActive(true);
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(SignOut);
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(HideConfirmationDialog);
        }
        else
        {
            SignOut(); // Skip confirmation if no dialog set up
        }
    }

    // Hide confirmation dialog
    private void HideConfirmationDialog()
    {
        if (confirmationDialog != null)
            confirmationDialog.SetActive(false);
    }

    // Handle user sign out
    private void SignOut()
    {
        HideConfirmationDialog();

        if (loadingText != null)
        {
            loadingText.text = "Signing out...";
            loadingText.gameObject.SetActive(true);
        }

        try
        {
            // Sign out from Firebase
            FirebaseAuth.DefaultInstance.SignOut();
            Debug.Log("User signed out successfully");

            // Return to authentication scene
            SceneManager.LoadScene(authSceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Sign out failed: " + e.Message);
            if (loadingText != null)
                loadingText.text = "Sign out failed: " + e.Message;
        }
    }

    private void OnDestroy()
    {
        signOutButton.onClick.RemoveListener(ShowConfirmationDialog);
    }
}