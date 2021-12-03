// Player resource management state controller 

using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(InputManager))]
public class LifeSupportController : MonoBehaviour
{
    // Component and script references 
    public UIController UI;
    public InputManager input;
    public GameObject Player;
    public List<HDAdditionalLightData> Lights = new List<HDAdditionalLightData>();
    public List<GameObject> Windows = new List<GameObject>();
    public List<GameObject> OverheatEffects = new List<GameObject>();
    public List<GameObject> Tanks;

    private List<float> initialIntensity;
    private int frameCounter = 1;

    // Global resource states
    private int Oxygen = PlayerState.Resources.Oxygen; // This will override the default starting oxygen of 100
    private int Heat = PlayerState.Resources.Heat; 
    private int Power = PlayerState.Resources.Power; 

    // Resource balance params
    public int DrainInterval = 25; 
    public int MaxPower = 1000;
    public int MaxHeat = 1000;
    public int MaxOverheat = 1500;
    public int MaxOxygen = 400;
    public int OxygenDecay = 2;
    public int HeatDecay = 2;
    public int OverheatDecay = 5;
    public int BoostOverheatRate = 50;
    public int ThrustOverheatRate = 100;
    public float HeatGenerationFactor = 5f;
    public float MinGenerationSpeed = 10f;
    public float MinThrustSpeed = 4f;
    public float StartPercentage = 100f;
    public int SapRate = 10;
    public bool Invincible = false;

    // Color params
    public Color OverheatColor = Color.red;
    public Color HotColor = Color.red;
    public Color ColdColor = Color.blue;

    public Color FullColor = Color.red;
    public Color EmptyColor = Color.white;

    private MaterialPropertyBlock _propBlock;
   
    void Start()
    {
        _propBlock = new MaterialPropertyBlock();
        initialIntensity = new List<float>();

        foreach(HDAdditionalLightData light in Lights)
        {
            initialIntensity.Add(light.intensity);
        }

        //PlayerState.Resources.Oxygen = Oxygen; // This will override the default starting oxygen of 100
        //PlayerState.Resources.Heat = (int)(MaxHeat * StartPercentage / 100); 
        //PlayerState.Resources.Power = Power; 
    }

    void Update()
    {
        DrainInterval = PlayerState.State.easyMode ? 100 : 25;
        Oxygen = PlayerState.Resources.Oxygen;
        Heat = PlayerState.Resources.Heat;
        Power = PlayerState.Resources.Power;

        // Update resource state and update player health visually 
        if(PlayerState.State.GameRunning && PlayerState.State.Alive)
        {
            frameCounter++;
            if (frameCounter % DrainInterval == 0f)
            {
                frameCounter = 1;

                if(Oxygen - OxygenDecay > 0)
                {
                    Oxygen -= OxygenDecay;
                }
                else{
                    Oxygen = 0;
                }

                if(Power - 5 > 0)
                {
                    Power -= 10;
                }
                else{
                    Power = 0;
                }
                
                if(Heat - HeatDecay > 0)
                {
                    Heat -= HeatDecay; 
                }
                else
                {
                    Heat = 0;
                }
                

                CarController carController = Player.GetComponent<CarController>();
                Rigidbody rb = carController.RB;
                var vel = rb.velocity;
                var velMag = vel.magnitude;

                int generativeForce = (int)Mathf.Floor(2 * HeatGenerationFactor * (velMag - MinGenerationSpeed));
        
                if(Heat + generativeForce < 0)
                {
                    Heat = 0;
                }
                else if(velMag > MinGenerationSpeed && input.getBoost())
                {
                    Debug.Log("overheat");
                    if(Heat + BoostOverheatRate <= MaxOverheat)
                    {
                        Heat += BoostOverheatRate;
                    }
                    else
                    {
                        Heat = MaxOverheat;
                    }
                    
                }
                else if(velMag > MinThrustSpeed && input.getSpace())
                {
                    Debug.Log("overheat");
                    if(Heat + ThrustOverheatRate <= MaxOverheat)
                    {
                        Heat += ThrustOverheatRate;
                    }
                    else
                    {
                        Heat = MaxOverheat;
                    }
                    
                }
                else if(Heat > MaxHeat && Heat < MaxOverheat)
                {
                    Debug.Log("decaying");
                    Heat -= OverheatDecay;
                }
                else if(Heat + generativeForce < MaxHeat)
                {
                    Heat += generativeForce;
                }
                else
                {
                    Heat = MaxHeat;  
                }
                

                PlayerState.Resources.Power = Power;
                PlayerState.Resources.Oxygen = Oxygen;
                PlayerState.Resources.Heat = Heat;
                UI.UpdateResourceValues();

                if(Heat > MaxHeat)
                {
                    Player.GetComponent<Renderer>().material.color = Color.Lerp(HotColor, OverheatColor, (float)(Heat - MaxHeat) / (float)(MaxOverheat - MaxHeat));
                }
                else
                {
                    Player.GetComponent<Renderer>().material.color = Color.Lerp(ColdColor, HotColor, (float)Heat / (float)MaxHeat);
                }

                float positionRatio = 1f / (float)Tanks.Count;
                for(int i = 0; i < Tanks.Count; i++)
                {
                    float rawFullness = (float)Oxygen / (float)MaxOxygen;
                    float positionFactor = (float)(i + 1);
                    float fullness = rawFullness / (positionRatio * positionFactor);
                    fullness = Mathf.Clamp(fullness, 0f, 1f);
                    Tanks[i].GetComponent<Renderer>().material.color =  Color.Lerp(EmptyColor, FullColor, fullness);
                }

                float intensityPercentage = (float)Power / (float)MaxPower;
                for(int i = 0; i < Lights.Count; i++)
                {
                    Lights[i].intensity = initialIntensity[i] * intensityPercentage;
                }  

                
                for(int i = 0; i < Windows.Count; i++)
                {
                    emissionValueSingular(new Color(255,255,255), 0.003f * intensityPercentage + 0.0005f, Windows[i]);
                }
                    
                if(!Invincible && PlayerState.State.Alive)
                {
                    gameOverCheck();
                } 
            }
        }
        

    }

