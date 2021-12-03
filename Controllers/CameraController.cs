using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This script manages 

public class CameraController : MonoBehaviour
{
    public GameObject focus; // Camera focal point
    public float Stiffness = 1f; // Smoothing factor
    public bool Smoothing = true; // Smoothing enabled

    // Constant offsets
    public float Height = 100f;
    public float XOffset = 200f;
    public float ZOffset = 130f;

    void Start(){

    }

    // Update is called once per frame
    void Update()
    {
        float smoothedPosition;
        
        if(Smoothing)
        {
            smoothedPosition = Stiffness * Time.deltaTime;
        }
        else{
            smoothedPosition = 1;
        }

        transform.position = Vector3.Lerp(transform.position, new Vector3(focus.transform.position.x - XOffset, focus.transform.position.y + startHeight, focus.transform.position.z - ZOffset), smoothedPosition);
    }
}
