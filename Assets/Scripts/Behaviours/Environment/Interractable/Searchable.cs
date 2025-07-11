using UnityEngine;

public class Searchable : MonoBehaviour, IInterractable
{
    public ItemStorage storage;

    public string GetInfo()
    {
        return "Open";
    }

    public void Interract(GameObject interractor)
    {
        Searcher searcher = interractor.GetComponent<Searcher>();

        if(searcher != null)
        {
            searcher.Search(storage);
        }
    }
}
