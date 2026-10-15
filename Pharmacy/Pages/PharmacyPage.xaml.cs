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
            if (Core.CurrentUser == null)
            {
                DataGridProducts.ItemsSource = null;
                return;
            }

            var pharmacy = Core.CurrentUser.Pharmacies.FirstOrDefault();

            if (pharmacy == null)
            {
                DataGridProducts.ItemsSource = null;
                return;
            }

            int pharmacyId = pharmacy.ID;

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

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

