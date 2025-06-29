using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRLookWalk : MonoBehaviour
{
    public Transform vrCamera;
    public float toggleAngle = 30.0f;
    public float speed = 3.0f;
    private bool moveForward;
    private CharacterController cc;
    //
    //public AudioClip audioClip;
    private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
        //cc = GetComponentInParent<CharacterController>();
        cc = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        //audioSource.clip = audioClip;
    }

    // Update is called once per frame
    void Update()
    {
        if(vrCamera.eulerAngles.x >= toggleAngle && vrCamera.eulerAngles.x < 90.0f)
        {
            moveForward = true;
        }
        else
        {
            moveForward = false;
        }

        if (moveForward)
        {
            Vector3 forward = vrCamera.TransformDirection(Vector3.forward);
            cc.SimpleMove(forward * speed);
        }

        if(moveForward && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
        if (!moveForward)
        {
            audioSource.Stop();
        }
    }
}
