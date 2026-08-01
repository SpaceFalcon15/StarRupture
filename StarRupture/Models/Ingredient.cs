using System;
using System.Collections.Generic;
using System.Text;

namespace StarRupture.Models
{
    public class Ingredient
    {
        public required string ItemName { get; init; }
        public decimal Amount { get; init; }
    }
}
