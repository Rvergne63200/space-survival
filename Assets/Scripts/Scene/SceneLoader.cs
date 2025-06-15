using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string sceneName;
    public Vector3 position;


    private void OnTriggerEnter(Collider other)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);

        foreach (GameObject obj in PrepareGameObjects.dontDestroyObjects)
        {
            obj.transform.position = position;
        }
    }
}