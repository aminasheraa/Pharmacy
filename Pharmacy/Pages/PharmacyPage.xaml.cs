using Pharmacy.Model;
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
    /// Логика взаимодействия для PharmacyPage.xaml
    /// </summary>
    public partial class PharmacyPage : Page
    {
        public PharmacyPage()
        {
            InitializeComponent();
            LoadProducts();
        }

        private void LoadProducts()
        {
            if (Core.CurrentPharmacy == null)
            {
                DataGridProducts.ItemsSource = null;
                MessageBox.Show("Аптека не выбрана.");
                return;
            }

            int pharmacyId = Core.CurrentPharmacy.ID;

            var products = Core.Context.ProductPharmacies.Where(p => p.PharmacyID == pharmacyId && p.Count > 0).ToList()
                .Select(p => new
                {
                    p.Products,
                    p.Count,
                    TotalSum = p.Products.Price * p.Count
                })
                .ToList();

            DataGridProducts.ItemsSource = products;
        }


        private void ButtonSupply_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new OrderSupplyPage());
        }

        private void InventoryBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new InventoryPage());
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}

