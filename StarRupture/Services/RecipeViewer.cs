using StarRupture.Models;
using static StarRupture.Services.ConsoleInput;

namespace StarRupture.Services;

public class RecipeViewer
{
    private readonly Dictionary<string, Item> _itemsByName;

    public RecipeViewer(Dictionary<string, Item> itemsByName)
    {
        _itemsByName = itemsByName;
    }

    public void ViewItemDetails()
    {
        string name = ReadRequiredText("Enter an item name: ");

        if (!_itemsByName.TryGetValue(name, out Item? item) || item is null)
        {
            Console.WriteLine($"'{name}' was not found.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"Item: {item.Name}");

        if (item.Recipes.Count == 0)
        {
            Console.WriteLine("This is a base resource.");
            return;
        }

        for (int recipeNumber = 0;
             recipeNumber < item.Recipes.Count; recipeNumber++)
        {
            Recipe recipe = item.Recipes[recipeNumber];

            Console.WriteLine();
            Console.WriteLine($"Recipe {recipeNumber + 1}");
            Console.WriteLine($"Processor: {recipe.Processor}");
            Console.WriteLine($"Craft time: {recipe.CraftTime} seconds");
            Console.WriteLine($"Output: {recipe.OutputQuantity} {item.Name}");

            Console.WriteLine("Ingredients:");

            foreach (Ingredient ingredient in recipe.Ingredients)
            {
                Console.WriteLine($"- {ingredient.Amount} {ingredient.ItemName}");
            }
        }
    }

    public void ViewFullRecipeTree()
    {
        string itemName = ReadRequiredText("Item to view: ");

        if (!_itemsByName.TryGetValue(itemName, out Item? item) || item is null)
        {
            Console.WriteLine($"'{itemName}' was not found.");
            return;
        }

        decimal requiredAmount = ReadPositiveDecimal($"How many {item.Name} do you need? ");

        var selectedRecipes = new Dictionary<string, Recipe>(StringComparer.OrdinalIgnoreCase);

        Console.WriteLine();
        Console.WriteLine($"Recipe tree for {requiredAmount} {item.Name}:");

        PrintRecipeTree(item, requiredAmount, selectedRecipes, "");
    }

    public Recipe GetRecipeForCalculation(Item item, Dictionary<string, Recipe> selectedRecipes)
    {
        if (selectedRecipes.TryGetValue(item.Name, out Recipe? selectedRecipe) && selectedRecipe is not null)
        {
            return selectedRecipe;
        }

        if (item.Recipes.Count == 1)
        {
            selectedRecipes[item.Name] = item.Recipes[0];

            return item.Recipes[0];
        }

        Console.WriteLine();
        Console.WriteLine($"{item.Name} has multiple recipes:");

        for (int index = 0; index < item.Recipes.Count; index++)
        {
            Recipe recipe = item.Recipes[index];

            Console.WriteLine($"{index + 1}. {recipe.Processor} " + $"({recipe.OutputQuantity} output, " + $"{recipe.CraftTime} seconds)");
        }

        int recipeNumber = ReadNumberInRange("Choose a recipe: ", 1, item.Recipes.Count);

        Recipe chosenRecipe = item.Recipes[recipeNumber - 1];

        selectedRecipes[item.Name] = chosenRecipe;

        return chosenRecipe;
    }

    private void PrintRecipeTree(Item item, decimal requiredAmount, Dictionary<string, Recipe> selectedRecipes, string indentation)
    {
        Console.WriteLine($"{indentation}- {requiredAmount} {item.Name}");

        if (item.Recipes.Count == 0)
        {
            Console.WriteLine($"{indentation}  Base resource");
            return;
        }

        Recipe recipe = GetRecipeForCalculation(item, selectedRecipes);

        decimal batchesRequired = Math.Ceiling(requiredAmount / recipe.OutputQuantity);

        decimal actualOutput = batchesRequired * recipe.OutputQuantity;

        Console.WriteLine($"{indentation}  {recipe.Processor}: " + $"{batchesRequired} craft(s), produces {actualOutput}");

        foreach (Ingredient ingredient in recipe.Ingredients)
        {
            Item ingredientItem = _itemsByName[ingredient.ItemName];

            decimal ingredientAmountNeeded = ingredient.Amount * batchesRequired;

            PrintRecipeTree(ingredientItem, ingredientAmountNeeded, selectedRecipes, indentation + "  ");
        }
    }
}