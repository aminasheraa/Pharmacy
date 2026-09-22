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
    /// Логика взаимодействия для AuthPage.xaml
    /// </summary>
    public partial class AuthPage : Page
    {
        public AuthPage()
        {
            InitializeComponent();
        }

        private void AuthBtn_Click(object sender, RoutedEventArgs e)
        {
            if (LoginTB == null || PB == null)
            {
                MessageBox.Show("Заполните все поля");
            }

            var user = Core.Context.Users.FirstOrDefault(u => u.Login == LoginTB.Text && u.Password == PB.Password);
            if (user != null)
            {
                MessageBox.Show("Успешная авторизация");
                Core.CurrentUser = user;
                
                if (user.RoleID == 1)
                {

                }
                else if (user.RoleID == 2)
                {
                    NavigationService.Navigate(new AdminPage(user));
                }
                else if (user.RoleID == 3)
                {

                }


            }
            else if (user == null)
            {
                MessageBox.Show("Неверный логин или пароль");

            }







        }
    }
}