    // Resource ingestion for use by pickup manager
    public void IngestResource(PlayerState.ResourceType type, int value)
    {
        switch (type)
        {
            case PlayerState.ResourceType.Oxygen:
                if(Oxygen + value < MaxOxygen)
                {
                    Oxygen += value;
                }
                else
                {
                    Oxygen = MaxOxygen;
                }
                
                PlayerState.Resources.Oxygen = Oxygen;
                UI.UpdateResourceValues();
                break;

            case PlayerState.ResourceType.Power:
                if(Power + value < MaxPower)
                {
                    Power += value;
                }
                else
                {
                    Power = MaxPower;
                }
                
                PlayerState.Resources.Power = Power;
                UI.UpdateResourceValues();
                break;

            case PlayerState.ResourceType.Heat:
                Debug.Log("Ingested!");
                if(Heat + value < MaxHeat)
                {
                    Heat += value;
                }
                else
                {
                    Heat = MaxHeat;
                }
                
                PlayerState.Resources.Heat = Heat;
                UI.UpdateResourceValues();
                break;
            
        }
        
    }

    // Death condition control
    public void Death(int deathType)
    {
        switch(deathType)
        {
            case 0:
                UI.gameOver(5);
                break;
            case 1:
                UI.gameOver(6);
                break;
            case 2:
                Player.GetComponent<Renderer>().material.color = ColdColor;
                Heat = 0;
                PlayerState.Resources.Heat = 0;
                UI.gameOver(7);
                break;
            default:
                break;
        }
    }

    // Death condition check
    private void gameOverCheck()
    {
        if (PlayerState.Resources.Oxygen <= 0)
        {
            UI.gameOver(0);
        }
        else if (PlayerState.Resources.Heat <= 0)
        {
            UI.gameOver(1);
        }
        else if (PlayerState.Resources.Heat >= MaxOverheat)
        {
            if (OverheatEffects != null && OverheatEffects.Count > 0)
            {
                foreach(GameObject effect in OverheatEffects)
                {
                    //effect.SetActive(true);
                    effect.GetComponent<VisualEffect>().Play();
                }
            }
            
            UI.gameOver(8);
        }
        else if (PlayerState.Resources.Power <= 0)
        {
            UI.gameOver(4);
        }
    }

    // Visual power for windows
    private void emissionValueSingular(Color newColor, float strength, GameObject target)
    {
        _propBlock = new MaterialPropertyBlock();
        var targetRenderer = target.GetComponent<Renderer>();
        if (targetRenderer != null)
        {
            targetRenderer.GetPropertyBlock(_propBlock, 0);

            _propBlock.SetColor("_EmissiveColor", newColor * Mathf.Lerp(0f, 2f, strength));
            targetRenderer.SetPropertyBlock(_propBlock, 0);
        }
    }
}
