// Manager for player-pickup interactions

using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine;

public class PickupManager : MonoBehaviour
{
    // Component and script references
    public LifeSupportController lifeSupport; // this references script attached to player car
    public List<HDAdditionalLightData> Lights = new List<HDAdditionalLightData>();
    public List<GameObject> EmmissiveMeshes = new List<GameObject>();
    public GameObject AltState;
    public AudioSource triggerSound;
    public AnimationCurve BobbingCurve;

    // Pickup params
    public int Value = 100; // building functionality to allow different values of pickups
    public bool enableBobbing = true;
    public bool VisualPower = true;

    // Internal states for pickup visuals including crystal sap state
    private List<float> initialIntensity;
    private float testInitialIntensity;
    private float yInitial;
    private bool sapping = false;
    private int sapIntervalCounter = 0;
    private int sapInterval = 10;
    private int initialValue;
    private MaterialPropertyBlock _propBlock;
    private bool heating = false;
    
    public void Start(){
            _propBlock = new MaterialPropertyBlock();
            yInitial = transform.position.y; 
            initialValue = Value;
            initialIntensity = new List<float>();
            
            if (gameObject.tag == "Power" && VisualPower)
            {           
                foreach(HDAdditionalLightData light in Lights)
                {
                    initialIntensity.Add(light.intensity);
                }
            }         
    }

    void FixedUpdate()
    {
        sapIntervalCounter++;
        if (sapIntervalCounter % sapInterval == 0)
        {
            sapIntervalCounter = 0;

            // Passive heat generation
            if(heating)
            {
                Debug.Log("Ingesting...");
                lifeSupport.IngestResource(PlayerState.ResourceType.Heat, Value);
            }
            
            // Light sapping
            if (sapping && Value - lifeSupport.SapRate > 0)
            {
                lifeSupport.IngestResource(PlayerState.ResourceType.Power, lifeSupport.SapRate);
                Value -= lifeSupport.SapRate;
            } 
            else if (sapping)
            {
                lifeSupport.IngestResource(PlayerState.ResourceType.Power, Value);
                Value = 0;
            }
        }    
    }

    void Update()
    {
        //GetComponent<Renderer>().material.color = Color.black; // This doesn't work and I don't know why
        //GetComponent<Renderer>().material.SetColor("_BaseColor", Color.black); // This doesn't work and I don't know why
        //GetComponent<Renderer>().material.SetColor("_EmissionColor", Color.black); // This doesn't work and I don't know why

        if (enableBobbing) transform.position = new Vector3(transform.position.x, yInitial + BobbingCurve.Evaluate((Time.time % BobbingCurve.length)), transform.position.z);

        if (gameObject.tag == "Power" && VisualPower)
        {
            float intensityPercentage = (float)Value / (float)initialValue;

            emissionValue(new Color(255,210,170), 0.006f * intensityPercentage + 0.001f);
            
            for(int i = 0; i < Lights.Count; i++)
            {
                Lights[i].intensity = initialIntensity[i] * Mathf.Clamp(intensityPercentage + 0.2f, 0, 1);
            }  
        }
    }

    void OnTriggerEnter(Collider collision)
    {
        //gameObject.GetComponent<CapsuleCollider>().enabled = false;

        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("Enter " + gameObject.tag + "!");
            switch (gameObject.tag)
            {
                case "Oxygen":
                    lifeSupport.IngestResource(PlayerState.ResourceType.Oxygen, Value);
                    triggerSound.Play();
                    destroySelf();
                    break;
                case "Power":
                    sapping = true;
                    break;
                case "Lost":
                    lifeSupport.Death(0);
                    break;
                case "Fall":
                    lifeSupport.Death(1);
                    break;
                case "Heat":
                    Debug.Log("Heating.");
                    heating = true;
                    break;
                 case "Snowman":
                    Debug.Log("Stinky.");
                    if(triggerSound != null)
                    {
                        triggerSound.Play();
                    }
                    lifeSupport.Death(2);
                    break;
            }

            if(AltState != null)
            {
                AltState.SetActive(true);
            }

        }

    }

    void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("Exit!");
            switch (gameObject.tag)
            {
                case "Power":
                    sapping = false;
                    break;
                
                case "Heat":
                    heating = false;
                    break;
            }
            
        }
    }

    private void destroySelf()
    {
        Destroy(gameObject);
    }

    private void emissionValue(Color newColor, float strength)
    {
        foreach (Transform item in transform)
        {
            _propBlock = new MaterialPropertyBlock();
            var targetRenderer = item.GetComponent<Renderer>();
            if (targetRenderer != null)
            {
                targetRenderer.GetPropertyBlock(_propBlock, 0);

                _propBlock.SetColor("_EmissiveColor", newColor * Mathf.Lerp(0f, 2f, strength));
                targetRenderer.SetPropertyBlock(_propBlock, 0);
            }
        }

    }
}
