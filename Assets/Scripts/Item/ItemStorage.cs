using UnityEngine;

public class ItemStorage : MonoBehaviour
{
    [SerializeField]
    protected ItemCollection<Item> _content;
    public ItemCollection<Item> Content { get => _content; private set => _content = value; }

    public int Add(Item item, int count = 1)
    {
        return Content.Add(item, count);
    }

    public void Remove(Item item, int count = 1)
    {
        Content.RemoveItem(item, count);
    }
}
