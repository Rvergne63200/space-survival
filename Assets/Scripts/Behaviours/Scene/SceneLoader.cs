using Mirror;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public string sceneName;
    public Vector3 position;

    public void LoadScene()
    {
        NetworkManager.singleton.ServerChangeScene(sceneName);

        if (PrepareGameObjects.dontDestroyObjects != null)
        {
            foreach (GameObject obj in PrepareGameObjects.dontDestroyObjects)
            {
                obj.transform.position = position;
            }
        }

        GameObject.FindGameObjectWithTag("Player").transform.position = position;
    }
}
