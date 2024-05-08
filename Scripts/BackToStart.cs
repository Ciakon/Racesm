using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToStart : MonoBehaviour
{
    public void returnToStart()
    {
        Destroy(GameObject.Find("Car Data"));
        Destroy(GameObject.Find("SkyboxManager"));
        Destroy(GameObject.Find("GameManager"));
        SceneManager.LoadScene(0);
        
        
    }
}
