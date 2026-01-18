using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrapper : MonoBehaviour
{
    [SerializeField] private string firstScene;
    private static bool initialized = false;

    private void Awake()
    {
        if (initialized) return;

        initialized = true;

        if (SceneManager.sceneCount == 1)
        {
            SceneManager.LoadScene(firstScene);
        }
    }
}
