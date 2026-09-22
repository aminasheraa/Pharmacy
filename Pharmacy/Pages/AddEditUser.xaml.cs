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
            if (user != null)
            {
                FIOBox.Text = currentuser.FIO;
                LoginBox.Text = currentuser.Login;
                PasswordBox.Text = currentuser.Password;
                BirthDP.Text = currentuser.DateOfBirth.ToString();
                /*                RoleCB.SelectedItem = user.Roles.Name();
                */
            }
        }

        public void LoadDetails()
        {
            
        }
    }
}
