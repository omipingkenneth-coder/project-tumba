using UnityEngine;
using UnityEngine.UI;

public class LoginSystem : MonoBehaviour
{
    [Header("Registration UI")]
    public InputField regUsernameInput;
    public InputField regPasswordInput;
    public Text regStatusText;

    [Header("Login UI")]
    public InputField loginUsernameInput;
    public InputField loginPasswordInput;
    public Text loginStatusText;

    public GameObject registrationPanel;
    public GameObject loginPanel;

    private const string USERNAME_KEY = "RegisteredUsername";
    private const string PASSWORD_KEY = "RegisteredPassword";

    public void GoToLogin()
    {
        registrationPanel.SetActive(false);
        loginPanel.SetActive(true);
    }
    public void GoToRegister()
    {
        registrationPanel.SetActive(true);
        loginPanel.SetActive(false);
    }
    // ================================
    // REGISTRATION
    // ================================
    public void RegisterUser()
    {
        string username = regUsernameInput.text.Trim();
        string password = regPasswordInput.text.Trim();

        if (username.Length == 0 || password.Length == 0)
        {
            regStatusText.text = "Please fill all fields.";
            regStatusText.color = Color.red;
            return;
        }

        // Save user
        PlayerPrefs.SetString(USERNAME_KEY, username);
        PlayerPrefs.SetString(PASSWORD_KEY, password);
        PlayerPrefs.Save();

        regStatusText.text = "Registration successful!";
        regStatusText.color = Color.green;
    }

    // ================================
    // LOGIN
    // ================================
    public void LoginUser()
    {
        string savedUsername = PlayerPrefs.GetString(USERNAME_KEY, "");
        string savedPassword = PlayerPrefs.GetString(PASSWORD_KEY, "");

        string username = loginUsernameInput.text.Trim();
        string password = loginPasswordInput.text.Trim();

        if (username.Length == 0 || password.Length == 0)
        {
            loginStatusText.text = "Enter username and password.";
            loginStatusText.color = Color.red;
            return;
        }

        if (username != savedUsername)
        {
            loginStatusText.text = "Username not found!";
            loginStatusText.color = Color.red;
            return;
        }

        if (password != savedPassword)
        {
            loginStatusText.text = "Incorrect password!";
            loginStatusText.color = Color.red;
            return;
        }

        loginStatusText.text = "Login successful!";
        loginStatusText.color = Color.green;

        // TODO: Load main menu or next scene here
    }
}
