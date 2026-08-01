using System;
using System.Collections.Generic;
using System.Text;

namespace StarRupture.Models
{
    public class Item
    {
        public required string Name { get; init; }
        public List<Recipe> Recipes { get; init; } = [];
    }
}
