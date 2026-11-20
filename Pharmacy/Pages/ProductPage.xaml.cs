
using Pharmacy.Model;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Pharmacy.Pages
{
    public partial class ProductPage : Page
    {
        private Products currentProduct;
        private const int MinimumStock = 5;

        public ProductPage(Products product)
        {
            InitializeComponent();

            currentProduct = product;

            LoadProduct();
        }

        private void LoadProduct()
        {
            if (currentProduct == null)
                return;

            NameTb.Text = currentProduct.Name;
            PriceTb.Text = "Цена: " + currentProduct.Price + " руб.";

            if (!string.IsNullOrEmpty(currentProduct.Image))
            {
                ProductImage.Source = new BitmapImage(new Uri(currentProduct.Image, UriKind.RelativeOrAbsolute));
            }

            if (Core.CurrentUser == null)
            {
                QuantityTb.Text = "В наличии в аптеке: нет данных";
                return;
            }

            var userPharmacy = Core.Context.UserPharmacy.FirstOrDefault(up => up.UserID == Core.CurrentUser.ID);

            if (userPharmacy == null)
            {
                QuantityTb.Text = "В наличии в аптеке: нет данных";
                return;
            }

            var productPharmacy = Core.Context.ProductPharmacies.FirstOrDefault(pp => pp.ProductID == currentProduct.ID && pp.PharmacyID == userPharmacy.PharmacyID);

            int count = productPharmacy != null ? productPharmacy.Count : 0;

            QuantityTb.Text = "В наличии в аптеке: " + count + " шт.";

            CheckStock(count);

            LoadSupplyHistory(userPharmacy.PharmacyID);

            var categories = currentProduct.Category.Select(c => c.Name).ToList();

            CategoryTb.Text = "Категории: " + (categories.Count > 0 ? string.Join(", ", categories) : "не указаны");
        }

        private void CheckStock(int count)
        {
            if (count <= MinimumStock)
            {
                LowStockWarningTb.Visibility = Visibility.Visible;

                if (count == 0)
                {
                    LowStockWarningTb.Text = "Внимание! Препарата нет в наличии!";
                }
                else
                {
                    LowStockWarningTb.Text = "Внимание! Заканчивается препарат. Осталось " + count + " шт.";
                }
            }
            else
            {
                LowStockWarningTb.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadSupplyHistory(int pharmacyId)
        {
            var history = Core.Context.ProductsSupply.Where(ps => ps.ProductID == currentProduct.ID && ps.Supplies.PharmaciesID == pharmacyId)
                .Select(ps => new
                {
                    Date = ps.Supplies.Date,
                    SupplyID = ps.SupplyID,
                    Count = ps.Count
                })
                .OrderByDescending(ps => ps.Date).ToList();

            ProductHistoryDG.ItemsSource = history;

            HistoryEmptyTb.Visibility = history.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ProductsPage());
        }
    }
}
