using UnityEngine;

public class OpenWebsite : MonoBehaviour
{
    // URL to open
    public string websiteURL = "https://www.apple.com/in/";

    // This method is called when the button is clicked
    public void OpenWebsiteOnClick()
    {
        // Opens the website in the default browser
        Application.OpenURL(websiteURL);
    }
}
