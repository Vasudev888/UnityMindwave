using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SceneAInputManager : MonoBehaviour
{
    public TMP_InputField usernameInputField;
    public TMP_InputField ageInputField;
    public TMP_InputField companyNameInputField;

    public void OnSubmit()
    {
        // Store the input data in the UserData Singleton
        UserData.Instance.Username = usernameInputField.text;
        UserData.Instance.Age = int.Parse(ageInputField.text);  // Make sure to handle invalid input
        UserData.Instance.CompanyName = companyNameInputField.text;

        // Load the next scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(1); // Replace with your Scene B name
    }
}
