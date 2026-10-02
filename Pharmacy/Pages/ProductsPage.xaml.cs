using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Pharmacy.Pages
{
    /// <summary>
    /// Логика взаимодействия для ProductsPage.xaml
    /// </summary>
    public partial class ProductsPage : Page
    {
        public ProductsPage()
        {
            InitializeComponent();

            ProductsLB.ItemsSource = Core.Context.Products.ToList();
            UpdateFilters();
        }

        private void UpdateFilters()
        {
            if (Core.CurrentUser == null)
            {
                ProductsLB.ItemsSource = null;
                return;
            }

            var userPharmacy = Core.CurrentUser.Pharmacies.FirstOrDefault();

            if (userPharmacy == null)
            {
                ProductsLB.ItemsSource = null;
                return;
            }

            int pharmacyId = userPharmacy.ID;

            var filtered = Core.Context.Products.Where(p => p.ProductPharmacies.Any(pp => pp.PharmacyID == pharmacyId && pp.Count > 0))
                .ToList();

            string search = SearchBox.Text.Trim().ToLower();

            if (!string.IsNullOrEmpty(search))
            {
                filtered = filtered
                    .Where(p => p.Name.ToLower().Contains(search))
                    .ToList();
            }

            if (CategoryCB.SelectedIndex > 0)
            {
                string selectedCategory =
                    (CategoryCB.SelectedItem as ComboBoxItem)?.Content.ToString();

                if (!string.IsNullOrEmpty(selectedCategory))
                {
                    filtered = filtered
                        .Where(p => p.Category.Any(c => c.Name == selectedCategory))
                        .ToList();
                }
            }

            switch (SortCB.SelectedIndex)
            {
                case 0:
                    filtered = filtered.OrderBy(p => p.Price).ToList();
                    break;

                case 1:
                    filtered = filtered.OrderByDescending(p => p.Price).ToList();
                    break;
            }

            ProductsLB.ItemsSource = filtered;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateFilters();
        }

        private void CategoryCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateFilters();
        }

        private void SortCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateFilters();
        }
    }
}
