using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(InputManager))]
public class CarController : MonoBehaviour
{
    public InputManager input;

    // Component references
    public List<TrailRenderer> Trails;
    public TrailRenderer ThrustTrail;
    public List<WheelCollider> throttleWheels;
    public List<WheelCollider> steerWheels;
    public List<WheelCollider> allWheels;
    public List<GameObject> steerWheelMeshes;
    public List<GameObject> meshes;
    public Rigidbody RB;

    // Vehicle performance params
    public float EnginePower;
    public float RocketForce = 2000f;
    public float BoostRatio;
    public float maxTurn = 10f;
    public float GroundThreshold = 3f;
    public float RollTorque = 1000f;
    
    // Grounded states
    private float distToGround;
    private bool grounded;
    
    // Visual effect states
    private bool endingTrail;
    private float maxTime;
    private float currentTime;
    private float timeElapsed;
    private float fadeTime = 1f;
    
    void Start()
    {
        // Initialize vehicle height to ground offset
        distToGround = GetComponent<CapsuleCollider>().bounds.extents.y;
        grounded = true;

        // Initialize trail states
        if(Trails.Count > 0)
        {
            maxTime = Trails[0].time;
            currentTime = maxTime;
            timeElapsed = 0f;
        }
       
    }
    void Update()
    {
        // Fade trail gradually
        if(endingTrail)
        {
            
            if(timeElapsed <= fadeTime)
            {
                timeElapsed += Time.deltaTime;
                currentTime = Mathf.Lerp(maxTime, 0, timeElapsed / fadeTime);
            }
            else
            {
                endingTrail = false;
                foreach(TrailRenderer trail in Trails)
                {
                    trail.emitting = false;
                }
            }
        }

        // Update trail times
        foreach(TrailRenderer trail in Trails)
        {
            trail.time = currentTime;
        }

        // Emit boost trail on boost and initiate fade on release
        if(input.getBoost() == true)
        {
            currentTime = maxTime;
            endingTrail = false;

            foreach(TrailRenderer trail in Trails)
            {
                trail.emitting = true;
            }
        }
        else if (!endingTrail)
        {
            endingTrail = true;
            timeElapsed = 0f;
        }

        // Emit thrust trail
        if(input.getSpace())
        {
            ThrustTrail.emitting = true;
        }
        else
        {
            ThrustTrail.emitting = false;
        }
            

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(PlayerState.State.Alive)
        {   
            // Determine if player is grounded to toggle roll and steer controls on x axis input
            if ((Vector3.Dot(transform.up, Vector3.down) > 0) && (Physics.Raycast(RB.position, transform.up, distToGround + 4f)) && !input.getSpace())
            {
                RB.AddTorque(transform.forward * RollTorque * input.getSteer() * 5f);
            }
            else if(grounded && !input.getSpace())
            {
                // Update throttle and steering angle for wheel colliders
                foreach(WheelCollider wheel in throttleWheels)
                {
                    float boost;
                    if(input.getBoost() == true){
                        boost = BoostRatio;
                    }
                    else
                    {
                        boost = 1f;
                    }

                    wheel.motorTorque = EnginePower * input.getThrottle() * Time.deltaTime * boost * 100;
                }
                const float THRESH = 0.01f;
                if(input.getSteer() < -THRESH || input.getSteer() > THRESH)
                {
                    foreach (WheelCollider wheel in steerWheels)
                    {
                        wheel.steerAngle = maxTurn * input.getSteer();
                    }
                }
            }
            else
            {
                // Roll vehicle
                RB.AddTorque(-transform.forward * RollTorque * input.getSteer());
                RB.AddTorque(transform.right * RollTorque * input.getThrottle());
            }

            // Apply thrust
            if(input.getSpace())
            {
                RB.AddForce(transform.up * RocketForce);
            }
            
            // Determine if vehicle is grounded
            grounded = Physics.Raycast(RB.position, -Vector3.up, distToGround + GroundThreshold);
            Debug.DrawRay(RB.position, -Vector3.up * (distToGround + GroundThreshold), Color.green);
        }
    }
}
