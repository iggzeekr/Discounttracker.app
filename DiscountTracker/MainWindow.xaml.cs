using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DiscountTracker.Models;
using DiscountTracker.Services;

namespace DiscountTracker
{
    public partial class MainWindow : Window
    {
        private readonly DataService _dataService;
        private List<Product> _products;
        private List<Product> _filteredProducts;

        public MainWindow()
        {
            InitializeComponent();
            _dataService = new DataService();
            _products = _dataService.LoadProducts();
            _filteredProducts = new List<Product>(_products);
            RefreshUI();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                MessageBox.Show("Please enter a product name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtOriginalPrice.Text, out decimal originalPrice) || originalPrice <= 0)
            {
                MessageBox.Show("Please enter a valid original price.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtDiscountedPrice.Text, out decimal discountedPrice) || discountedPrice <= 0)
            {
                MessageBox.Show("Please enter a valid discounted price.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (discountedPrice >= originalPrice)
            {
                MessageBox.Show("Discounted price should be less than original price.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create product
            var product = new Product
            {
                Name = txtProductName.Text.Trim(),
                Store = string.IsNullOrWhiteSpace(txtStore.Text) ? "Unknown Store" : txtStore.Text.Trim(),
                Category = (cmbCategory.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Other",
                OriginalPrice = originalPrice,
                DiscountedPrice = discountedPrice,
                Url = txtUrl.Text.Trim()
            };

            _products.Add(product);
            _dataService.SaveProducts(_products);

            // Clear inputs
            txtProductName.Text = "";
            txtStore.Text = "";
            txtOriginalPrice.Text = "";
            txtDiscountedPrice.Text = "";
            txtUrl.Text = "";
            cmbCategory.SelectedIndex = 0;

            ApplyFilters();
            RefreshUI();

            MessageBox.Show($"Deal added! You save ₺{product.SavedAmount:N0} ({product.DiscountPercentage}% off)", 
                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Guid id)
            {
                var result = MessageBox.Show("Are you sure you want to delete this deal?", 
                    "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _products.RemoveAll(p => p.Id == id);
                    _dataService.SaveProducts(_products);
                    ApplyFilters();
                    RefreshUI();
                }
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CmbFilterCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_products == null) return;

            var searchText = txtSearch?.Text?.ToLower() ?? "";
            var categoryFilter = (cmbFilterCategory?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "All Categories";
            var sortOption = (cmbSort?.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Newest First";

            // Filter
            _filteredProducts = _products.Where(p =>
            {
                var matchesSearch = string.IsNullOrEmpty(searchText) ||
                    p.Name.ToLower().Contains(searchText) ||
                    p.Store.ToLower().Contains(searchText);

                var matchesCategory = categoryFilter == "All Categories" ||
                    p.Category == categoryFilter;

                return matchesSearch && matchesCategory && p.IsActive;
            }).ToList();

            // Sort
            _filteredProducts = sortOption switch
            {
                "Highest Discount" => _filteredProducts.OrderByDescending(p => p.DiscountPercentage).ToList(),
                "Lowest Price" => _filteredProducts.OrderBy(p => p.DiscountedPrice).ToList(),
                "Most Saved" => _filteredProducts.OrderByDescending(p => p.SavedAmount).ToList(),
                _ => _filteredProducts.OrderByDescending(p => p.AddedDate).ToList()
            };

            productsPanel.ItemsSource = _filteredProducts;
            
            // Show/hide empty state
            emptyState.Visibility = _filteredProducts.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void RefreshUI()
        {
            ApplyFilters();
            UpdateStats();
        }

        private void UpdateStats()
        {
            var activeProducts = _products.Where(p => p.IsActive).ToList();

            txtTotalDeals.Text = activeProducts.Count.ToString();
            txtTotalSaved.Text = $"₺{activeProducts.Sum(p => p.SavedAmount):N0}";
            
            var avgDiscount = activeProducts.Count > 0 
                ? activeProducts.Average(p => p.DiscountPercentage) 
                : 0;
            txtAvgDiscount.Text = $"%{avgDiscount:N0}";

            var bestDeal = activeProducts.Count > 0 
                ? activeProducts.Max(p => p.DiscountPercentage) 
                : 0;
            txtBestDeal.Text = $"%{bestDeal:N0}";
        }
    }
}

