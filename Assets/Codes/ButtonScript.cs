using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonScript : MonoBehaviour
{
    public void Retry_Single(){
        SceneManager.LoadScene("SinglePlay");
    }
    public void MainMenu(){
        SceneManager.LoadScene("Lobby");
    }
}
