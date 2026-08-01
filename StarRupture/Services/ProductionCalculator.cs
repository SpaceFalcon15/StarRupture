using System;
using System.Collections.Generic;
using System.Text;
using StarRupture.Models;

namespace StarRupture.Services
{
    public class ProductionCalculator
    {
        private readonly Dictionary<string, Item> _itemsByName;

        public ProductionCalculator(Dictionary<string, Item> itemsByName)
        {
            _itemsByName = itemsByName;
        }

        public ProductionResult Calcuate(Item outputItem, Recipe outputRecipe, int processorCount, decimal duration, Func<Item, Recipe> recipeSelector)
        {
            decimal craftsStartedPerProcessor = Math.Ceiling(duration / outputRecipe.CraftTime);

            decimal craftsCompletedPerProcessor = Math.Floor(duration / outputRecipe.CraftTime);

            decimal craftsStarted = craftsStartedPerProcessor * processorCount;

            decimal craftsCompleted = craftsCompletedPerProcessor * processorCount;

            var result = new ProductionResult
            {
                ItemName = outputItem.Name,
                Processor = outputRecipe.Processor,
                ProcessorCount = processorCount,
                Duration = duration,
                CraftsStarted = craftsStarted,
                CompletedOutput = craftsCompleted * outputRecipe.OutputQuantity

            };

            foreach (Ingredient ingredient in outputRecipe.Ingredients) 
            {
                decimal amountRequired = ingredient.Amount * craftsStarted;

                AddAmount(result.DirectIngredients, ingredient.ItemName, amountRequired);
                Item ingredientItem = _itemsByName[ingredient.ItemName];
                AddBaseResources(ingredientItem, amountRequired, recipeSelector, result.BaseResources);
            }
            return result;
        }

        private void AddBaseResources(Item item, decimal requiredAmount, Func<Item, Recipe> recipeSelector, Dictionary<string, decimal> baseResources)
        {
            if (item.Recipes.Count == 0)
            {
                AddAmount(baseResources, item.Name, requiredAmount);
                return;
            }
            Recipe recipe= recipeSelector(item);

            decimal batchesRequired = Math.Ceiling(requiredAmount / recipe.OutputQuantity);

            foreach (Ingredient ingredient in recipe.Ingredients) 
            {
                Item ingredientItem = _itemsByName[ingredient.ItemName];

                decimal ingredientAmount = ingredient.Amount * batchesRequired;
                AddBaseResources(ingredientItem, ingredientAmount, recipeSelector, baseResources);
            }
        }

        private static void AddAmount(Dictionary<string,decimal> amounts, string itemName, decimal amount)
        {
            if (amounts.ContainsKey(itemName))
            {
                amounts[itemName] += amount;
            }
            else
            {
                amounts[itemName] = amount;
            }
        }
    }
}
