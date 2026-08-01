using StarRupture.DataAccess;
using StarRupture.Models;
using StarRupture.Services;
using System.Security.Permissions;

namespace StarRupture
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "items.json");
            Console.WriteLine($"Using JSON file: {jsonPath}");
            var itemRepository = new ItemRepository(jsonPath);

            List<Item> items = itemRepository.LoadItems();

            Dictionary<string, Item> itemsByName = itemsByName = items.ToDictionary(item => item.Name, StringComparer.OrdinalIgnoreCase);
            
            var productionCalculator = new ProductionCalculator(itemsByName);

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Star Rupture Recipe Manager");
                Console.WriteLine("1. Add base resource");
                Console.WriteLine("2. Add recipe");
                Console.WriteLine("3. List items");
                Console.WriteLine("4. View Item details");
                Console.WriteLine("5. Calculate base resources for an item");
                Console.WriteLine("6. View full recipe tree");
                Console.WriteLine("7. Add recipe to existing item");
                Console.WriteLine("8. Calculate production requirements");
                Console.WriteLine("9. Exit");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        AddBaseResource(items, itemsByName, itemRepository);
                        break;
                    case "2":
                        AddRecipe(items, itemsByName, itemRepository);
                        break;
                    case "3":
                        ListItems(items);
                        break;
                    case "4":
                        ViewItemDetails(itemsByName);
                        break;
                    case "5":
                        CalculateBaseResources(itemsByName);
                        break;
                    case "6":
                        ViewFullRecipeTree(itemsByName);
                        break;
                    case "7":
                        AddRecipeToExistingItem(items, itemsByName, itemRepository);
                        break;
                    case "8":
                        CalculateProductionRequirements(itemsByName, productionCalculator);
                        break;
                    default:
                        Console.WriteLine("That is not a valid option.");
                        break;
                }
            }
        }
        static void ViewFullRecipeTree(Dictionary<string, Item> itemsByName)
        {
            string itemName = ReadRequiredText("Item to view: ");

            if (!itemsByName.TryGetValue(itemName, out Item? item))
            {
                Console.WriteLine($"'{itemName}' was not found.");
                return;
            }

            decimal requiredAmount = ReadPositiveDecimal(
                $"How many {item.Name} do you need? ");

            var selectedRecipes = new Dictionary<string, Recipe>(
                StringComparer.OrdinalIgnoreCase);

            Console.WriteLine();
            Console.WriteLine($"Recipe tree for {requiredAmount} {item.Name}:");

            PrintRecipeTree(
                item,
                requiredAmount,
                itemsByName,
                selectedRecipes,
                "");
        }

        static void PrintRecipeTree(
            Item item,
            decimal requiredAmount,
            Dictionary<string, Item> itemsByName,
            Dictionary<string, Recipe> selectedRecipes,
            string indentation)
        {
            Console.WriteLine($"{indentation}- {requiredAmount} {item.Name}");

            if (item.Recipes.Count == 0)
            {
                Console.WriteLine($"{indentation}  Base resource");
                return;
            }

            Recipe recipe = GetRecipeForCalculation(item, selectedRecipes);

            decimal batchesRequired = Math.Ceiling(
                requiredAmount / recipe.OutputQuantity);

            decimal actualOutput = batchesRequired * recipe.OutputQuantity;

            Console.WriteLine(
                $"{indentation}  {recipe.Processor}: " +
                $"{batchesRequired} craft(s), produces {actualOutput}");

            foreach (Ingredient ingredient in recipe.Ingredients)
            {
                Item ingredientItem = itemsByName[ingredient.ItemName];

                decimal ingredientAmountNeeded =
                    ingredient.Amount * batchesRequired;

                PrintRecipeTree(
                    ingredientItem,
                    ingredientAmountNeeded,
                    itemsByName,
                    selectedRecipes,
                    indentation + "  ");
            }
        }
        static void AddBaseResource(List<Item> items, Dictionary<string, Item> itemsByName, ItemRepository itemRepository) 
        {
            Console.Write("Enter the resource name: ");

            string? enteredName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(enteredName))
            {
                Console.WriteLine("Resource name cannot be empty.");
                return;
            }

            string name = enteredName.Trim();

            if (itemsByName.ContainsKey(name))
            {
                Console.WriteLine($"An item named '{name}' already exists.");
                return;
            }

            var resource = new Item
            {
                Name = name,
                Recipes = []
            };
            items.Add(resource);
            itemsByName.Add(resource.Name, resource);

            itemRepository.SaveItems(items);

            Console.WriteLine($"'{resource.Name}' was added and saved.");
        }

        static void AddRecipeToExistingItem(
            List<Item> items,
            Dictionary<string, Item> itemsByName,
            ItemRepository itemRepository)
        {
            string itemName = ReadRequiredText(
                "Enter the name of the item to add a recipe to: ");

            if (!itemsByName.TryGetValue(itemName, out Item? item))
            {
                Console.WriteLine($"'{itemName}' was not found.");
                return;
            }

            Recipe recipe = ReadRecipe(
                items,
                itemsByName,
                itemRepository,
                item.Recipes);

            item.Recipes.Add(recipe);

            itemRepository.SaveItems(items);

            Console.WriteLine(
                $"A {recipe.Processor} recipe was added to '{item.Name}'.");
        }

        static void CalculateProductionRequirements(
    Dictionary<string, Item> itemsByName,
    ProductionCalculator productionCalculator)
        {
            string itemName = ReadRequiredText("Item to produce: ");

            if (!itemsByName.TryGetValue(itemName, out Item? item))
            {
                Console.WriteLine($"'{itemName}' was not found.");
                return;
            }

            if (item.Recipes.Count == 0)
            {
                Console.WriteLine(
                    $"'{item.Name}' is a base resource and cannot be produced.");
                return;
            }

            Console.WriteLine($"Available processors for {item.Name}:");

            foreach (Recipe recipe in item.Recipes)
            {
                Console.WriteLine($"- {recipe.Processor}");
            }

            string processorName = ReadRequiredText("Processor to use: ");

            Recipe? selectedRecipe = item.Recipes.FirstOrDefault(recipe =>
                string.Equals(
                    recipe.Processor,
                    processorName,
                    StringComparison.OrdinalIgnoreCase));

            if (selectedRecipe is null)
            {
                Console.WriteLine(
                    $"'{processorName}' does not have a recipe for '{item.Name}'.");
                return;
            }

            int processorCount = ReadPositiveInt("Number of processors: ");

            decimal durationSeconds = ReadPositiveDecimal(
                "Production time in seconds: ");

            var selectedRecipes = new Dictionary<string, Recipe>(
                StringComparer.OrdinalIgnoreCase);

            ProductionResult result = productionCalculator.Calcuate(
                item,
                selectedRecipe,
                processorCount,
                durationSeconds,
                ingredientItem => GetRecipeForCalculation(
                    ingredientItem,
                    selectedRecipes));

            Console.WriteLine();
            Console.WriteLine("Production result");
            Console.WriteLine($"Item: {result.ItemName}");
            Console.WriteLine($"Processor: {result.Processor}");
            Console.WriteLine($"Processors: {result.ProcessorCount}");
            Console.WriteLine($"Time: {result.Duration} seconds");
            Console.WriteLine($"Crafts completed: {result.CraftsCompleted}");
            Console.WriteLine($"Completed output: {result.CompletedOutput}");
            Console.WriteLine(
                $"Crafts started (ingredient requirement): {result.CraftsStarted}");

            Console.WriteLine();
            Console.WriteLine("Direct ingredients required:");

            foreach (KeyValuePair<string, decimal> ingredient in
                     result.DirectIngredients.OrderBy(ingredient => ingredient.Key))
            {
                Console.WriteLine($"- {ingredient.Value} {ingredient.Key}");
            }

            Console.WriteLine();
            Console.WriteLine("Base resources required:");

            foreach (KeyValuePair<string, decimal> resource in
                     result.BaseResources.OrderBy(resource => resource.Key))
            {
                Console.WriteLine($"- {resource.Value} {resource.Key}");
            }
        }

        static Recipe GetRecipeForCalculation(Item item, Dictionary<string, Recipe> selectedRecipes)
        {
            if (selectedRecipes.TryGetValue(item.Name, out Recipe? selectedRecipe))
            {
                return selectedRecipe;
            }

            if (item.Recipes.Count == 1)
            {
                selectedRecipes[item.Name] = item.Recipes[0];
                return item.Recipes[0];
            }

            Console.WriteLine($"\n{item.Name} has multiple recipes:");

            for (int index = 0; index < item.Recipes.Count; index++) 
            {
                Recipe recipe = item.Recipes[index];

                Console.WriteLine($"{index + 1}. {recipe.Processor}" + $"{recipe.OutputQuantity} output, {recipe.CraftTime} seconds");
            }

            int recipeNumber = ReadNumberInRange("Choose a recipe:", 1, item.Recipes.Count);

            Recipe chosenRecipe = item.Recipes[recipeNumber - 1];
            selectedRecipes[item.Name] = chosenRecipe;

            return chosenRecipe;

        }

        static void AddRecipe(List<Item> items, Dictionary<string, Item> itemsByName, ItemRepository itemRepository)
        {
            string name = ReadRequiredText("Enter the crafted item name:");

            if (itemsByName.ContainsKey(name))
            {
                Console.WriteLine($"An item named '{name}' already exists.");
                return;
            }

            var recipes = new List<Recipe>();

            do
            {
                recipes.Add(ReadRecipe(
                items,
                itemsByName,
                itemRepository,
                recipes));

                Console.Write("Add another recipe for this item? (y/n): ");
            }
            while (Console.ReadLine()?.Trim().Equals("y", StringComparison.OrdinalIgnoreCase) == true);

            var item = new Item 
            {
                Name = name,
                Recipes = recipes
            };

            items.Add(item);
            itemsByName.Add(item.Name, item);
            itemRepository.SaveItems(items);

            Console.WriteLine($"'{item.Name}' with {item.Recipes.Count} recipe(s) was added and saved.");
        }

        static Recipe ReadRecipe(
            List<Item> items,
            Dictionary<string, Item> itemsByName,
            ItemRepository itemRepository,
            IReadOnlyCollection<Recipe> existingRecipes)
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

                Console.WriteLine($"This item already has a {processor} recipe. " + "Choose a different processor.");
            }

            decimal craftTimeSeconds = ReadPositiveDecimal("Craft time in seconds: ");
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

                    if (!itemsByName.TryGetValue(ingredientName, out Item? ingredientItem))
                    {
                        Console.Write(
                            $"'{ingredientName}' does not exist. " +
                            "Do you want to add it to the program? (y/n): ");

                        if (!ReadYesNo())
                        {
                            continue;
                        }

                        ingredientItem = AddMissingItem(ingredientName, items, itemsByName, itemRepository);
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

        static Item AddMissingItem(
    string itemName,
    List<Item> items,
    Dictionary<string, Item> itemsByName,
    ItemRepository itemRepository)
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
                    recipes.Add(ReadRecipe(
                        items,
                        itemsByName,
                        itemRepository,
                        recipes));

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

            items.Add(item);
            itemsByName.Add(item.Name, item);

            itemRepository.SaveItems(items);

            Console.WriteLine(
                $"'{item.Name}' was added with {item.Recipes.Count} recipe(s).");

            return item;
        }

        static int ReadNumberInRange(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);

                if (int.TryParse(Console.ReadLine(), out int value) && value >= min && value <= max)
                {
                    return value;
                }

                Console.WriteLine($"Enter a whole number from {min} to {max}.");
            }
        }

        static void CalculateBaseResources(Dictionary<string, Item> itemsByName)
        {
            string itemName = ReadRequiredText("Item to calculate base resources for: ");
            if (!itemsByName.TryGetValue(itemName, out Item? item))
            {
                Console.WriteLine($"'{itemName}' does not exist.");
                return;
            }

            decimal requiredAmount = ReadPositiveDecimal($"How many {item.Name} do you need?");

            var baseResources = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

            // Keeps a recipe choice consistent if the same item appears more than once.
            var SelectedRecipes = new Dictionary<string, Recipe>(StringComparer.OrdinalIgnoreCase);

            AddRequiredResources(item, requiredAmount, itemsByName, SelectedRecipes, baseResources);

            Console.WriteLine($"\n- Base resources required for {requiredAmount} {item.Name}:");

            foreach (KeyValuePair<string, decimal> resource in baseResources.OrderBy(resource => resource.Key))
            {
                Console.WriteLine($"- {resource.Value} {resource.Key}");
            }
        }

        static void AddRequiredResources(Item item, decimal requiredAmount, Dictionary<string, Item> itemsByName, 
            Dictionary<string, Recipe> selectedRecipes, Dictionary<string, decimal> baseResources)
        {
            // No recipes means this item is a base resource.
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

            Recipe recipe = GetRecipeForCalculation(item, selectedRecipes);
            // A recipe might output 2, 3, or more items, so whole craft batches are needed.
            decimal batchesRequired = Math.Ceiling(requiredAmount / recipe.OutputQuantity);

            foreach (Ingredient ingredient in recipe.Ingredients)
            {
                Item ingredientItem = itemsByName[ingredient.ItemName];

                decimal ingredientAmountNeeded = ingredient.Amount * batchesRequired;

                AddRequiredResources(ingredientItem, ingredientAmountNeeded, itemsByName, selectedRecipes, baseResources);
            }

        }
        static void ViewItemDetails(Dictionary<string, Item> itemsByName)
        {
            string name = ReadRequiredText("Enter an item name: ");

            if (!itemsByName.TryGetValue(name, out Item? item))
            {
                Console.WriteLine($"'{name}' does not exist.");
                return;
            }

            Console.WriteLine($"\nItem: {item.Name}");

            if (item.Recipes.Count == 0)
            {
                Console.WriteLine("This is a base resource.");
                return;
            }

            for (int recipeNumber = 0; recipeNumber < item.Recipes.Count; recipeNumber++)
            {
                Recipe recipe = item.Recipes[recipeNumber];

                Console.WriteLine($"\nRecipe {recipeNumber + 1}");
                Console.WriteLine($"Processor: {recipe.Processor}");
                Console.WriteLine($"Craft time: {recipe.CraftTime} seconds");
                Console.WriteLine($"Output quantity: {recipe.OutputQuantity}");

                Console.WriteLine("Ingredients:");

                foreach (Ingredient ingredient in recipe.Ingredients)
                {
                    Console.WriteLine($"- {ingredient.Amount} {ingredient.ItemName}");
                }
            }
            
        }

        static string ReadRequiredText(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);

                string? input = Console.ReadLine()?.Trim();

                if (!string.IsNullOrWhiteSpace(input))
                {
                    return input;
                }

                Console.WriteLine("A value is required.");
            }
        }

        static decimal ReadPositiveDecimal(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);

                if (decimal.TryParse(Console.ReadLine(), out decimal value) && value > 0)
                {
                    return value;
                }

                Console.WriteLine("Enter a number greater than zero.");
            }
        }

        static int ReadPositiveInt(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);

                if (int.TryParse(Console.ReadLine(), out int value) && value > 0)
                {
                    return value;
                }

                Console.WriteLine("Enter a whole number greater than zero.");
            }
        }

        static void ListItems(List<Item> items)
        {
            Console.WriteLine();

            foreach (Item item in items.OrderBy(item => item.Name)) 
            {
                Console.WriteLine($"{item.Name} - {item.Recipes.Count} recipes(s)");
            }
        }

        static bool ReadYesNo()
        {
            while (true)
            {
                string? answer = Console.ReadLine()?.Trim();

                if (answer?.Equals("y", StringComparison.OrdinalIgnoreCase) == true ||
                    answer?.Equals("yes", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return true;
                }

                if (answer?.Equals("n", StringComparison.OrdinalIgnoreCase) == true ||
                    answer?.Equals("no", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return false;
                }

                Console.Write("Please enter y or n: ");
            }
        }
    }
}
