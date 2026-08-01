using System;
using System.Collections.Generic;
using System.Text;

namespace StarRupture.Models
{
    public class ProductionResult
    {
        public required string ItemName { get; init; }
        public required string Processor { get; init; }

        public int ProcessorCount { get; init; }
        public decimal Duration { get; init; }

        public decimal CraftsStarted { get; init; }
        public decimal CraftsCompleted { get; init; }
        public decimal CompletedOutput { get; init; }

        public Dictionary<string, decimal> DirectIngredients { get; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, decimal> BaseResources { get; } = new(StringComparer.OrdinalIgnoreCase);
    }
}
