using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BuildObject : MonoBehaviour
{
    public BuildVerifier verifier;
    public GameObject[] childs;

    public UnityEvent ev_put;

    public bool Check()
    {
        return verifier.Check();
    }

    public void Put()
    {
        Destroy(verifier);

        List<Collider> colliders = GetChilds<Collider>();

        foreach (Collider collider in colliders)
        {
            collider.enabled = true;
        }

        ev_put.Invoke();
    }

    public List<T> GetChilds<T>() where T : Component
    {
        List<T> childsComponents = new List<T>();

        foreach (GameObject child in childs)
        {
            T childComponent = child.GetComponent<T>();
            if(childComponent != null)
            {
                childsComponents.Add(childComponent);
            }
        }

        return childsComponents;
    }
}
