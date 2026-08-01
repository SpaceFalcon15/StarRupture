using StarRupture.DataAccess;
using StarRupture.Models;
using static StarRupture.Services.ConsoleInput;

namespace StarRupture.Services;

public class ItemEditor
{
    private readonly List<Item> _items;
    private readonly Dictionary<string, Item> _itemsByName;
    private readonly ItemRepository _itemRepository;

    public ItemEditor(
        List<Item> items,
        Dictionary<string, Item> itemsByName,
        ItemRepository itemRepository)
    {
        _items = items;
        _itemsByName = itemsByName;
        _itemRepository = itemRepository;
    }

    public void AddBaseResource()
    {
        string name = ReadRequiredText("Enter the resource name: ");

        if (_itemsByName.ContainsKey(name))
        {
            Console.WriteLine($"An item named '{name}' already exists.");
            return;
        }

        var resource = new Item
        {
            Name = name,
            Recipes = []
        };

        _items.Add(resource);
        _itemsByName.Add(resource.Name, resource);

        _itemRepository.SaveItems(_items);

        Console.WriteLine($"'{resource.Name}' was added and saved.");
    }

    public void AddCraftedItem()
    {
        string name = ReadRequiredText("Enter the crafted item name: ");

        if (_itemsByName.ContainsKey(name))
        {
            Console.WriteLine($"An item named '{name}' already exists.");
            return;
        }

        var recipes = new List<Recipe>();

        while (true)
        {
            recipes.Add(ReadRecipe(recipes));

            Console.Write("Add another recipe for this item? (y/n): ");

            if (!ReadYesNo())
            {
                break;
            }
        }

        var item = new Item
        {
            Name = name,
            Recipes = recipes
        };

        _items.Add(item);
        _itemsByName.Add(item.Name, item);

        _itemRepository.SaveItems(_items);

        Console.WriteLine(
            $"'{item.Name}' was added with {item.Recipes.Count} recipe(s).");
    }

    public void AddRecipeToExistingItem()
    {
        string itemName = ReadRequiredText(
            "Enter the name of the item to add a recipe to: ");

        if (!_itemsByName.TryGetValue(itemName, out Item? item) ||
            item is null)
        {
            Console.WriteLine($"'{itemName}' was not found.");
            return;
        }

        Recipe recipe = ReadRecipe(item.Recipes);

        item.Recipes.Add(recipe);

        _itemRepository.SaveItems(_items);

        Console.WriteLine(
            $"A {recipe.Processor} recipe was added to '{item.Name}'.");
    }

    private Recipe ReadRecipe(IReadOnlyCollection<Recipe> existingRecipes)
    {
        string processor;

        while (true)
        {
            processor = ReadRequiredText("Processor: ");

            bool processorAlreadyUsed = existingRecipes.Any(recipe =>
                string.Equals(
                    recipe.Processor,
                    processor,
                    StringComparison.OrdinalIgnoreCase));

            if (!processorAlreadyUsed)
            {
                break;
            }

            Console.WriteLine(
                $"This item already has a {processor} recipe. " +
                "Choose a different processor.");
        }

        decimal craftTimeSeconds = ReadPositiveDecimal(
            "Craft time in seconds: ");

        decimal outputAmount = ReadPositiveDecimal("Output amount: ");

        int ingredientCount = ReadPositiveInt(
            "How many different ingredients are there? ");

        var ingredients = new List<Ingredient>();

        for (int ingredientNumber = 1;
             ingredientNumber <= ingredientCount;
             ingredientNumber++)
        {
            while (true)
            {
                Console.Write($"Ingredient {ingredientNumber} name: ");

                string? enteredName = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(enteredName))
                {
                    Console.WriteLine("An ingredient name is required.");
                    continue;
                }

                string ingredientName = enteredName.Trim();
                Item ingredientItem;

                if (!_itemsByName.TryGetValue(
                        ingredientName,
                        out Item? foundItem) ||
                    foundItem is null)
                {
                    Console.Write(
                        $"'{ingredientName}' does not exist. " +
                        "Do you want to add it to the program? (y/n): ");

                    if (!ReadYesNo())
                    {
                        continue;
                    }

                    ingredientItem = AddMissingItem(ingredientName);
                }
                else
                {
                    ingredientItem = foundItem;
                }

                bool alreadyAdded = ingredients.Any(ingredient =>
                    string.Equals(
                        ingredient.ItemName,
                        ingredientItem.Name,
                        StringComparison.OrdinalIgnoreCase));

                if (alreadyAdded)
                {
                    Console.WriteLine(
                        "That ingredient has already been added to this recipe.");
                    continue;
                }

                decimal amount = ReadPositiveDecimal(
                    $"Amount of {ingredientItem.Name}: ");

                ingredients.Add(new Ingredient
                {
                    ItemName = ingredientItem.Name,
                    Amount = amount
                });

                break;
            }
        }

        return new Recipe
        {
            Processor = processor,
            CraftTime = craftTimeSeconds,
            OutputQuantity = outputAmount,
            Ingredients = ingredients
        };
    }

    private Item AddMissingItem(string itemName)
    {
        Console.WriteLine();
        Console.WriteLine($"Adding '{itemName}' to the system.");

        Console.Write(
            $"Is '{itemName}' a base resource with no recipe? (y/n): ");

        var recipes = new List<Recipe>();

        if (!ReadYesNo())
        {
            while (true)
            {
                recipes.Add(ReadRecipe(recipes));

                Console.Write(
                    $"Add another recipe for '{itemName}'? (y/n): ");

                if (!ReadYesNo())
                {
                    break;
                }
            }
        }

        var item = new Item
        {
            Name = itemName,
            Recipes = recipes
        };

        _items.Add(item);
        _itemsByName.Add(item.Name, item);

        _itemRepository.SaveItems(_items);

        Console.WriteLine(
            $"'{item.Name}' was added with {item.Recipes.Count} recipe(s).");

        return item;
    }
}