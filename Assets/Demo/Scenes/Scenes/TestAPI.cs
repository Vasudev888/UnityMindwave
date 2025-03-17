using System.Collections;
using TMPro;
using UnityEngine;

public class TestAPI : MonoBehaviour
{
    public TextMeshProUGUI jokeContent;
    public TextMeshProUGUI punchline;

    public void NewJoke()
    {
        Joke j = APIHelper.GetNewJoke();
        jokeContent.text = j.setup;
        punchline.text = j.punchline;
    }


}
