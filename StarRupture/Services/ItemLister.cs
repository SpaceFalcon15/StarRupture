using StarRupture.Models;

namespace StarRupture.Services;

public class ItemLister
{
    private readonly List<Item> _items;

    public ItemLister(List<Item> items)
    {
        _items = items;
    }

    public void ListItems()
    {
        Console.WriteLine();

        foreach (Item item in _items.OrderBy(item => item.Name))
        {
            Console.WriteLine($"{item.Name} — {item.Recipes.Count} recipe(s)");
        }
    }
}
