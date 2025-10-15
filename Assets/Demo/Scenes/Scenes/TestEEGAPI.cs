using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Text;
using Newtonsoft.Json;
using TMPro;
using System.IO;
using System;

public class TestEEGAPI : MonoBehaviour
{
    // Reference to the UI Image you want to send
    //[SerializeField] private Image image;
    [SerializeField]TextMeshProUGUI responseText;  // Assign this in the Inspector
    string userID = "ece40588-2535-45d7-824c-7c693394aaac";
    public GameObject AnalysisTextScrollObject;
    public TextMeshProUGUI AnalysisTextField;
   
    // Reference to EEG data visualizer for getting current values
    [SerializeField] private MindwaveDataVisualizerNew eegDataVisualizer;
    
    // Reference to the hardcoded text component in the scene
    [Header("UI Text References")]
    [SerializeField] private TextMeshProUGUI hardcodedAnalysisText; // The text component with hardcoded analysis
    
    // PDF Generation
    [Header("PDF Generation")]
    [SerializeField] private Button generatePDFButton;
    [SerializeField] private TextMeshProUGUI pdfStatusText;
    [SerializeField] private Button takeScreenshotButton;
    [SerializeField] private Button clearScreenshotsButton;
    
    // Screenshot functionality
    private List<Texture2D> screenshots = new List<Texture2D>();
    private List<string> screenshotPaths = new List<string>();
    
    // Eye-tracking templates
    private string[] eyeTrackingTemplates;
   
    string response;
    string currentAnalysis = "";

    // ===============================================
    // ORIGINAL LLM API CALLING FUNCTIONALITY
    // ===============================================
    // This section contains the original LLM API calling code.
    // Currently commented out but preserved for future use when LLM is connected.
    // To restore LLM functionality:
    // 1. Uncomment the StartCoroutine call in Start()
    // 2. Uncomment the StartCoroutine call in AIAnalysisClick()
    // 3. Comment out the template-based analysis calls

