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
using System.Data.Entity;

namespace Pharmacy.Pages
{
    /// <summary>
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        public AdminPage(Users user)
        {
            InitializeComponent();
        }

        private void UsersLB_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var users = Core.Context.Users.Include("Roles").Where(u => u.RoleID == 1 || u.RoleID == 3).ToList();
            UsersLB.ItemsSource = users;
        }

        private void AddUserBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(null);

        }

        private void EditUserBtn_Click(object sender, RoutedEventArgs e)
        {
            var user = (sender as Button).DataContext as Users;
            NavigationService.Navigate(new AddEditUser(user));
        }
    }
}
