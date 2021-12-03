using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    // Start is called before the first frame update

    // UI elements
    public Text oxygenIn;
    public Text heatIn;
    public Text powerIn;
    public Text scoreIn;
    public GameObject gameOverScreen;
    public GameObject mainMenuScreen;
    public Text GameOverText;
    public Text oxygen;
    public Text heat;
    public Text power;
    public Text score;
    public Text highscore;
    public bool PauseOnStart;
    public bool DisableNewGame = true;
    public GameObject esayMode;
    public Text highscoreIn;
    public Text highscoreDisplay;

    private int locked = 0;
    private bool newLocked = false;
    void Start()
    {
        if(PauseOnStart)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1f;
            PlayerState.State.Alive = true;  
            PlayerState.State.GameRunning = true;
            newLocked = true;
        }


    }

    // Update is called once per frame
    public void UpdateResourceValues()
    {
        oxygenIn.text = PlayerState.Resources.Oxygen.ToString();
        heatIn.text = PlayerState.Resources.Heat.ToString();
        powerIn.text = PlayerState.Resources.Power.ToString();
    }

    /*
    Conditions:
        0 = over due to loss of oxygen
        1 = over due to loss of heat
        2 = fell out of map
        3 = legit victory
        4 = over due to loss of power
        5 = over due to out of bounds
        6 = fell in hole
    */
    public void gameOver(int condition)
    {
        //PlayerPrefs.DeleteAll(); //FOR DEBUG ONLY, NOT FOR FINAL GAME, if you want to check if highscore works, run this line of code for one run and it will clear your highscore.
        if (locked == 0 && newLocked)
        {
            Time.timeScale = 0.2f;
            PlayerState.State.easyMode = false;
            PlayerState.State.Alive = false;
            locked = 1;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            //remove other elements of UI
            power.enabled = false;
            heat.enabled = false;
            oxygen.enabled = false;
            oxygenIn.enabled = false;
            powerIn.enabled = false;
            heatIn.enabled = false;
            highscore.enabled = false;

            // Determine condition
            gameOverScreen.SetActive(true);
            switch (condition)
            {
                case 0:
                    GameOverText.text = "You gasped for breath, it wasn't very effective. Game Over!";
                    scoreIn.enabled = false;
                    score.enabled = false;
                    break;
                case 1:
                    GameOverText.text = "Frozen. Game Over!";
                    scoreIn.enabled = false;
                    score.enabled = false;
                    break;
                case 2:
                    GameOverText.text = "I don't think you're supposed to be here...";
                    scoreIn.enabled = false;
                    score.enabled = false;
                    break;
                case 3:
                    GameOverText.text = "Escaped!";
                    if (PlayerPrefs.GetString("HighScore", "null") == "null")
                    {
                        PlayerPrefs.SetString("HighScore", PlayerState.Resources.Oxygen.ToString());
                        PlayerPrefs.Save();
                        highscore.enabled = true;
                    }
                    else
                    {
                        int tempScore;
                        int.TryParse(PlayerPrefs.GetString("HighScore"), out tempScore);
                        if (PlayerState.Resources.Oxygen > tempScore)
                        {
                            PlayerPrefs.SetString("HighScore", PlayerState.Resources.Oxygen.ToString());
                            PlayerPrefs.Save();
                            highscore.enabled = true;
                        }
                    }
                    scoreIn.text = PlayerState.Resources.Oxygen.ToString();
                    break;
                case 4:
                    GameOverText.text = "Your light finally went out. Game Over!";
                    scoreIn.enabled = false;
                    score.enabled = false;
                    break;
                case 5:
                    GameOverText.text = "You were lost in the darkness. Game Over!";
                    scoreIn.enabled = false;
                    score.enabled = false;
                    break;
                case 6:
                    GameOverText.text = "You fell to your doom. Game Over!";
                    scoreIn.enabled = false;
                    score.enabled = false;
                    break;
                case 7:
                    GameOverText.text = "You were frozen by the cold touch of the snowman. Game Over!";
                    scoreIn.enabled = false;
                    score.enabled = false;
                    break;
                case 8:
                    GameOverText.text = "Overheated. Game Over!";
                    scoreIn.enabled = false;
                    score.enabled = false;
                    break;
            }
        }
    }

    public void newGame(int condition)
    {
        //PlayerPrefs.DeleteAll(); //FOR DEBUG ONLY, NOT FOR FINAL GAME, if you want to check if highscore works, run this line of code for one run and it will clear your highscore.
        if (!newLocked && !DisableNewGame)
        {
             if (PlayerPrefs.GetString("HighScore", "null") != "null")
             {
                 highscoreIn.enabled = true;
                 highscoreIn.text = PlayerPrefs.GetString("Highscore");
                 highscoreDisplay.enabled = true;
             }
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            PlayerState.resetResources();
            PlayerState.State.Alive = true;
            PlayerState.State.GameRunning = true;
            Time.timeScale = 1f;
            newLocked = true;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.None;
            //remove other elements of UI
            esayMode.SetActive(false);
            power.enabled = true;
            heat.enabled = true;
            oxygen.enabled = true;
            oxygenIn.enabled = true;
            powerIn.enabled = true;
            heatIn.enabled = true;
            highscore.enabled = true;

            //determine condition
            mainMenuScreen.SetActive(false);
            switch (condition)
            {
                case 0:
                    break;
            }
        }
    }

    public void exitGame()
    {
        Application.Quit();
    }

    public void restartGame()
    {
        PlayerState.resetResources();
        SceneManager.LoadScene("Level_1");
    }
    
    public void toggleEasy()
    {
        PlayerState.State.easyMode = !PlayerState.State.easyMode;
    }
}
