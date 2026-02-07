using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UniRx;
using Firebase.Auth;
using System.Threading.Tasks;

public class AuthUIManager : MonoBehaviour
{
    // UI elements for login and registration panels
    [Header("UI References")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject registerPanel;
    [SerializeField] private InputField loginEmailInput;
    [SerializeField] private InputField loginPasswordInput;
    [SerializeField] private InputField registerEmailInput;
    [SerializeField] private InputField registerPasswordInput;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button createAccountButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Text feedbackText;

    private void Start()
    {
        // Set up button click listeners
        loginButton.onClick.AddListener(LoginUser);
        createAccountButton.onClick.AddListener(ShowRegisterPanel);
        registerButton.onClick.AddListener(RegisterUser);
        backButton.onClick.AddListener(ShowLoginPanel);

        // Initialize Firebase when starting
        InitializeFirebase();
    }

    // Initialize Firebase connection
    private async void InitializeFirebase()
    {
        await FirebaseTasks.Instance.InitializeFirebase();
    }

    // Switch to registration panel
    private void ShowRegisterPanel()
    {
        loginPanel.SetActive(false);
        registerPanel.SetActive(true);
        ClearInputFields();
    }

    // Switch to login panel
    private void ShowLoginPanel()
    {
        registerPanel.SetActive(false);
        loginPanel.SetActive(true);
        ClearInputFields();
    }

    // Handle user registration
    private async void RegisterUser()
    {
        string email = registerEmailInput.text.Trim();
        string password = registerPasswordInput.text.Trim();

        if (!ValidateInputs(email, password)) return;

        try
        {
            // Create new user in Firebase
            var user = await FirebaseTasks.Instance.CreateUser(email, password);
            if (user != null)
            {
                // Reset player data for new user
                if (PlayerDataManager.Instance != null)
                {
                    PlayerDataManager.Instance.ResetState();
                }
                
                // Create initial player data in database
                await FirebaseTasks.Instance.CreatePlayerData(user.UserId, email);
                StartGame();
            }
        }
        catch (System.Exception e)
        {
            ShowMessage("Error: " + e.Message);
            Debug.LogError("Registration Error: " + e.Message);
        }
    }

    // Handle user login
    private async void LoginUser()
    {
        string email = loginEmailInput.text.Trim();
        string password = loginPasswordInput.text.Trim();

        if (!ValidateInputs(email, password)) return;

        try
        {
            // Sign in user with Firebase
            var user = await FirebaseTasks.Instance.SignIn(email, password);
            if (user != null)
            {
                // Reset player data
                if (PlayerDataManager.Instance != null)
                {
                    PlayerDataManager.Instance.ResetState();
                }
                
                // Load existing player data
                if (PlayerDataManager.Instance != null)
                {
                    await PlayerDataManager.Instance.LoadPlayerData();
                }
                StartGame();
            }
        }
        catch (System.Exception e)
        {
            ShowMessage("Error: " + e.Message);
            Debug.LogError("Login Error: " + e.Message);
        }
    }

    // Validate email and password inputs
    private bool ValidateInputs(string email, string password)
    {
        if (!IsValidEmail(email))
        {
            ShowMessage("Please enter a valid email!");
            return false;
        }

        if (string.IsNullOrEmpty(password) || password.Length < 6)
        {
            ShowMessage("Password must be at least 6 characters!");
            return false;
        }

        return true;
    }

    // Check if email format is valid
    private bool IsValidEmail(string email)
    {
        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailPattern);
    }

    // Display feedback message to user
    private void ShowMessage(string message)
    {
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);
    }

    // Clear all input fields
    private void ClearInputFields()
    {
        loginEmailInput.text = "";
        loginPasswordInput.text = "";
        registerEmailInput.text = "";
        registerPasswordInput.text = "";
        feedbackText.text = "";
        feedbackText.gameObject.SetActive(false);
    }

    // Load main game scene
    private void StartGame()
    {
        SceneManager.LoadScene("main");
    }
}