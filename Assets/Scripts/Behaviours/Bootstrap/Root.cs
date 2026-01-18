using UnityEngine;

public class Root : MonoBehaviour
{
    private static bool exists = false;

    private void Awake()
    {
        if (exists)
        {
            Destroy(gameObject);
            return;
        }

        exists = true;
        DontDestroyOnLoad(gameObject);
    }
}