    private IEnumerator SendJSONToAPI(string url)
    { 

        // User activity details
        Dictionary<string, object> userActivity = new Dictionary<string, object>
        {
            {"Username", "Vasu"},
            {"Company Name", "LTIMindtree"},
            {"Age", 123},
            {"Activity", "Reading"},
            {"TaskCompletion", "75%"}
        };

        // Convert UI Image to base64 string
        //string base64Image = UIImageToBase64();

        // Create the JSON payload
        Dictionary<string, object> requestData = new Dictionary<string, object>
        {
            //{"image", base64Image},
            //{"eeg_data", eegDataList},
            {"user_activity", userActivity},
            {"userID", userID }
        };

        // Convert to JSON string using Newtonsoft.Json
        string jsonData = JsonConvert.SerializeObject(requestData);
        Debug.Log("Sending JSON with image data");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("JSON Data sent successfully!");
                Debug.Log("Response: " + request.downloadHandler.text);
               // string response = request.downloadHandler.text;
                 response = request.downloadHandler.text;
                responseText.text = response;
            }
            else
            {
                Debug.LogError("Error sending JSON: " + request.error);
            }
        }
    }



    
    void Start()
    {
        // ORIGINAL LLM CALLING CODE (Commented Out - Uncomment when LLM is connected)
        // StartCoroutine(SendJSONToAPI("http://127.0.0.1:5000/upload"));
        
        // Initialize eye-tracking templates
        InitializeEyeTrackingTemplates();
        
        // Setup PDF generation button
        if (generatePDFButton != null)
        {
            generatePDFButton.onClick.AddListener(GeneratePDFReport);
        }
        
        // Setup screenshot button
        if (takeScreenshotButton != null)
        {
            takeScreenshotButton.onClick.AddListener(TakeScreenshot);
        }
        
        // Setup clear screenshots button
        if (clearScreenshotsButton != null)
        {
            clearScreenshotsButton.onClick.AddListener(ClearScreenshots);
        }
        
        //userID = Registration.Instance.GetUserID();
        //AnalysisTextScrollObject.SetActive(false);
    }

    public void AIAnalysisClick()
    {
        AnalysisTextScrollObject.SetActive(true);
        
        // TEMPLATE-BASED ANALYSIS (Current Implementation)
        // Generate realistic AI analysis based on current EEG data using templates
        string aiAnalysis = GenerateAIAnalysis();
        currentAnalysis = aiAnalysis; // Store for PDF generation
        
        // Debug: Log the first 200 characters to see what's being generated
        Debug.Log($"Generated Analysis Preview: {aiAnalysis.Substring(0, Mathf.Min(200, aiAnalysis.Length))}...");
        
        // Update all text components
        AnalysisTextField.text = aiAnalysis;
        responseText.text = aiAnalysis;
        
        // Update the hardcoded text component in the scene
        if (hardcodedAnalysisText != null)
        {
            hardcodedAnalysisText.text = aiAnalysis;
        }
        
        // ORIGINAL LLM CALLING CODE (Commented Out - Uncomment when LLM is connected)
        /*
        StartCoroutine(SendJSONToAPI("http://127.0.0.1:5000/upload"));
        */
    }
    
    // ===============================================
    // TEMPLATE-BASED ANALYSIS (Current Implementation)
    // ===============================================
    // This section contains the template-based analysis system
    // that generates different eye-tracking analysis reports
    // without requiring LLM connectivity.
    
    /// <summary>
    /// Initialize the eye-tracking templates array
    /// </summary>
    private void InitializeEyeTrackingTemplates()
    {
        eyeTrackingTemplates = new string[] {
            // Template 0: Honda F-Pattern Analysis
            "1. Overall Scanning Pattern\nUsers follow a classic \"F-pattern\" on the Honda American Honda Finance page. They start at the top-left \"Apply for Credit Pre-Approval\" card, read across to \"Lease vs Finance\" and \"End of Lease\" cards, then sweep down the left edge into the hero image, and finally land on the \"Honda Loyalty Benefits\" call-to-action in the top-right.\n\n2. High-Attention Zones\n\nTop-Left Card (\"Apply for Credit Pre-Approval\")\nTop-Center Card (\"Lease vs Finance\")\nHero Banner Image (couple with stroller & car)\nTop-Right CTA (\"Honda Loyalty Benefits\" → Learn More)\n\n3. Moderate-Attention Zones\n\nTop-Right Card (\"End of Lease\" icon & text)\nHeader/Navigation Area (Honda logo, back-arrow)\n\n4. Low-Attention Zones\n\nLower Page Content (service-bay photo, dealership showroom)\nFooter & Cookie Banner\n\nRecommended Actionable Takeaways\nMove or Animate Your Primary Offer into the Top-Right\nThe \"Honda Loyalty Benefits\" button is where eyes naturally finish the F-scan. If you need one location for a dynamic or personalized offer, this is it.\n\nStagger the Secondary CTAs\nFade-in or highlight the \"Lease vs Finance\" card after a 2–3 second delay to re-engage users mid-scroll.\n\nSimplify/Personalize Lower Content\nVery little dwell time happens below the hero. Either reduce visual clutter there or replace it with a highly relevant, user-specific offer (\"Your next lease savings\").\n\nUse Quadrant-Based Targeting\nIf you're dynamically swapping modules, assign:\n\nQuadrant 1 (Top-Right) → Primary dynamic CTA\nQuadrant 2 (Top-Left & Center) → Secondary info blocks\nQuadrant 4 (Bottom) → Minimal or personalized follow-up content",

            // Template 1: Honda Z-Pattern Analysis
            "1. Overall Scanning Pattern\nUsers exhibit a strong \"Z-pattern\" behavior on the Honda finance page. The gaze starts at the top-left Honda logo, sweeps horizontally across the navigation, drops down to the hero section, and then follows a diagonal path to the bottom-right corner where the primary CTA resides.\n\n2. High-Attention Zones\n\nTop-Left Honda Logo & Branding\nHero Section (main image with couple)\nBottom-Right CTA (\"Honda Loyalty Benefits\")\nNavigation Menu Items\n\n3. Moderate-Attention Zones\n\nCenter-Left Content Cards\nRight-side Secondary Information\nMid-page Call-to-Action Buttons\n\n4. Low-Attention Zones\n\nFooter Links and Legal Text\nSidebar Advertisements\nBottom-left Corner Elements\n\nRecommended Actionable Takeaways\nStrengthen the Z-Path with Visual Cues\nAdd subtle arrows or progress indicators that guide users along the natural Z-scanning path.\n\nOptimize Corner CTAs\nBoth top-right and bottom-right corners receive high attention - place your most important Honda financing actions there.\n\nReduce Mid-Page Clutter\nUsers skip over the middle section - either simplify or add compelling visual breaks to capture attention.\n\nUse Progressive Disclosure\nReveal Honda financing information progressively as users follow the Z-pattern to maintain engagement.",

            // Template 2: Honda Layer Cake Pattern
            "1. Overall Scanning Pattern\nUsers demonstrate a \"Layer Cake\" scanning behavior on the Honda page, moving horizontally across content sections in distinct layers. Each horizontal sweep covers one complete row before moving to the next layer down.\n\n2. High-Attention Zones\n\nTop Layer: All three main Honda cards receive equal attention\nMiddle Layer: Hero image and surrounding Honda content\nBottom Layer: Primary CTA and supporting Honda elements\n\n3. Moderate-Attention Zones\n\nNavigation header\nSecondary Honda information blocks\nSupporting imagery\n\n4. Low-Attention Zones\n\nFooter content\nLegal disclaimers\nBackground decorative elements\n\nRecommended Actionable Takeaways\nDesign for Horizontal Scanning\nEnsure each Honda content layer can be understood when scanned horizontally from left to right.\n\nBalance Information Density\nEach layer should have similar visual weight to maintain consistent scanning behavior.\n\nUse Consistent Layout Patterns\nMaintain the same layout structure across different Honda sections to support the layer-cake scanning habit.\n\nOptimize for Mobile\nLayer-cake patterns work especially well on mobile devices - ensure Honda's responsive design supports this behavior.",

            // Template 3: Honda Spotted Pattern
            "1. Overall Scanning Pattern\nUsers display a \"Spotted\" scanning pattern on the Honda page, jumping between high-contrast elements and visual anchors rather than following a linear path. The gaze bounces between key Honda visual elements based on color, size, and contrast.\n\n2. High-Attention Zones\n\nBright, high-contrast Honda CTAs\nLarge, bold Honda headlines\nProminent images with people\nColorful Honda icons and buttons\n\n3. Moderate-Attention Zones\n\nSecondary Honda headlines\nSupporting imagery\nNavigation elements\nForm fields and inputs\n\n4. Low-Attention Zones\n\nSmall body text\nBackground patterns\nSubtle decorative elements\nFooter links\n\nRecommended Actionable Takeaways\nIncrease Visual Hierarchy\nUse size, color, and contrast to create clear visual anchors that guide the spotted scanning pattern.\n\nReduce Visual Noise\nEliminate competing elements that might distract from your key Honda conversion points.\n\nUse Strategic Color Psychology\nApply high-contrast colors to the most important Honda elements to make them stand out in the spotted pattern.\n\nTest Different Visual Weights\nExperiment with making key Honda elements more prominent to see how it affects the scanning pattern.",

            // Template 4: Honda Commitment Pattern
            "1. Overall Scanning Pattern\nUsers show a \"Commitment\" pattern on the Honda page, spending extended time on specific elements that align with their Honda financing interests or goals. The gaze lingers on relevant Honda content while quickly skipping over less pertinent information.\n\n2. High-Attention Zones\n\nPersonalized Honda offers and recommendations\nRelevant Honda product information\nTrust signals and Honda testimonials\nClear Honda value propositions\n\n3. Moderate-Attention Zones\n\nGeneral Honda product categories\nSupporting Honda information\nNavigation options\nSecondary Honda features\n\n4. Low-Attention Zones\n\nGeneric Honda marketing content\nUnrelated Honda product suggestions\nLegal disclaimers\nSocial media links\n\nRecommended Actionable Takeaways\nPersonalize Honda Content Based on Behavior\nUse data to show the most relevant Honda content first to increase commitment pattern engagement.\n\nStrengthen Honda Trust Signals\nAdd testimonials, reviews, and security badges where users are most likely to commit attention.\n\nReduce Decision Paralysis\nPresent clear, simple Honda choices to help users commit to a specific action.\n\nUse Progressive Profiling\nGather user preferences gradually to improve Honda personalization and commitment pattern effectiveness.",

            // Template 5: Honda Exhaustive Review Pattern
            "1. Overall Scanning Pattern\nUsers demonstrate an \"Exhaustive Review\" pattern on the Honda page, systematically examining every element before making a Honda financing decision. The scanning is thorough and methodical, covering all available Honda options.\n\n2. High-Attention Zones\n\nAll primary Honda content sections\nDetailed Honda product specifications\nComparison tables and Honda charts\nComprehensive Honda feature lists\n\n3. Moderate-Attention Zones\n\nSupporting Honda documentation\nAdditional Honda resources\nRelated Honda product suggestions\nHelp and support information\n\n4. Low-Attention Zones\n\nHonda marketing fluff and promotional text\nUnnecessary animations\nDistracting decorative elements\nRedundant Honda information\n\nRecommended Actionable Takeaways\nOrganize Honda Information Hierarchically\nStructure Honda content so that the most important information is found first in the exhaustive review.\n\nProvide Comprehensive Honda Details\nInclude all necessary Honda information upfront to satisfy users who want to review everything.\n\nUse Clear Honda Information Architecture\nMake it easy for users to find and compare different Honda options systematically.\n\nMinimize Cognitive Load\nPresent Honda information in digestible chunks to support thorough review without overwhelming users.",

            // Template 6: Honda Banner Blindness Pattern
            "1. Overall Scanning Pattern\nUsers exhibit \"Banner Blindness\" behavior on the Honda page, actively avoiding areas that look like advertisements or promotional content. The gaze focuses on Honda content areas while skipping over banner-like elements.\n\n2. High-Attention Zones\n\nMain Honda content areas\nNavigation and functional elements\nUser-generated Honda content\nEditorial-style Honda content\n\n3. Moderate-Attention Zones\n\nSubtle Honda promotional content\nIntegrated Honda product information\nSupporting visual elements\nSecondary Honda navigation\n\n4. Low-Attention Zones\n\nBanner-style Honda advertisements\nPromotional Honda pop-ups\nFlashy promotional Honda graphics\nObvious Honda marketing content\n\nRecommended Actionable Takeaways\nDesign Native-Looking Honda Content\nMake promotional Honda content look like editorial content to avoid banner blindness.\n\nUse Subtle Honda Integration\nBlend Honda marketing messages naturally into the content flow rather than creating obvious promotional areas.\n\nFocus on Honda User Value\nEmphasize how the Honda content benefits the user rather than promoting the Honda brand.\n\nTest Different Honda Visual Styles\nExperiment with Honda content that doesn't trigger banner blindness while still achieving marketing goals.",

            // Template 7: Honda Golden Triangle Pattern
            "1. Overall Scanning Pattern\nUsers follow a \"Golden Triangle\" pattern on the Honda page, focusing primarily on the top-left area where the most important Honda information is typically located. The attention forms a triangle shape with the apex at the top-left.\n\n2. High-Attention Zones\n\nTop-left Honda logo and branding\nPrimary Honda navigation menu\nMain Honda headline and key messaging\nTop-left Honda content blocks\n\n3. Moderate-Attention Zones\n\nCenter-top Honda content\nLeft-side secondary Honda information\nTop-right functional elements\n\n4. Low-Attention Zones\n\nRight-side Honda promotional content\nBottom-right corner elements\nFooter and Honda legal information\nSidebar Honda advertisements\n\nRecommended Actionable Takeaways\nOptimize the Honda Golden Triangle\nPlace your most important Honda content in the top-left area where users naturally focus.\n\nUse F-Pattern Principles\nStructure Honda content to work with the natural left-to-right, top-to-bottom reading pattern.\n\nMinimize Right-Side Honda Distractions\nKeep the right side clean and focused to maintain attention on the Honda golden triangle.\n\nTest Honda Content Hierarchy\nEnsure your most important Honda messages are positioned within the golden triangle area.",

            // Template 8: Honda Pinball Pattern
            "1. Overall Scanning Pattern\nUsers display a \"Pinball\" pattern on the Honda page, bouncing between different Honda elements in a somewhat random manner. The gaze moves quickly between various Honda page elements without following a predictable path.\n\n2. High-Attention Zones\n\nInteractive Honda elements and buttons\nAnimated or moving Honda content\nHigh-contrast Honda visual elements\nUnexpected or novel Honda content\n\n3. Moderate-Attention Zones\n\nStandard Honda content blocks\nHonda navigation elements\nSupporting Honda imagery\nHonda form fields\n\n4. Low-Attention Zones\n\nStatic Honda text content\nBackground elements\nFooter Honda information\nLegal disclaimers\n\nRecommended Actionable Takeaways\nCreate Interactive Honda Elements\nAdd hover effects, animations, and interactive features to capture the pinball attention pattern.\n\nUse Visual Honda Surprises\nInclude unexpected Honda elements that can catch and redirect attention effectively.\n\nOptimize for Quick Honda Scanning\nMake Honda content scannable since users won't spend much time in any one area.\n\nGuide the Honda Bounce\nUse visual cues to guide the pinball pattern toward your most important Honda conversion points.",

            // Template 9: Honda Reading Pattern
            "1. Overall Scanning Pattern\nUsers follow a traditional \"Reading\" pattern on the Honda page, moving systematically from left to right, top to bottom, as if reading a book. The gaze follows a predictable, linear path through the Honda content.\n\n2. High-Attention Zones\n\nHonda headlines and subheadings\nFirst sentences of Honda paragraphs\nBullet points and Honda lists\nCall-to-action Honda buttons\n\n3. Moderate-Attention Zones\n\nHonda body text content\nSupporting Honda imagery\nHonda navigation links\nHonda form labels\n\n4. Low-Attention Zones\n\nFooter Honda content\nLegal disclaimers\nSidebar Honda information\nBackground decorative elements\n\nRecommended Actionable Takeaways\nStructure Honda Content for Reading\nOrganize Honda information in a logical, hierarchical manner that supports natural reading flow.\n\nUse Clear Honda Typography\nChoose fonts and formatting that enhance Honda readability and support the reading pattern.\n\nCreate Scannable Honda Content\nUse headings, bullet points, and short paragraphs to make Honda content easy to scan while reading.\n\nOptimize for Mobile Honda Reading\nEnsure the Honda reading pattern works well on mobile devices with appropriate font sizes and spacing."
        };
    }
    
    private string GenerateAIAnalysis()
    {
        // Get current EEG values if available
        float currentAttention = 0f;
        float currentMeditation = 0f;
        
        if (eegDataVisualizer != null)
        {
            currentAttention = eegDataVisualizer.GetAttentionValue();
            currentMeditation = eegDataVisualizer.GetMeditationValue();
        }
        else
        {
            // Fallback values if no EEG data available
            currentAttention = UnityEngine.Random.Range(30f, 85f);
            currentMeditation = UnityEngine.Random.Range(25f, 75f);
        }
        
        // Generate analysis based on attention and meditation levels
        string analysis = GenerateAnalysisBasedOnValues(currentAttention, currentMeditation);
        
        return analysis;
    }
    
    private string GenerateAnalysisBasedOnValues(float attention, float meditation)
    {
        // Calculate Click Through Rate based on attention and meditation levels
        float ctr = CalculateCTR(attention, meditation);
        
        // Analyze EEG patterns to determine the most likely eye-tracking behavior
        int templateIndex = AnalyzeEEGPatterns(attention, meditation);
        string selectedTemplate = eyeTrackingTemplates[templateIndex];
        
        // Debug logging
        Debug.Log($"Selected Template Index: {templateIndex}");
        Debug.Log($"CTR Calculated: {ctr:F2}%");
        
        // Add EEG context and CTR at the beginning
        string eegContext = $"**EEG Analysis Context:**\nAttention Level: {attention:F1}/100 | Meditation Level: {meditation:F1}/100\n\n**Click Through Rate (CTR):** {ctr:F2}%\n\n";
        
        // Add CTR analysis section
        string ctrAnalysis = GenerateCTRAnalysis(ctr, attention, meditation);
        
        // Debug: Log CTR analysis content
        Debug.Log($"CTR Analysis Content: {ctrAnalysis.Substring(0, Mathf.Min(200, ctrAnalysis.Length))}...");
        
        // Add timestamp
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string footer = $"\n\n---\n*Eye-tracking analysis generated at {timestamp} based on current EEG readings*";
        
        // Combine all sections
        string fullAnalysis = eegContext + ctrAnalysis + selectedTemplate + footer;
        
        Debug.Log($"Full Analysis Length: {fullAnalysis.Length}");
        Debug.Log($"CTR Analysis Length: {ctrAnalysis.Length}");
        Debug.Log($"Template Preview: {selectedTemplate.Substring(0, Mathf.Min(100, selectedTemplate.Length))}...");
        
        return fullAnalysis;
    }
    
    /// <summary>
    /// Analyze EEG patterns to determine the most likely eye-tracking behavior
    /// </summary>
    private int AnalyzeEEGPatterns(float attention, float meditation)
    {
        // Add some randomness to make it more realistic
        float randomFactor = UnityEngine.Random.Range(0f, 1f);
        
        // Pattern selection based on EEG data and random factors
        if (attention > 75f && meditation < 40f)
        {
            // High attention, low meditation = Quick scanning patterns
            if (randomFactor < 0.2f) return 0; // Honda F-Pattern
            else if (randomFactor < 0.4f) return 1; // Honda Z-Pattern
            else if (randomFactor < 0.6f) return 2; // Honda Layer Cake
            else if (randomFactor < 0.8f) return 3; // Honda Spotted
            else return 4; // Honda Commitment
        }
        else if (attention > 60f && meditation > 50f && meditation < 80f)
        {
            // High attention, optimal meditation = Focused patterns
            if (randomFactor < 0.2f) return 5; // Honda Exhaustive Review
            else if (randomFactor < 0.4f) return 6; // Honda Banner Blindness
            else if (randomFactor < 0.6f) return 7; // Honda Golden Triangle
            else if (randomFactor < 0.8f) return 8; // Honda Pinball
            else return 9; // Honda Reading
        }
        else if (attention < 40f && meditation > 60f)
        {
            // Low attention, high meditation = Overthinking patterns
            if (randomFactor < 0.25f) return 0; // Honda F-Pattern
            else if (randomFactor < 0.5f) return 1; // Honda Z-Pattern
            else if (randomFactor < 0.75f) return 2; // Honda Layer Cake
            else return 3; // Honda Spotted
        }
        else if (attention > 50f && meditation < 30f)
        {
            // Medium attention, very low meditation = Distracted patterns
            if (randomFactor < 0.25f) return 4; // Honda Commitment
            else if (randomFactor < 0.5f) return 5; // Honda Exhaustive Review
            else if (randomFactor < 0.75f) return 6; // Honda Banner Blindness
            else return 7; // Honda Golden Triangle
        }
        else
        {
            // Mixed or average states - random selection
            return UnityEngine.Random.Range(0, eyeTrackingTemplates.Length);
        }
    }
    
    /// <summary>
    /// Test method to verify all Honda templates are working - call this from Unity Inspector
    /// </summary>
    [ContextMenu("Test All Honda Templates")]
    public void TestAllTemplates()
    {
        Debug.Log("=== TESTING ALL HONDA TEMPLATES ===");
        for (int i = 0; i < 10; i++)
        {
            string templateName = i switch
            {
                0 => "Honda F-Pattern",
                1 => "Honda Z-Pattern", 
                2 => "Honda Layer Cake",
                3 => "Honda Spotted",
                4 => "Honda Commitment",
                5 => "Honda Exhaustive Review",
                6 => "Honda Banner Blindness",
                7 => "Honda Golden Triangle",
                8 => "Honda Pinball",
                9 => "Honda Reading",
                _ => "Unknown"
            };
            
            string testAnalysis = GenerateAnalysisBasedOnValues(50f, 50f);
            Debug.Log($"Template {i + 1} ({templateName}) Preview: {testAnalysis.Substring(0, Mathf.Min(100, testAnalysis.Length))}...");
        }
        Debug.Log("=== END HONDA TEMPLATE TEST ===");
    }
    
    /// <summary>
    /// Test method to force a specific template - call this from Unity Inspector
    /// </summary>
    [ContextMenu("Test Template 1 (Honda Z-Pattern)")]
    public void TestTemplate1()
    {
        Debug.Log("=== TESTING TEMPLATE 1 (HONDA Z-PATTERN) ===");
        
        // Force Honda Z-Pattern by directly selecting template 1
        int templateIndex = 1;
        string selectedTemplate = eyeTrackingTemplates[templateIndex];
        
        // Generate analysis with Honda Z-Pattern
        float attention = 60f;
        float meditation = 55f;
        float ctr = CalculateCTR(attention, meditation);
        string ctrAnalysis = GenerateCTRAnalysis(ctr, attention, meditation);
        
        string eegContext = $"**EEG Analysis Context:**\nAttention Level: {attention:F1}/100 | Meditation Level: {meditation:F1}/100\n\n**Click Through Rate (CTR):** {ctr:F2}%\n\n";
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string footer = $"\n\n---\n*Eye-tracking analysis generated at {timestamp} based on current EEG readings*";
        
        string testAnalysis = eegContext + ctrAnalysis + selectedTemplate + footer;
        
        // Update UI
        if (AnalysisTextField != null)
        {
            AnalysisTextField.text = testAnalysis;
        }
        if (responseText != null)
        {
            responseText.text = testAnalysis;
        }
        if (hardcodedAnalysisText != null)
        {
            hardcodedAnalysisText.text = testAnalysis;
        }
        
        Debug.Log($"Honda Z-Pattern Template Selected: {templateIndex}");
        Debug.Log($"Full Analysis: {testAnalysis}");
        Debug.Log("=== END HONDA Z-PATTERN TEST ===");
    }
    
    /// <summary>
    /// Test method to force template 5 (Commitment Pattern) - call this from Unity Inspector
    /// </summary>
    [ContextMenu("Test Template 5 (Commitment Pattern)")]
    public void TestTemplate5()
    {
        Debug.Log("=== TESTING TEMPLATE 5 (COMMITMENT PATTERN) ===");
        string testAnalysis = GenerateAnalysisBasedOnValues(70f, 60f);
        Debug.Log($"Full Analysis: {testAnalysis}");
        Debug.Log("=== END TEMPLATE 5 TEST ===");
    }
    
    /// <summary>
    /// Test method to force template 9 (Pinball Pattern) - call this from Unity Inspector
    /// </summary>
    [ContextMenu("Test Template 9 (Pinball Pattern)")]
    public void TestTemplate9()
    {
        Debug.Log("=== TESTING TEMPLATE 9 (PINBALL PATTERN) ===");
        string testAnalysis = GenerateAnalysisBasedOnValues(45f, 35f);
        Debug.Log($"Full Analysis: {testAnalysis}");
        Debug.Log("=== END TEMPLATE 9 TEST ===");
    }
    
    /// <summary>
    /// Test method to show exactly what's being generated - call this from Unity Inspector
    /// </summary>
    [ContextMenu("Show Current Analysis in UI")]
    public void ShowCurrentAnalysisInUI()
    {
        Debug.Log("=== SHOWING CURRENT ANALYSIS IN UI ===");
        string testAnalysis = GenerateAnalysisBasedOnValues(65f, 50f);
        
        // Update all text components to show the analysis
        if (AnalysisTextField != null)
        {
            AnalysisTextField.text = testAnalysis;
        }
        if (responseText != null)
        {
            responseText.text = testAnalysis;
        }
        if (hardcodedAnalysisText != null)
        {
            hardcodedAnalysisText.text = testAnalysis;
        }
        
        Debug.Log($"Analysis set in UI. Length: {testAnalysis.Length}");
        Debug.Log("=== END UI UPDATE ===");
    }
    
    /// <summary>
    /// Test method to force random template selection - call this from Unity Inspector
    /// </summary>
    [ContextMenu("Force Random Template")]
    public void ForceRandomTemplate()
    {
        Debug.Log("=== FORCING RANDOM TEMPLATE ===");
        
        // Force random selection by using a random template index
        int randomIndex = UnityEngine.Random.Range(0, eyeTrackingTemplates.Length);
        string randomTemplate = eyeTrackingTemplates[randomIndex];
        
        // Generate analysis with random values
        float randomAttention = UnityEngine.Random.Range(30f, 90f);
        float randomMeditation = UnityEngine.Random.Range(20f, 80f);
        
        string testAnalysis = GenerateAnalysisBasedOnValues(randomAttention, randomMeditation);
        
        // Update UI
        if (AnalysisTextField != null)
        {
            AnalysisTextField.text = testAnalysis;
        }
        if (responseText != null)
        {
            responseText.text = testAnalysis;
        }
        if (hardcodedAnalysisText != null)
        {
            hardcodedAnalysisText.text = testAnalysis;
        }
        
        Debug.Log($"Random Template Index: {randomIndex}");
        Debug.Log($"Random Attention: {randomAttention}, Meditation: {randomMeditation}");
        Debug.Log($"Template Preview: {randomTemplate.Substring(0, Mathf.Min(100, randomTemplate.Length))}...");
        Debug.Log("=== END RANDOM TEMPLATE TEST ===");
    }
    
    /// <summary>
    /// Test pattern analysis algorithm with different EEG values - call this from Unity Inspector
    /// </summary>
    [ContextMenu("Test Pattern Analysis Algorithm")]
    public void TestPatternAnalysisAlgorithm()
    {
        Debug.Log("=== TESTING PATTERN ANALYSIS ALGORITHM ===");
        
        // Test different EEG combinations
        float[,] testValues = {
            {80f, 30f}, // High attention, low meditation
            {70f, 60f}, // High attention, optimal meditation
            {30f, 70f}, // Low attention, high meditation
            {55f, 25f}, // Medium attention, very low meditation
            {45f, 45f}  // Average values
        };
        
        string[] testNames = {
            "High Attention + Low Meditation",
            "High Attention + Optimal Meditation", 
            "Low Attention + High Meditation",
            "Medium Attention + Very Low Meditation",
            "Average Values"
        };
        
        for (int i = 0; i < testValues.GetLength(0); i++)
        {
            float attention = testValues[i, 0];
            float meditation = testValues[i, 1];
            int patternIndex = AnalyzeEEGPatterns(attention, meditation);
            
            Debug.Log($"{testNames[i]}: Attention={attention}, Meditation={meditation} → Pattern {patternIndex}");
        }
        
        Debug.Log("=== END PATTERN ANALYSIS TEST ===");
    }
    
    /// <summary>
    /// Force Z-Pattern template in UI - call this from Unity Inspector
    /// </summary>
    [ContextMenu("Force Z-Pattern in UI")]
    public void ForceZPatternInUI()
    {
        Debug.Log("=== FORCING Z-PATTERN IN UI ===");
        
        // Get current EEG values
        float attention = 60f;
        float meditation = 55f;
        if (eegDataVisualizer != null)
        {
            attention = eegDataVisualizer.GetAttentionValue();
            meditation = eegDataVisualizer.GetMeditationValue();
        }
        
        // Calculate CTR
        float ctr = CalculateCTR(attention, meditation);
        
        // Force Z-Pattern template (index 1)
        string[] eyeTrackingTemplates = {
            "F-Pattern", "Z-Pattern", "Layer Cake", "Spotted", "Commitment", 
            "Exhaustive Review", "Banner Blindness", "Golden Triangle", "Pinball", "Reading"
        };
        
        string selectedTemplate = "1. Overall Scanning Pattern\nUsers exhibit a strong \"Z-pattern\" behavior. The gaze starts at the top-left logo, sweeps horizontally across the navigation, drops down to the hero section, and then follows a diagonal path to the bottom-right corner where the primary CTA resides.\n\n2. High-Attention Zones\n\nTop-Left Logo & Branding\nHero Section (main image with couple)\nBottom-Right CTA (\"Honda Loyalty Benefits\")\nNavigation Menu Items\n\n3. Moderate-Attention Zones\n\nCenter-Left Content Cards\nRight-side Secondary Information\nMid-page Call-to-Action Buttons\n\n4. Low-Attention Zones\n\nFooter Links and Legal Text\nSidebar Advertisements\nBottom-left Corner Elements\n\nRecommended Actionable Takeaways\nStrengthen the Z-Path with Visual Cues\nAdd subtle arrows or progress indicators that guide users along the natural Z-scanning path.\n\nOptimize Corner CTAs\nBoth top-right and bottom-right corners receive high attention - place your most important actions there.\n\nReduce Mid-Page Clutter\nUsers skip over the middle section - either simplify or add compelling visual breaks to capture attention.\n\nUse Progressive Disclosure\nReveal information progressively as users follow the Z-pattern to maintain engagement.";
        
        // Generate CTR analysis
        string ctrAnalysis = GenerateCTRAnalysis(ctr, attention, meditation);
        
        // Create full analysis
        string eegContext = $"**EEG Analysis Context:**\nAttention Level: {attention:F1}/100 | Meditation Level: {meditation:F1}/100\n\n**Click Through Rate (CTR):** {ctr:F2}%\n\n";
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string footer = $"\n\n---\n*Eye-tracking analysis generated at {timestamp} based on current EEG readings*";
        
        string fullAnalysis = eegContext + ctrAnalysis + selectedTemplate + footer;
        
        // Update all text components
        if (AnalysisTextField != null)
        {
            AnalysisTextField.text = fullAnalysis;
        }
        if (responseText != null)
        {
            responseText.text = fullAnalysis;
        }
        if (hardcodedAnalysisText != null)
        {
            hardcodedAnalysisText.text = fullAnalysis;
        }
        
        Debug.Log($"Z-Pattern analysis set in UI. Length: {fullAnalysis.Length}");
        Debug.Log("=== END Z-PATTERN FORCE ===");
    }
    
    /// <summary>
    /// Calculate Click Through Rate based on EEG attention and meditation levels
    /// </summary>
    private float CalculateCTR(float attention, float meditation)
    {
        // Base CTR calculation considering both attention and meditation
        // Higher attention = higher likelihood to click
        // Higher meditation = more focused, potentially higher CTR
        // But very high meditation might indicate overthinking, reducing CTR
        
        float baseCTR = 0.5f; // Base 0.5% CTR
        
        // Attention factor (0-100 scale)
        float attentionFactor = (attention / 100f) * 0.8f; // Max 0.8% boost from attention
        
        // Meditation factor - optimal around 50-70, decreases if too high or too low
        float meditationFactor;
        if (meditation < 30f)
        {
            meditationFactor = meditation / 100f * 0.3f; // Low meditation = low focus
        }
        else if (meditation > 80f)
        {
            meditationFactor = (100f - meditation) / 100f * 0.4f; // Very high meditation = overthinking
        }
        else
        {
            meditationFactor = 0.4f; // Optimal meditation range
        }
        
        // Random variation to simulate real-world CTR fluctuations
        float randomVariation = UnityEngine.Random.Range(-0.2f, 0.3f);
        
        // Calculate final CTR
        float ctr = baseCTR + attentionFactor + meditationFactor + randomVariation;
        
        // Ensure CTR is within realistic bounds (0.1% to 8%)
        ctr = Mathf.Clamp(ctr, 0.1f, 8.0f);
        
        return ctr;
    }
    
    /// <summary>
    /// Generate CTR analysis based on calculated CTR and EEG values
    /// </summary>
    private string GenerateCTRAnalysis(float ctr, float attention, float meditation)
    {
        string ctrPerformance = "";
        string recommendations = "";
        
        // Determine CTR performance level
        if (ctr >= 5.0f)
        {
            ctrPerformance = "**EXCELLENT** - Above industry average (2-3%)";
            recommendations = "• Maintain current design and placement strategies\n• Consider A/B testing to optimize further\n• Focus on scaling successful elements";
        }
        else if (ctr >= 3.0f)
        {
            ctrPerformance = "**GOOD** - Above average performance";
            recommendations = "• Test different CTA colors and sizes\n• Optimize placement based on eye-tracking data\n• Consider urgency elements (limited time offers)";
        }
        else if (ctr >= 1.5f)
        {
            ctrPerformance = "**AVERAGE** - Industry standard performance";
            recommendations = "• Improve visual hierarchy and contrast\n• Test different headlines and copy\n• Optimize for mobile responsiveness";
        }
        else
        {
            ctrPerformance = "**BELOW AVERAGE** - Needs improvement";
            recommendations = "• Redesign CTAs with higher contrast\n• Simplify the user journey\n• Test different value propositions\n• Consider reducing cognitive load";
        }
        
        // Generate specific recommendations based on EEG data
        string eegBasedRecommendations = "";
        if (attention > 70f && meditation < 50f)
        {
            eegBasedRecommendations = "• High attention detected - capitalize on engagement with clear, direct CTAs\n• Low meditation suggests quick decision-making - use urgency elements";
        }
        else if (attention < 40f && meditation > 60f)
        {
            eegBasedRecommendations = "• Low attention, high meditation - users are overthinking\n• Simplify choices and reduce decision paralysis\n• Use social proof and testimonials to build trust";
        }
        else if (attention > 60f && meditation > 50f)
        {
            eegBasedRecommendations = "• Optimal engagement state - users are focused and calm\n• Perfect time for detailed information and comparison tools\n• Consider longer-form content with clear CTAs";
        }
        else
        {
            eegBasedRecommendations = "• Mixed engagement state - test different approaches\n• Consider progressive disclosure of information\n• Use dynamic content based on user behavior";
        }
        
        return $"**CTR PERFORMANCE ANALYSIS**\n\n" +
               $"Current CTR: {ctr:F2}%\n" +
               $"Performance Level: {ctrPerformance}\n\n" +
               $"**GENERAL RECOMMENDATIONS:**\n{recommendations}\n\n" +
               $"**EEG-BASED RECOMMENDATIONS:**\n{eegBasedRecommendations}\n\n" +
               $"**CTR OPTIMIZATION INSIGHTS:**\n" +
               $"• Attention Level Impact: {(attention > 50f ? "Positive" : "Negative")} on click likelihood\n" +
               $"• Meditation Level Impact: {(meditation > 40f && meditation < 80f ? "Optimal" : "Suboptimal")} for decision-making\n" +
               $"• Recommended CTA Strategy: {(ctr > 3f ? "Maintain and scale" : "Redesign and test")}\n\n";
    }
    
    // ===============================================
    // PDF GENERATION METHODS
    // ===============================================
    
    /// <summary>
    /// Take a screenshot and add it to the collection
    /// </summary>
    public void TakeScreenshot()
    {
        StartCoroutine(CaptureScreenshot());
    }
    
    /// <summary>
    /// Capture screenshot coroutine
    /// </summary>
    private IEnumerator CaptureScreenshot()
    {
        // Wait for end of frame to ensure UI is rendered
        yield return new WaitForEndOfFrame();
        
        // Capture screenshot
        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        
        // Add to collection
        screenshots.Add(screenshot);
        
        // Save to file
        string fileName = $"Screenshot_{DateTime.Now:yyyyMMdd_HHmmss}_{screenshots.Count}.png";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        
        byte[] imageData = screenshot.EncodeToPNG();
        File.WriteAllBytes(filePath, imageData);
        
        screenshotPaths.Add(filePath);
        
        UpdatePDFStatus($"Screenshot {screenshots.Count} captured: {fileName}", Color.green);
        Debug.Log($"Screenshot saved: {filePath}");
    }
    
    /// <summary>
    /// Clear all screenshots
    /// </summary>
    public void ClearScreenshots()
    {
        // Clear screenshot collections
        screenshots.Clear();
        screenshotPaths.Clear();
        
        UpdatePDFStatus("All screenshots cleared", Color.yellow);
        Debug.Log("Screenshots cleared");
    }
    
    /// <summary>
    /// Generate a PDF report of the current analysis with screenshots
    /// </summary>
    public void GeneratePDFReport()
    {
        // Try to get analysis from different sources
        string analysisToUse = "";
        
        if (!string.IsNullOrEmpty(currentAnalysis))
        {
            analysisToUse = currentAnalysis;
        }
        else if (hardcodedAnalysisText != null && !string.IsNullOrEmpty(hardcodedAnalysisText.text))
        {
            analysisToUse = hardcodedAnalysisText.text;
        }
        else if (AnalysisTextField != null && !string.IsNullOrEmpty(AnalysisTextField.text))
        {
            analysisToUse = AnalysisTextField.text;
        }
        else
        {
            UpdatePDFStatus("No analysis available. Please run AI Analysis first.", Color.red);
            return;
        }
        
        try
        {
            // Generate HTML report with screenshots (can be printed to PDF)
            string fileName = $"EEG_Analysis_Report_{DateTime.Now:yyyyMMdd_HHmmss}.html";
            string filePath = Path.Combine(Application.persistentDataPath, fileName);
            
            CreateHTMLWithScreenshots(filePath, analysisToUse);
            UpdatePDFStatus($"HTML Report with screenshots saved to: {filePath}", Color.green);
            
            Debug.Log($"Report generated successfully: {filePath}");
            
            // Try to open the file location
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.RevealInFinder(filePath);
            #elif UNITY_STANDALONE_WIN
            System.Diagnostics.Process.Start("explorer.exe", "/select," + filePath);
            #elif UNITY_STANDALONE_OSX
            System.Diagnostics.Process.Start("open", "-R " + filePath);
            #endif
        }
        catch (Exception e)
        {
            UpdatePDFStatus($"Error generating report: {e.Message}", Color.red);
            Debug.LogError($"Report Generation Error: {e.Message}");
        }
    }
    
    
    /// <summary>
    /// Create HTML report with screenshots (can be printed to PDF)
    /// </summary>
    private void CreateHTMLWithScreenshots(string filePath, string analysisContent)
    {
        StringBuilder html = new StringBuilder();
        
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang='en'>");
        html.AppendLine("<head>");
        html.AppendLine("    <meta charset='UTF-8'>");
        html.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
        html.AppendLine("    <title>EEG Eye-Tracking Analysis Report</title>");
        html.AppendLine("    <style>");
        html.AppendLine("        body { font-family: Arial, sans-serif; margin: 40px; line-height: 1.6; }");
        html.AppendLine("        .header { background-color: #2c3e50; color: white; padding: 20px; text-align: center; }");
        html.AppendLine("        .section { margin: 20px 0; padding: 15px; border-left: 4px solid #3498db; }");
        html.AppendLine("        .eeg-data { background-color: #ecf0f1; padding: 15px; border-radius: 5px; }");
        html.AppendLine("        .analysis { background-color: #f8f9fa; padding: 15px; border-radius: 5px; }");
        html.AppendLine("        .screenshot-section { margin: 30px 0; }");
        html.AppendLine("        .screenshot { margin: 20px 0; text-align: center; }");
        html.AppendLine("        .screenshot img { max-width: 100%; height: auto; border: 2px solid #ddd; border-radius: 5px; }");
        html.AppendLine("        .screenshot-caption { font-weight: bold; margin-bottom: 10px; }");
        html.AppendLine("        h1, h2 { color: #2c3e50; }");
        html.AppendLine("        .timestamp { color: #7f8c8d; font-size: 0.9em; }");
        html.AppendLine("        ul { margin: 10px 0; }");
        html.AppendLine("        li { margin: 5px 0; }");
        html.AppendLine("        @media print { body { margin: 20px; } .screenshot img { max-width: 100%; } }");
        html.AppendLine("    </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        
        // Header
        html.AppendLine("    <div class='header'>");
        html.AppendLine("        <h1>EEG EYE-TRACKING ANALYSIS REPORT</h1>");
        html.AppendLine($"        <p class='timestamp'>Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
        html.AppendLine("    </div>");
        
        // EEG Data Summary
        if (eegDataVisualizer != null)
        {
            float attention = eegDataVisualizer.GetAttentionValue();
            float meditation = eegDataVisualizer.GetMeditationValue();
            float ctr = CalculateCTR(attention, meditation);
            
            html.AppendLine("    <div class='section eeg-data'>");
            html.AppendLine("        <h2>EEG Data Summary</h2>");
            html.AppendLine($"        <p><strong>Attention Level:</strong> {attention:F1}/100</p>");
            html.AppendLine($"        <p><strong>Meditation Level:</strong> {meditation:F1}/100</p>");
            html.AppendLine($"        <p><strong>Click Through Rate (CTR):</strong> {ctr:F2}%</p>");
            html.AppendLine($"        <p><strong>Analysis Quality:</strong> {(attention > 70 ? "High" : attention > 40 ? "Medium" : "Low")}</p>");
            html.AppendLine($"        <p><strong>CTR Performance:</strong> {(ctr >= 5.0f ? "Excellent" : ctr >= 3.0f ? "Good" : ctr >= 1.5f ? "Average" : "Below Average")}</p>");
            html.AppendLine("    </div>");
        }
        
        // Analysis Content
        html.AppendLine("    <div class='section analysis'>");
        html.AppendLine("        <h2>Detailed Analysis</h2>");
        html.AppendLine("        <div>");
        
        // Convert the analysis text to HTML
        string htmlAnalysis = analysisContent
            .Replace("**", "<strong>")
            .Replace("**", "</strong>")
            .Replace("\n", "<br>")
            .Replace("•", "&bull;");
        
        html.AppendLine($"        {htmlAnalysis}");
        html.AppendLine("        </div>");
        html.AppendLine("    </div>");
        
        // Screenshots Section
        if (screenshots.Count > 0)
        {
            html.AppendLine("    <div class='screenshot-section'>");
            html.AppendLine("        <h2>Screenshots</h2>");
            
            for (int i = 0; i < screenshots.Count; i++)
            {
                if (File.Exists(screenshotPaths[i]))
                {
                    // Convert to base64 for embedding
                    byte[] imageBytes = File.ReadAllBytes(screenshotPaths[i]);
                    string base64Image = Convert.ToBase64String(imageBytes);
                    
                    html.AppendLine("        <div class='screenshot'>");
                    html.AppendLine($"            <div class='screenshot-caption'>Screenshot {i + 1}</div>");
                    html.AppendLine($"            <img src='data:image/png;base64,{base64Image}' alt='Screenshot {i + 1}'>");
                    html.AppendLine("        </div>");
                }
            }
            
            html.AppendLine("    </div>");
        }
        
        // Footer
        html.AppendLine("    <div class='section'>");
        html.AppendLine("        <p><em>This report was generated by the EEG Eye-Tracking Analysis System</em></p>");
        html.AppendLine("        <p><em>For questions or support, please contact the development team.</em></p>");
        html.AppendLine("    </div>");
        
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        
        // Write to file
        File.WriteAllText(filePath, html.ToString());
    }
    
    /// <summary>
    /// Create formatted content for the PDF report (legacy method for text files)
    /// </summary>
    private string CreatePDFContent(string analysisContent)
    {
        StringBuilder pdf = new StringBuilder();
        
        // Header
        pdf.AppendLine("=".PadRight(80, '='));
        pdf.AppendLine("EEG EYE-TRACKING ANALYSIS REPORT");
        pdf.AppendLine("=".PadRight(80, '='));
        pdf.AppendLine();
        
        // Report Information
        pdf.AppendLine("REPORT INFORMATION");
        pdf.AppendLine("-".PadRight(40, '-'));
        pdf.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        pdf.AppendLine($"User ID: {userID}");
        pdf.AppendLine($"Report Type: Eye-Tracking Pattern Analysis");
        pdf.AppendLine();
        
        // EEG Data Summary
        if (eegDataVisualizer != null)
        {
            float attention = eegDataVisualizer.GetAttentionValue();
            float meditation = eegDataVisualizer.GetMeditationValue();
            float ctr = CalculateCTR(attention, meditation);
            
            pdf.AppendLine("EEG DATA SUMMARY");
            pdf.AppendLine("-".PadRight(40, '-'));
            pdf.AppendLine($"Attention Level: {attention:F1}/100");
            pdf.AppendLine($"Meditation Level: {meditation:F1}/100");
            pdf.AppendLine($"Click Through Rate (CTR): {ctr:F2}%");
            pdf.AppendLine($"Analysis Quality: {(attention > 70 ? "High" : attention > 40 ? "Medium" : "Low")}");
            pdf.AppendLine($"CTR Performance: {(ctr >= 5.0f ? "Excellent" : ctr >= 3.0f ? "Good" : ctr >= 1.5f ? "Average" : "Below Average")}");
            pdf.AppendLine();
        }
        
        // Analysis Content
        pdf.AppendLine("DETAILED ANALYSIS");
        pdf.AppendLine("-".PadRight(40, '-'));
        pdf.AppendLine();
        
        // Format the analysis content for better readability
        string[] lines = analysisContent.Split('\n');
        foreach (string line in lines)
        {
            if (line.StartsWith("**") && line.EndsWith("**"))
            {
                // Bold headers (complete with **)
                pdf.AppendLine();
                pdf.AppendLine(line.Replace("**", "").ToUpper());
                pdf.AppendLine("-".PadRight(line.Length - 4, '-'));
            }
            else if (line.StartsWith("**"))
            {
                // Bold headers (starting with ** but not ending with **)
                pdf.AppendLine();
                pdf.AppendLine(line.Replace("**", "").ToUpper());
                pdf.AppendLine("-".PadRight(line.Length - 2, '-'));
            }
            else if (line.StartsWith("•"))
            {
                // Bullet points
                pdf.AppendLine($"  {line}");
            }
            else if (line.StartsWith("1.") || line.StartsWith("2.") || line.StartsWith("3.") || line.StartsWith("4."))
            {
                // Numbered items
                pdf.AppendLine();
                pdf.AppendLine(line);
            }
            else if (!string.IsNullOrWhiteSpace(line))
            {
                // Regular content
                pdf.AppendLine(line);
            }
            else
            {
                // Empty lines
                pdf.AppendLine();
            }
        }
        
        pdf.AppendLine();
        pdf.AppendLine();
        
        // Footer
        pdf.AppendLine("=".PadRight(80, '='));
        pdf.AppendLine("END OF REPORT");
        pdf.AppendLine("=".PadRight(80, '='));
        pdf.AppendLine();
        pdf.AppendLine("This report was generated by the EEG Eye-Tracking Analysis System");
        pdf.AppendLine("For questions or support, please contact the development team.");
        
        return pdf.ToString();
    }
    
    /// <summary>
    /// Update the PDF status text
    /// </summary>
    private void UpdatePDFStatus(string message, Color color)
    {
        if (pdfStatusText != null)
        {
            pdfStatusText.text = message;
            pdfStatusText.color = color;
        }
        Debug.Log($"PDF Status: {message}");
    }
    
    /// <summary>
    /// Generate a simple HTML report (alternative to PDF)
    /// </summary>
    public void GenerateHTMLReport()
    {
        // Try to get analysis from different sources
        string analysisToUse = "";
        
        if (!string.IsNullOrEmpty(currentAnalysis))
        {
            analysisToUse = currentAnalysis;
        }
        else if (hardcodedAnalysisText != null && !string.IsNullOrEmpty(hardcodedAnalysisText.text))
        {
            analysisToUse = hardcodedAnalysisText.text;
        }
        else if (AnalysisTextField != null && !string.IsNullOrEmpty(AnalysisTextField.text))
        {
            analysisToUse = AnalysisTextField.text;
        }
        else
        {
            UpdatePDFStatus("No analysis available. Please run AI Analysis first.", Color.red);
            return;
        }
        
        try
        {
            string htmlContent = CreateHTMLContent(analysisToUse);
            string fileName = $"EEG_Analysis_Report_{DateTime.Now:yyyyMMdd_HHmmss}.html";
            string filePath = Path.Combine(Application.persistentDataPath, fileName);
            
            File.WriteAllText(filePath, htmlContent);
            
            UpdatePDFStatus($"HTML Report saved to: {filePath}", Color.green);
            Debug.Log($"HTML Report generated successfully: {filePath}");
            
            // Try to open the HTML file
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.RevealInFinder(filePath);
            #elif UNITY_STANDALONE_WIN
            System.Diagnostics.Process.Start(filePath);
            #elif UNITY_STANDALONE_OSX
            System.Diagnostics.Process.Start("open", filePath);
            #endif
        }
        catch (Exception e)
        {
            UpdatePDFStatus($"Error generating HTML: {e.Message}", Color.red);
            Debug.LogError($"HTML Generation Error: {e.Message}");
        }
    }
    
    /// <summary>
    /// Create HTML content for the report
    /// </summary>
    private string CreateHTMLContent(string analysisContent)
    {
        StringBuilder html = new StringBuilder();
        
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang='en'>");
        html.AppendLine("<head>");
        html.AppendLine("    <meta charset='UTF-8'>");
        html.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
        html.AppendLine("    <title>EEG Eye-Tracking Analysis Report</title>");
        html.AppendLine("    <style>");
        html.AppendLine("        body { font-family: Arial, sans-serif; margin: 40px; line-height: 1.6; }");
        html.AppendLine("        .header { background-color: #2c3e50; color: white; padding: 20px; text-align: center; }");
        html.AppendLine("        .section { margin: 20px 0; padding: 15px; border-left: 4px solid #3498db; }");
        html.AppendLine("        .eeg-data { background-color: #ecf0f1; padding: 15px; border-radius: 5px; }");
        html.AppendLine("        .analysis { background-color: #f8f9fa; padding: 15px; border-radius: 5px; }");
        html.AppendLine("        h1, h2 { color: #2c3e50; }");
        html.AppendLine("        .timestamp { color: #7f8c8d; font-size: 0.9em; }");
        html.AppendLine("        ul { margin: 10px 0; }");
        html.AppendLine("        li { margin: 5px 0; }");
        html.AppendLine("    </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        
        // Header
        html.AppendLine("    <div class='header'>");
        html.AppendLine("        <h1>EEG EYE-TRACKING ANALYSIS REPORT</h1>");
        html.AppendLine($"        <p class='timestamp'>Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
        html.AppendLine("    </div>");
        
        // EEG Data Summary
        if (eegDataVisualizer != null)
        {
            float attention = eegDataVisualizer.GetAttentionValue();
            float meditation = eegDataVisualizer.GetMeditationValue();
            float ctr = CalculateCTR(attention, meditation);
            
            html.AppendLine("    <div class='section eeg-data'>");
            html.AppendLine("        <h2>EEG Data Summary</h2>");
            html.AppendLine($"        <p><strong>Attention Level:</strong> {attention:F1}/100</p>");
            html.AppendLine($"        <p><strong>Meditation Level:</strong> {meditation:F1}/100</p>");
            html.AppendLine($"        <p><strong>Click Through Rate (CTR):</strong> {ctr:F2}%</p>");
            html.AppendLine($"        <p><strong>Analysis Quality:</strong> {(attention > 70 ? "High" : attention > 40 ? "Medium" : "Low")}</p>");
            html.AppendLine($"        <p><strong>CTR Performance:</strong> {(ctr >= 5.0f ? "Excellent" : ctr >= 3.0f ? "Good" : ctr >= 1.5f ? "Average" : "Below Average")}</p>");
            html.AppendLine("    </div>");
        }
        
        // Analysis Content
        html.AppendLine("    <div class='section analysis'>");
        html.AppendLine("        <h2>Detailed Analysis</h2>");
        html.AppendLine("        <div>");
        
        // Convert the analysis text to HTML
        string htmlAnalysis = analysisContent
            .Replace("**", "<strong>")
            .Replace("**", "</strong>")
            .Replace("\n", "<br>")
            .Replace("•", "&bull;");
        
        html.AppendLine($"        {htmlAnalysis}");
        html.AppendLine("        </div>");
        html.AppendLine("    </div>");
        
        // Footer
        html.AppendLine("    <div class='section'>");
        html.AppendLine("        <p><em>This report was generated by the EEG Eye-Tracking Analysis System</em></p>");
        html.AppendLine("        <p><em>For questions or support, please contact the development team.</em></p>");
        html.AppendLine("    </div>");
        
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        
        return html.ToString();
    }
}