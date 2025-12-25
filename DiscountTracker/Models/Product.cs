using System;

namespace DiscountTracker.Models
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public string Store { get; set; } = string.Empty;
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime AddedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        public decimal DiscountPercentage => 
            OriginalPrice > 0 ? Math.Round((1 - DiscountedPrice / OriginalPrice) * 100, 1) : 0;

        public decimal SavedAmount => OriginalPrice - DiscountedPrice;

        public string DiscountBadge => $"%{DiscountPercentage:0}";
    }
}

