using UnityEngine;
using UnityEngine.Events;
using System;

public class Searcher : MonoBehaviour
{
    private ItemStorage currentSearchedStorage = null;

    public UnityEvent<ItemStorage> ev_UpdateSearchStorage;

    public void Search(ItemStorage itemStorage)
    {
        if(currentSearchedStorage == null)
        {
            currentSearchedStorage = itemStorage;
        }
        else
        {
            currentSearchedStorage = null;
        }
        
        ev_UpdateSearchStorage.Invoke(currentSearchedStorage);
    }

    public void ClearCurrentSearchedStorage()
    {
        currentSearchedStorage = null;
        ev_UpdateSearchStorage.Invoke(currentSearchedStorage);
    }
}
