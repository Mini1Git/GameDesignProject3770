using UnityEngine;
using UnityEngine.SceneManagement;

public class mainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);//load the playable level when play button is pressed
    }

    public void QuitGame()
    {   
        Debug.Log("Quit Game");
        Application.Quit();
    }
}
