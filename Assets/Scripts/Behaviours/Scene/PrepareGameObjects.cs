using UnityEngine;
using UnityEngine.SceneManagement;

public class PrepareGameObjects : MonoBehaviour
{
    public static GameObject[] dontDestroyObjects;

    public string sceneName;

    public GameObject[] objectsToSave;

    private void Awake()
    {
        dontDestroyObjects = objectsToSave;

        foreach (GameObject obj in dontDestroyObjects)
        {
            DontDestroyOnLoad(obj);
        }

        SceneManager.LoadScene(sceneName);

        foreach (GameObject obj in dontDestroyObjects)
        {
            obj.transform.position = Vector3.zero;
        }
    }
}
