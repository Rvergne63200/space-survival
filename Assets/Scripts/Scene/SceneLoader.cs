using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string sceneName;
    public Vector3 position;

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);

        if (PrepareGameObjects.dontDestroyObjects != null)
        {
            foreach (GameObject obj in PrepareGameObjects.dontDestroyObjects)
            {
                obj.transform.position = position;
            }
        }
    }
}
