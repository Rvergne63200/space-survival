using UnityEngine;
using UnityEngine.SceneManagement;

public class TriggerSceneLoader : SceneLoader
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            LoadScene();
        }
    }
}