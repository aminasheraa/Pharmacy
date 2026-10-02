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
    /// Логика взаимодействия для ManagerPage.xaml
    /// </summary>
    public partial class ManagerPage : Page
    {
        public ManagerPage()
        {
            InitializeComponent();

            ManagerName.Text = Core.CurrentUser.FIO;
            if (Core.CurrentUser == null)
            {
                PharmaciesLB.ItemsSource = null;
                return;
            }

            PharmaciesLB.ItemsSource = Core.CurrentUser.Pharmacies.ToList();
        }

        private void OrderBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void InventoryBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ExitBtn_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Вы уверены, что хотите выйти из аккаунта?", "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Core.CurrentUser = null;

                NavigationService.Navigate(new AuthPage());

            }
        }
    }
}
