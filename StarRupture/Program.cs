using StarRupture.DataAccess;
using StarRupture.Models;
using StarRupture.Services;
using static StarRupture.Services.ConsoleInput;
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

            var itemLister = new ItemLister(items);

            Dictionary<string, Item> itemsByName = itemsByName = items.ToDictionary(item => item.Name, StringComparer.OrdinalIgnoreCase);
            
            var productionCalculator = new ProductionCalculator(itemsByName);

            var itemEditor = new ItemEditor(items, itemsByName, itemRepository);

            var recipeViewer = new RecipeViewer(itemsByName);
            
            var resourcePlanner = new ResourcePlanner(itemsByName, recipeViewer);

            var productionPlanner = new ProductionPlanner(itemsByName, productionCalculator, recipeViewer);

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
                        itemEditor.AddBaseResource();
                        break;
                    case "2":
                        itemEditor.AddCraftedItem();
                        break;
                    case "3":
                        itemLister.ListItems();
                        break;
                    case "4":
                        recipeViewer.ViewItemDetails();
                        break;
                    case "5":
                        resourcePlanner.CalculateBaseResources();
                        break;
                    case "6":
                        recipeViewer.ViewFullRecipeTree();
                        break;
                    case "7":
                        itemEditor.AddCraftedItem();
                        break;
                    case "8":
                        productionPlanner.CalculateProductionRequirements();
                        break;
                    default:
                        Console.WriteLine("That is not a valid option.");
                        break;
                }
            }
        }
    }
}
