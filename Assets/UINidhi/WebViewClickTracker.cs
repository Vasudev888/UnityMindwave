using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebViewClickTracker : MonoBehaviour {
    public WebViewObject webViewObject;
    private int clickCount = 0;

    void Start()
    {
        // Find the WebView object
        webViewObject = GameObject.FindAnyObjectByType<WebViewObject>();
        webViewObject.Init(
            cb: (msg) => Debug.Log($"WebView Message: {msg}"),
            err: (msg) => Debug.Log($"WebView Error: {msg}"),
            started: (msg) => Debug.Log($"WebView Started: {msg}"),
            ld: (msg) =>
            {
                Debug.Log($"WebView Loaded: {msg}");
                // Inject JavaScript to track clicks
                webViewObject.EvaluateJS(@"
                    document.addEventListener('click', function(event) {
                        window.unityWebView.sendMessage('UserClicked', 'clicked');
                    });
                ");
            },
            enableWKWebView: true
        );

        // Load the webpage
      //  webViewObject.LoadURL("https://example.com");
        webViewObject.SetVisibility(true);
    }

    // Handle messages from the WebView
    public void UserClicked(string message)
    {
        if (message == "clicked")
        {
            clickCount++;
            Debug.Log($"User clicked on the webpage. Total clicks: {clickCount}");
        }
    }
}
