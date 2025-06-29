using UnityEngine;

public class GazeInteraction : MonoBehaviour
{
    [Header("Gaze Settings")]
    public float gazeTime = 3f; // Seconds to trigger
    public LayerMask gazeLayerMask = -1; // What layers to check
    public float maxGazeDistance = 10f; // Maximum gaze distance
    
    [Header("Debug")]
    public bool showDebugRay = true;
    
    private float timer = 0f;
    private GameObject currentObject;
    private GazeClickable currentClickable;

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // Debug ray visualization
        if (showDebugRay)
        {
            Debug.DrawRay(transform.position, transform.forward * maxGazeDistance, Color.red);
        }

        if (Physics.Raycast(ray, out hit, maxGazeDistance, gazeLayerMask, QueryTriggerInteraction.Collide))
        {
            GameObject hitObject = hit.collider.gameObject;
            GazeClickable clickable = hitObject.GetComponent<GazeClickable>();

            if (clickable != null)
            {
                if (hitObject == currentObject)
                {
                    timer += Time.deltaTime;
                    
                    // Update progress on the clickable object
                    clickable.UpdateGazeProgress(timer / gazeTime);

                    if (timer >= gazeTime)
                    {
                        // Trigger interaction
                        clickable.OnGazeClick();
                        ResetGaze();
                    }
                }
                else
                {
                    // Reset previous object
                    if (currentClickable != null)
                    {
                        currentClickable.OnGazeExit();
                    }
                    
                    // Set new object
                    currentObject = hitObject;
                    currentClickable = clickable;
                    timer = 0f;
                    clickable.OnGazeEnter();
                }
            }
            else
            {
                ResetGaze();
            }
        }
        else
        {
            ResetGaze();
        }
    }

    void ResetGaze()
    {
        if (currentClickable != null)
        {
            currentClickable.OnGazeExit();
        }
        
        currentObject = null;
        currentClickable = null;
        timer = 0f;
    }
}
