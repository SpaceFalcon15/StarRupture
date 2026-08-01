using System;
using System.Collections.Generic;
using System.Text;
using StarRupture.Models;
using System.Text.Json;

namespace StarRupture.DataAccess
{
    public class ItemRepository
    {
        private readonly string _jsonPath;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
        public ItemRepository(string jsonPath)
        {
            _jsonPath = jsonPath;
        }

        public List<Item> LoadItems()
        {
            if (!File.Exists(_jsonPath))
            {
                return [];
            }

            string json = File.ReadAllText(_jsonPath);

            return JsonSerializer.Deserialize<List<Item>>(json, _jsonOptions) ?? [];
        }

        public void SaveItems(List<Item> items)
        {
            string? directory = Path.GetDirectoryName(_jsonPath);

            if (directory is not null)
            {
                Directory.CreateDirectory(directory);
            }

            List<Item> itemsInNameOrder = items.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToList();
            string json = JsonSerializer.Serialize(itemsInNameOrder, _jsonOptions);
            File.WriteAllText(_jsonPath, json);
        }
    }
}
