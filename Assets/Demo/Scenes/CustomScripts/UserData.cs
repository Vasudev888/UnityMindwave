using UnityEngine;

public class UserData : MonoBehaviour
{
    public static UserData Instance { get; private set; }

    public string Username { get; set; }
    public int Age { get; set; }
    public string CompanyName { get; set; }

    private void Awake()
    {
        // Check if an instance of UserData already exists
        if (Instance == null)
        {
            // If not, set this instance and mark it to not be destroyed on scene load
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // If an instance already exists, destroy this duplicate
            Destroy(gameObject);
        }
    }
}
