using UnityEngine;

public class Searchable : MonoBehaviour, IInterractable
{
    public ItemStorage storage;

    [SerializeField] private Sprite icon;

    public string GetInfo()
    {
        return "Open";
    }

    public Sprite GetIcon()
    {
        return icon;
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
