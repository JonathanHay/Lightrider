using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Wrapper for player control inputs

public class InputManager : MonoBehaviour
{
    public UIController UI;
    public Transform relativePos;

    // Input states
    private float throttle;
    private float steer;
    private bool boost;
    private bool space;
    private float rotationSpeed = 2;
    private float mouseX;
    private float mouseY;
    
    // Update is called once per frame
    void Start() {
        boost = false;
    }

    void Update()
    {
        throttle = Input.GetAxis("Vertical");
        steer = Input.GetAxis("Horizontal");

        if (Input.GetKey(KeyCode.LeftShift))
        {
            boost = true;
        }
        else{
            boost = false;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            space = true;
            UI.newGame(0);
        }
        else
        {
            space = false;
        }

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    // late update as to not interfere with physics, possibly unnecessary 
    void LateUpdate() {
        CamControl();
    }

    void CamControl()
    {
        mouseX += Input.GetAxis("Mouse X") * rotationSpeed;
        mouseY -= Input.GetAxis("Mouse Y") * rotationSpeed;
        mouseY = Mathf.Clamp(mouseY, -15, 60);
    }

    public float getThrottle(){
        return throttle;
    }

    public bool getBoost(){
        return boost;
    }

    public float getSteer(){
        return steer;
    }

    public bool getSpace(){
        return space;
    }
}
