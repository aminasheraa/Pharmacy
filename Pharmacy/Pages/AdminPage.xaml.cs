using Pharmacy.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            LoadUsers();
        }

        private void LoadUsers()
        {
            var users = Core.Context.Users.Include("Roles").Where(u => u.RoleID == 1 || u.RoleID == 3).ToList();
            UsersLB.ItemsSource = users;
        }

        private void AddUserBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddEditUser(null));
            LoadUsers();


        }

        private void EditUserBtn_Click(object sender, RoutedEventArgs e)
        {
            var user = (sender as Button).DataContext as Users;
            NavigationService.Navigate(new AddEditUser(user));
        }





        private void DeleteUserBtn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            var user = btn.DataContext as Users;
            if (user == null) return;

            var realUser = Core.Context.Users
                .FirstOrDefault(u => u.ID == user.ID);

            if (realUser == null)
            {
                MessageBox.Show("Пользователь не найден.");
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Удалить пользователя " + realUser.FIO +
                " и все связанные с ним данные?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            var userPharmacies = Core.Context.UserPharmacy.Where(up => up.UserID == realUser.ID).ToList();

            Core.Context.UserPharmacy.RemoveRange(userPharmacies);

            var orders = Core.Context.OnlineOrder.Where(o => o.UserID == realUser.ID).ToList();

            foreach (var order in orders)
            {
                var orderProducts = Core.Context.SpecificallyProducts.Where(p => p.OnlineOrderID == order.ID).ToList();
                Core.Context.SpecificallyProducts.RemoveRange(orderProducts);
            }

            Core.Context.OnlineOrder.RemoveRange(orders);

            var supplies = Core.Context.Supplies.Where(s => s.UserID == realUser.ID).ToList();

            foreach (var supply in supplies)
            {
                var supplyProducts = Core.Context.ProductsSupply
                    .Where(p => p.SupplyID == supply.ID)
                    .ToList();

                Core.Context.ProductsSupply.RemoveRange(supplyProducts);
            }

            Core.Context.Supplies.RemoveRange(supplies);
            Core.Context.Users.Remove(realUser);
            Core.Context.SaveChanges();

            LoadUsers();

            MessageBox.Show("Пользователь и связанные данные удалены","Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUsers();

        }
    }
}
