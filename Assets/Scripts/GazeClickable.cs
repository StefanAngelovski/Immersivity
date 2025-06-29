using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GazeClickable : MonoBehaviour
{
    [Header("Visual Feedback")]
    public Color normalColor = Color.white;     
    public Color gazeColor = Color.yellow;
    public Color clickedColor = Color.green;
    
    [Header("Progress Indicator")]
    public bool showProgressIndicator = true;
    public GameObject progressIndicatorPrefab; // This will appear in the Inspector
    
    [Header("Scene Management")]
    public string sceneToLoad = ""; // Scene name to load when clicked
    public bool isExit = false; // Check this to exit the application instead
    
    [Header("Guide System")]
    public bool isGuide = false; // Check this to show a guide message instead
    public GameObject guideUI; // Link the Canvas or UI element to show
    
    [Header("Barrel System")]
    public bool isBarrel = false; // Check this if this object is a barrel
    public int barrelID = 1; // ID for this barrel (1, 3, or 5)
    public GameObject goldObject; // Gold object to show when sequence is successful
    public Color selectedBarrelColor = Color.green; // Color when barrel is selected in sequence
    
    [Header("Destruction")]
    public bool isDestroyable = false; // Check this to destroy object when gazed at
    
    private Renderer objectRenderer;
    private Color originalColor;
    private bool isGazedAt = false;
    private bool isSelectedInSequence = false; // Track if this barrel is selected in the sequence
    
    // Static shared references
    private static GameObject sharedProgressIndicator;
    private static Image sharedProgressImage;
    
    // Barrel system
    private static List<int> selectedBarrels = new List<int>();
    private static List<int> correctBarrelSequence = new List<int> { 1, 3, 5 }; // Required barrel sequence in order
    private static GameObject sharedGoldObject; // Shared gold object reference
    private static List<GazeClickable> selectedBarrelObjects = new List<GazeClickable>(); // Track selected barrel objects for color changes

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
            
            // For barrel objects, ensure we have a more vibrant selected color
            if (isBarrel && selectedBarrelColor == Color.green)
            {
                selectedBarrelColor = new Color(0f, 1f, 0f, 1f); // Bright green
            }
        }
        
        // Initialize the shared progress indicator if needed
        if (sharedProgressIndicator == null && progressIndicatorPrefab != null)
        {
            sharedProgressIndicator = progressIndicatorPrefab;
            sharedProgressImage = sharedProgressIndicator.GetComponent<Image>();
            sharedProgressIndicator.SetActive(false);
        }
        
        // Initialize the shared gold object if this is a barrel with a gold object
        if (isBarrel && goldObject != null && sharedGoldObject == null)
        {
            sharedGoldObject = goldObject;
            sharedGoldObject.SetActive(false); // Hide it initially
        }
    }

    public void OnGazeEnter()
    {
        isGazedAt = true;
        Debug.Log("Gaze Enter: " + gameObject.name + (isBarrel ? " (Barrel ID: " + barrelID + ")" : ""));
            
        if (objectRenderer != null)
            objectRenderer.material.color = gazeColor;
        
        // Show the shared progress indicator
        if (sharedProgressIndicator != null && showProgressIndicator)
        {
            sharedProgressIndicator.SetActive(true);
            if (sharedProgressImage != null)
                sharedProgressImage.fillAmount = 0f;
        }
    }

    public void UpdateGazeProgress(float progress)
    {
        if (sharedProgressImage != null)
            sharedProgressImage.fillAmount = Mathf.Clamp01(progress);
    }

    public void OnGazeExit()
    {
        isGazedAt = false;
        Debug.Log("Gaze Exit: " + gameObject.name + (isBarrel ? " (Barrel ID: " + barrelID + ")" : ""));
        
        if (objectRenderer != null)
        {
            // If this barrel is selected in sequence, keep the selected color
            // Otherwise, return to original color
            if (isBarrel && isSelectedInSequence)
                objectRenderer.material.color = selectedBarrelColor;
            else
                objectRenderer.material.color = originalColor;
        }
        
        if (sharedProgressIndicator != null)
        {
            sharedProgressIndicator.SetActive(false);
            if (sharedProgressImage != null)
                sharedProgressImage.fillAmount = 0f;
        }
    }

    public void OnGazeClick()
    {
        Debug.Log("Gaze Clicked: " + gameObject.name + (isBarrel ? " (Barrel ID: " + barrelID + ")" : ""));
        
        if (objectRenderer != null)
            objectRenderer.material.color = clickedColor;
        
        if (sharedProgressIndicator != null)
        {
            sharedProgressIndicator.SetActive(false);
            if (sharedProgressImage != null)
                sharedProgressImage.fillAmount = 0f;
        }
        
        PerformInteraction();
    }

    private void PerformInteraction()
    {
        // Check if this object should be destroyed
        if (isDestroyable)
        {
            Debug.Log("Destroying object: " + gameObject.name);
            Destroy(gameObject);
            return;
        }
        
        // Check if this should exit the application
        if (isExit)
        {
            Debug.Log("Exiting application...");
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
            return;
        }
        
        // Check if this should show a guide
        if (isGuide)
        {
            if (guideUI != null)
            {
                Debug.Log("Showing guide UI for: " + gameObject.name);
                guideUI.SetActive(true);
            }
            else
            {
                Debug.LogWarning("Guide UI not assigned for " + gameObject.name);
            }
            return;
        }
        
        // Check if this is a barrel
        if (isBarrel)
        {
            HandleBarrelInteraction();
            return;
        }
        
        // Check if a scene name is specified
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log("Loading scene: " + sceneToLoad);
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("No scene specified in sceneToLoad field for " + gameObject.name);
        }
    }
    
    private void HandleBarrelInteraction()
    {
        // Check if this barrel ID is valid (1, 3, or 5)
        if (barrelID == 1 || barrelID == 3 || barrelID == 5)
        {
            // Check if this is the next expected barrel in the sequence
            int expectedNext = selectedBarrels.Count < correctBarrelSequence.Count ? correctBarrelSequence[selectedBarrels.Count] : -1;
            
            if (barrelID == expectedNext)
            {
                // Correct barrel in sequence
                selectedBarrels.Add(barrelID);
                selectedBarrelObjects.Add(this);
                
                // Mark this barrel as selected and change color
                isSelectedInSequence = true;
                if (objectRenderer != null)
                {
                    // Try different material properties for better visibility
                    Material mat = objectRenderer.material;
                    mat.color = selectedBarrelColor;
                    
                    // Also try setting emission if available
                    if (mat.HasProperty("_EmissionColor"))
                    {
                        mat.SetColor("_EmissionColor", selectedBarrelColor * 0.3f);
                        mat.EnableKeyword("_EMISSION");
                    }
                    
                    Debug.Log("Changed barrel " + barrelID + " color to: " + selectedBarrelColor);
                }
                
                Debug.Log("Selected barrel " + barrelID + " (Step " + selectedBarrels.Count + "/" + correctBarrelSequence.Count + "). Current sequence: [" + string.Join(", ", selectedBarrels) + "]");
                CheckBarrelSequence();
            }
            else
            {
                // Wrong barrel or out of order - reset the sequence and colors
                if (selectedBarrels.Count > 0)
                {
                    Debug.Log("Wrong barrel " + barrelID + " selected! Expected barrel " + expectedNext + ". Resetting sequence.");
                    ResetBarrelColors();
                    selectedBarrels.Clear();
                    selectedBarrelObjects.Clear();
                }
                
                // If they selected barrel 1, start the sequence
                if (barrelID == 1)
                {
                    selectedBarrels.Add(barrelID);
                    selectedBarrelObjects.Add(this);
                    
                    // Mark this barrel as selected and change color
                    isSelectedInSequence = true;
                    if (objectRenderer != null)
                    {
                        // Try different material properties for better visibility
                        Material mat = objectRenderer.material;
                        mat.color = selectedBarrelColor;
                        
                        // Also try setting emission if available
                        if (mat.HasProperty("_EmissionColor"))
                        {
                            mat.SetColor("_EmissionColor", selectedBarrelColor * 0.3f);
                            mat.EnableKeyword("_EMISSION");
                        }
                        
                        Debug.Log("Started sequence - changed barrel " + barrelID + " color to: " + selectedBarrelColor);
                    }
                    
                    Debug.Log("Started new sequence with barrel 1. Current sequence: [" + string.Join(", ", selectedBarrels) + "]");
                }
            }
        }
        else
        {
            Debug.LogWarning("Invalid barrel ID: " + barrelID + ". Only IDs 1, 3, and 5 are allowed.");
        }
    }
    
    private void CheckBarrelSequence()
    {
        // Check if the sequence is complete
        if (selectedBarrels.Count == correctBarrelSequence.Count)
        {
            // Verify the sequence is correct (should always be true with our new logic, but double-check)
            bool isCorrectSequence = true;
            for (int i = 0; i < selectedBarrels.Count; i++)
            {
                if (selectedBarrels[i] != correctBarrelSequence[i])
                {
                    isCorrectSequence = false;
                    break;
                }
            }

            if (isCorrectSequence)
            {
                Debug.Log("SUCCESS! Correct barrel sequence completed: [" + string.Join(", ", selectedBarrels) + "]");
                
                // Show the gold object if available
                if (sharedGoldObject != null)
                {
                    sharedGoldObject.SetActive(true);
                    Debug.Log("Gold object revealed!");
                }
                else
                {
                    Debug.LogWarning("No gold object assigned to show for success!");
                }
                
                // Reset for next attempt if needed
                selectedBarrels.Clear();
                
                // Reset barrel colors after success
                ResetBarrelColors();
                selectedBarrelObjects.Clear();
                selectedBarrelObjects.Clear();
                
            }
        }
    }
    
    private void ResetBarrelColors()
    {
        // Reset all selected barrel colors back to their original colors
        foreach (GazeClickable barrel in selectedBarrelObjects)
        {
            if (barrel != null && barrel.objectRenderer != null)
            {
                barrel.isSelectedInSequence = false;
                Material mat = barrel.objectRenderer.material;
                mat.color = barrel.originalColor;
                
                // Reset emission if it was set
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor("_EmissionColor", Color.black);
                    mat.DisableKeyword("_EMISSION");
                }
                
                Debug.Log("Reset barrel " + barrel.barrelID + " color to original: " + barrel.originalColor);
            }
        }
    }

    // Keep editor testing if needed
#if UNITY_EDITOR
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            float rotX = Input.GetAxis("Mouse Y") * 5f;
            float rotY = Input.GetAxis("Mouse X") * 5f;
        
            Camera.main.transform.Rotate(Vector3.left, rotX);
            Camera.main.transform.Rotate(Vector3.up, rotY);
        }
    }
#endif
}
