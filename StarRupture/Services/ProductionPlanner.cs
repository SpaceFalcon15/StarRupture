using StarRupture.Models;
using static StarRupture.Services.ConsoleInput;

namespace StarRupture.Services;

public class ProductionPlanner
{
    private readonly Dictionary<string, Item> _itemsByName;
    private readonly ProductionCalculator _productionCalculator;
    private readonly RecipeViewer _recipeViewer;

    public ProductionPlanner(Dictionary<string, Item> itemsByName, ProductionCalculator productionCalculator, RecipeViewer recipeViewer)
    {
        _itemsByName = itemsByName;
        _productionCalculator = productionCalculator;
        _recipeViewer = recipeViewer;
    }

    public void CalculateProductionRequirements()
    {
        string itemName = ReadRequiredText("Item to produce: ");

        if (!_itemsByName.TryGetValue(itemName, out Item? item) || item is null)
        {
            Console.WriteLine($"'{itemName}' was not found.");
            return;
        }

        if (item.Recipes.Count == 0)
        {
            Console.WriteLine($"'{item.Name}' is a base resource and cannot be produced.");
            return;
        }

        Console.WriteLine($"Available processors for {item.Name}:");

        foreach (Recipe recipe in item.Recipes)
        {
            Console.WriteLine($"- {recipe.Processor}");
        }

        string processorName = ReadRequiredText("Processor to use: ");

        Recipe? selectedRecipe = item.Recipes.FirstOrDefault(recipe => string.Equals(recipe.Processor, processorName, StringComparison.OrdinalIgnoreCase));

        if (selectedRecipe is null)
        {
            Console.WriteLine($"'{processorName}' does not have a recipe for '{item.Name}'.");
            return;
        }

        int processorCount = ReadPositiveInt("Number of processors: ");

        decimal duration = ReadPositiveDecimal("Production time in seconds: ");

        var selectedRecipes = new Dictionary<string, Recipe>(StringComparer.OrdinalIgnoreCase);

        ProductionResult result = _productionCalculator.Calcuate(item, selectedRecipe, processorCount, duration, ingredientItem => _recipeViewer.GetRecipeForCalculation(ingredientItem, selectedRecipes));

        Console.WriteLine();
        Console.WriteLine("Production result");
        Console.WriteLine($"Item: {result.ItemName}");
        Console.WriteLine($"Processor: {result.Processor}");
        Console.WriteLine($"Processors: {result.ProcessorCount}");
        Console.WriteLine($"Time: {result.Duration} seconds");
        Console.WriteLine($"Crafts completed: {result.CraftsCompleted}");
        Console.WriteLine($"Completed output: {result.CompletedOutput}");
        Console.WriteLine($"Crafts started (ingredient requirement): {result.CraftsStarted}");

        Console.WriteLine();
        Console.WriteLine("Direct ingredients required:");

        foreach (KeyValuePair<string, decimal> ingredient in result.DirectIngredients.OrderBy(ingredient => ingredient.Key))
        {
            Console.WriteLine($"- {ingredient.Value} {ingredient.Key}");
        }

        Console.WriteLine();
        Console.WriteLine("Base resources required:");

        foreach (KeyValuePair<string, decimal> resource in result.BaseResources.OrderBy(resource => resource.Key))
        {
            Console.WriteLine($"- {resource.Value} {resource.Key}");
        }
    }
}
