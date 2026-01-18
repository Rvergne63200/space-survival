using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneContext : MonoBehaviour
{
    private void Awake()
    {
        if (FindObjectOfType<Root>() == null)
        {
            SceneManager.LoadScene("Bootstrap", LoadSceneMode.Additive);
        }
    }
}
