using UnityEngine;
using UnityEngine.SceneManagement;

// Loads another scene (main menu <-> world), after saving the current world
public class SceneLoader : MonoBehaviour
{
    public string sceneName;

    public void LoadScene()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.BeforeSceneChange();
        }

        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
