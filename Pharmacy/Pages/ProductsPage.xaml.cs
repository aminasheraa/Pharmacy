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
using Pharmacy.Model;

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
            CategoryCB.SelectedIndex = 0;
            SortCB.SelectedIndex = 0;
            UpdateFilters();
        }

        private int? GetUserPharmacyId()
        {
            if (Core.CurrentUser == null)
                return null;

            var userPharmacy = Core.Context.UserPharmacy.FirstOrDefault(up => up.UserID == Core.CurrentUser.ID);

            if (userPharmacy == null)
                return null;

            return userPharmacy.PharmacyID;
        }
        private void UpdateFilters()
        {
            int? pharmacyId = GetUserPharmacyId();

            if (pharmacyId == null)
            {
                ProductsLB.ItemsSource = null;
                return;
            }

            var filtered = Core.Context.ProductPharmacies
                .Where(p => p.PharmacyID == pharmacyId.Value && p.Count > 0)
                .ToList();

            string search = SearchBox.Text.Trim().ToLower();

            if (!string.IsNullOrEmpty(search))
            {
                filtered = filtered.Where(p => p.Products.Name.ToLower().Contains(search)).ToList();
            }

            if (CategoryCB.SelectedIndex > 0)
            {
                string selectedCategory = (CategoryCB.SelectedItem as ComboBoxItem)?.Content.ToString();

                if (!string.IsNullOrEmpty(selectedCategory))
                {
                    filtered = filtered.Where(p => p.Products.Category.Any(c => c.Name == selectedCategory)).ToList();
                }
            }

            switch (SortCB.SelectedIndex)
            {
                case 0:
                    filtered = filtered.OrderBy(p => p.Products.Price).ToList();
                    break;

                case 1:
                    filtered = filtered.OrderByDescending(p => p.Products.Price).ToList();
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

        private void OpenProductBtn_Click(object sender, RoutedEventArgs e)
        {
            ProductPharmacies productPharmacy =
                (sender as Button).DataContext as ProductPharmacies;

            if (productPharmacy == null)
            {
                MessageBox.Show("Не удалось получить товар!");
                return;
            }

            NavigationService.Navigate(new ProductPage(productPharmacy.Products));
        }

        private void LogOutBtn_Click(object sender, RoutedEventArgs e)
        {

            var result = MessageBox.Show("Вы уверены, что хотите выйти из аккаунта?", "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Core.CurrentUser = null;

                NavigationService.Navigate(new AuthPage());

            }
        }

        private void OnlineOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new OnlineOrdersPage());
        }
    }
}
