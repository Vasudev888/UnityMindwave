using UnityEngine;
using TMPro;

public class SceneBDisplayManager : MonoBehaviour
{
    public TextMeshProUGUI usernameTextMesh;
    public TextMeshProUGUI ageTextMesh;
    public TextMeshProUGUI companyNameTextMesh;

    private void Start()
    {
        // Retrieve the data from UserData Singleton and display it
        usernameTextMesh.text = UserData.Instance.Username.ToString();
        ageTextMesh.text = UserData.Instance.Age.ToString() ;
        companyNameTextMesh.text = UserData.Instance.CompanyName.ToString();
    }
}
