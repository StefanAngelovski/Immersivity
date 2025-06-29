using UnityEngine;

public class NearRiddleSound : MonoBehaviour
{
    [Header("Sound Settings")]
    public AudioClip soundClip; // Drag your audio clip here in the inspector
    public float volume = 1.0f; // Volume of the sound (0-1)
    public bool playOnce = true; // Play only once when triggered
    
    private AudioSource audioSource;
    private bool hasPlayed = false; // Track if sound has already been played

    void Start()
    {
        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure the AudioSource
        audioSource.clip = soundClip;
        audioSource.volume = volume;
        audioSource.playOnAwake = false; // Don't play automatically
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the object entering has a "Player" tag or if it's the main camera
        if (other.CompareTag("Player") || other.name.Contains("Camera") || other.CompareTag("MainCamera"))
        {
            PlaySound();
        }
    }
    
    private void PlaySound()
    {
        // Check if we should play the sound
        if (soundClip != null && audioSource != null)
        {
            // If playOnce is true, only play if it hasn't been played yet
            if (!playOnce || !hasPlayed)
            {
                audioSource.Play();
                hasPlayed = true;
                Debug.Log("Playing riddle sound: " + soundClip.name);
            }
        }
        else
        {
            Debug.LogWarning("No sound clip assigned to " + gameObject.name);
        }
    }
    
    // Optional: Reset the sound so it can be played again
    public void ResetSound()
    {
        hasPlayed = false;
    }
}
