using UnityEngine;
using UnityEngine.Events;
using System;

public class Searcher : MonoBehaviour
{
    private ItemStorage currentSearchedStorage = null;
    public ItemStorage CurrentSearchedStorage => currentSearchedStorage;

    public UnityEvent<ItemStorage> ev_UpdateSearchStorage;

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
