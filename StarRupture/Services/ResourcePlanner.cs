using StarRupture.Models;
using static StarRupture.Services.ConsoleInput;

namespace StarRupture.Services;

public class ResourcePlanner
{
    private readonly Dictionary<string, Item> _itemsByName;
    private readonly RecipeViewer _recipeViewer;

    public ResourcePlanner(Dictionary<string, Item> itemsByName, RecipeViewer recipeViewer)
    {
        _itemsByName = itemsByName;
        _recipeViewer = recipeViewer;
    }

    public void CalculateBaseResources()
    {
        string itemName = ReadRequiredText("Item to calculate: ");

        if (!_itemsByName.TryGetValue(itemName, out Item? item) || item is null)
        {
            Console.WriteLine($"'{itemName}' was not found.");
            return;
        }

        decimal requiredAmount = ReadPositiveDecimal($"How many {item.Name} do you need? ");

        var baseResources = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

        var selectedRecipes = new Dictionary<string, Recipe>(StringComparer.OrdinalIgnoreCase);

        AddRequiredResources(item, requiredAmount, selectedRecipes,baseResources);

        Console.WriteLine();
        Console.WriteLine($"Base resources required for {requiredAmount} {item.Name}:");

        foreach (KeyValuePair<string, decimal> resource in baseResources.OrderBy(resource => resource.Key))
        {
            Console.WriteLine($"- {resource.Value} {resource.Key}");
        }
    }

    private void AddRequiredResources(Item item, decimal requiredAmount, Dictionary<string, Recipe> selectedRecipes, Dictionary<string, decimal> baseResources)
    {
        if (item.Recipes.Count == 0)
        {
            if (baseResources.ContainsKey(item.Name))
            {
                baseResources[item.Name] += requiredAmount;
            }
            else
            {
                baseResources[item.Name] = requiredAmount;
            }

            return;
        }

        Recipe recipe = _recipeViewer.GetRecipeForCalculation(item, selectedRecipes);

        decimal batchesRequired = Math.Ceiling(requiredAmount / recipe.OutputQuantity);

        foreach (Ingredient ingredient in recipe.Ingredients)
        {
            Item ingredientItem = _itemsByName[ingredient.ItemName];

            decimal ingredientAmountNeeded = ingredient.Amount * batchesRequired;

            AddRequiredResources(ingredientItem, ingredientAmountNeeded, selectedRecipes, baseResources);
        }
    }
}
