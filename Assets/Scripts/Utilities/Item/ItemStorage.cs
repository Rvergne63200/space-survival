using UnityEngine;

public class ItemStorage : MonoBehaviour
{
    [SerializeField]
    protected ItemCollection<Item> _content;
    public ItemCollection<Item> Content { get => _content; private set => _content = value; }

    private void Awake()
    {
        Content = Content;
    }

    public virtual int Add(Item item, int count = 1)
    {
        return Content.Add(item, count);
    }

    public virtual int Remove(Item item, int count = 1)
    {
        return Content.RemoveItem(item, count);
    }
}
