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
                RoleCB.SelectedIndex = currentuser.RoleID + 1;
                
            }
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            if (FIOBox.Text == null || LoginBox.Text == null || PasswordBox == null || BirthDP == null || RoleCB.SelectedItem == null)
            {
                MessageBox.Show("Заполните все поля данными");
                return;
            }

            currentuser.FIO = FIOBox.Text;
            currentuser.Login = LoginBox.Text;
            currentuser.Password = PasswordBox.Text;
            currentuser.DateOfBirth = (DateTime)BirthDP.SelectedDate;
            currentuser.RoleID = RoleCB.SelectedIndex + 1;

            if (currentuser.ID == 0)
                Core.Context.Users.Add(currentuser);
            Core.Context.SaveChanges();
            MessageBox.Show("Успешно добавлен/сохранён пользователь");
            NavigationService.GoBack();

            
        }
    }
}
