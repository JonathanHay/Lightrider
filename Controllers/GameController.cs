// Top level controller for scene transitions, state management and UI triggers

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    // Start is called before the first frame update
    public int currentLevel;
    public int nextLevel;
    public GameObject player;
    public UIController UI;
    public Animator anim;

    private IEnumerator transitionCo;

    void Start()
    {
        transitionCo = WaitForAnim();
        PlayerState.State.Level = currentLevel;
        if (PlayerState.State.Level != 1)
        {
            anim.SetBool("fadeToBlack", false);
            anim.SetBool("fadeFromBlack", true);
            PlayerState.State.GameRunning = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        currentLevel = PlayerState.State.Level;
    }
    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log("Test");
        // this probably isn't needed
        if (collision.gameObject.tag == "Player")
        {
            switch (gameObject.tag)
            {
                case "Finish":
                    if (currentLevel == 3)
                    {
                        UI.gameOver(3);
                    }
                    else
                    {
                        PlayerState.State.GameRunning = false;
                        anim.SetBool("fadeFromBlack", false);
                        PlayerState.State.Level = nextLevel;
                        anim.SetBool("fadeToBlack", true);
                        StartCoroutine(transitionCo);
                    }
                    break;
            }
            
        }
    }

    private IEnumerator WaitForAnim()
    {
        yield return new WaitForSeconds(2.0f);
        SceneManager.LoadScene("Level_" + nextLevel);
    }
}
