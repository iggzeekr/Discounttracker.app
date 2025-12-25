using System;
using System.Collections.Generic;
using System.IO;
using DiscountTracker.Models;
using Newtonsoft.Json;

namespace DiscountTracker.Services
{
    public class DataService
    {
        private readonly string _filePath;

        public DataService()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appData, "DiscountTracker");
            Directory.CreateDirectory(appFolder);
            _filePath = Path.Combine(appFolder, "products.json");
        }

        public List<Product> LoadProducts()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    return JsonConvert.DeserializeObject<List<Product>>(json) ?? new List<Product>();
                }
            }
            catch (Exception)
            {
                // Return empty list on error
            }
            return new List<Product>();
        }

        public void SaveProducts(List<Product> products)
        {
            try
            {
                var json = JsonConvert.SerializeObject(products, Formatting.Indented);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception)
            {
                // Handle error silently
            }
        }
    }
}

