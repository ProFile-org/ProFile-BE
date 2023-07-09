namespace Application.Common.Models;


public class ItemsResult<T>
{
    public ItemsResult(IEnumerable<T> items)
    {
        Items = items;
    }
    public IEnumerable<T> Items { get; }
}