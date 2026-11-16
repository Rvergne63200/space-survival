using UnityEngine;
using UnityEngine.Events;
using System;

public class Searcher : MonoBehaviour
{
    private ItemStorage currentSearchedStorage = null;
    public ItemStorage CurrentSearchedStorage => currentSearchedStorage;

    public UnityEvent<ItemStorage> ev_UpdateSearchStorage;

    // The searched storage is closed beyond this distance (meters), or when it is destroyed
    public float maxDistance = 4.5f;

    private void Update()
    {
        if (currentSearchedStorage == null)
        {
            // Destroyed while open (Unity null): close its interface
            if (!ReferenceEquals(currentSearchedStorage, null))
            {
                ClearCurrentSearchedStorage();
            }

            return;
        }

        if (InteractionDistance.To(currentSearchedStorage, transform.position) > maxDistance)
        {
            ClearCurrentSearchedStorage();
        }
    }

    public void Search(ItemStorage itemStorage)
    {
        // Searching the open storage again closes it, searching another one opens it instead
        if(currentSearchedStorage == itemStorage)
        {
            currentSearchedStorage = null;
        }
        else
        {
            currentSearchedStorage = itemStorage;
        }
        
        ev_UpdateSearchStorage.Invoke(currentSearchedStorage);
    }

    public void ClearCurrentSearchedStorage()
    {
        currentSearchedStorage = null;
        ev_UpdateSearchStorage.Invoke(currentSearchedStorage);
    }
}
