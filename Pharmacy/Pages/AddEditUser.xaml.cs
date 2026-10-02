using Pharmacy.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography;
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
    /// Логика взаимодействия для AddEditUser.xaml
    /// </summary>

    public partial class AddEditUser : Page
    {
        private Users currentuser;
        public AddEditUser(Users user)
        {
            InitializeComponent();
            currentuser = user;
            RoleCB.ItemsSource = Core.Context.Roles.ToList();
            if (user != null)
            {
                TitleTb.Text = "Редактирование пользователя";
                FIOBox.Text = currentuser.FIO;
                LoginBox.Text = currentuser.Login;
                PasswordBox.Text = currentuser.Password;
                BirthDP.Text = currentuser.DateOfBirth.ToString();
                RoleCB.SelectedItem = currentuser.Roles;

            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FIOBox.Text) || string.IsNullOrWhiteSpace(LoginBox.Text) || string.IsNullOrWhiteSpace(PasswordBox.Text) || BirthDP.SelectedDate == null || RoleCB.SelectedItem == null)
            {
                MessageBox.Show("Заполните все поля данными");
                return;
            }

            var selectedRole = RoleCB.SelectedItem as Roles; 

            if (currentuser != null)
            {
                currentuser.FIO = FIOBox.Text;
                currentuser.Login = LoginBox.Text;
                currentuser.Password = GetHash(PasswordBox.Text);
                currentuser.DateOfBirth = BirthDP.SelectedDate.Value;
                currentuser.RoleID = selectedRole.ID; 
            }
            else
            {
                Users newUser = new Users
                {
                    FIO = FIOBox.Text,
                    Login = LoginBox.Text,
                    Password = GetHash(PasswordBox.Text),
                    DateOfBirth = BirthDP.SelectedDate.Value,
                    RoleID = selectedRole.ID 
                };

                Core.Context.Users.Add(newUser);
            }

            Core.Context.SaveChanges();

            MessageBox.Show("Успешно добавлен/сохранён пользователь");
            NavigationService.GoBack();
        }
        public static string GetHash(String password)
        {
            using (var hash = SHA1.Create())
            {
                return
               string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password)).Select(x =>
               x.ToString("X2")));
            }
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}

  
