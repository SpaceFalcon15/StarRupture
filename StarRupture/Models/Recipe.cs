using System;
using System.Collections.Generic;
using System.Text;

namespace StarRupture.Models
{
    public class Recipe
    {
        public required string Processor { get; init; }
        public decimal CraftTime { get; init; }

        public decimal OutputQuantity { get; init; } = 1;

        public List<Ingredient> Ingredients { get; init; } = [];
    }
}
