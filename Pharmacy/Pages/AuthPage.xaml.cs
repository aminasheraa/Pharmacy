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
        private int failedAttempts = 0;

        private const int AttemptsBeforeCaptcha = 3;

        public AuthPage()
        {
            InitializeComponent();

            CaptchaPanel.Visibility = Visibility.Collapsed;
            AuthPanel.Visibility = Visibility.Visible;
        }

        private void AuthBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginTB.Text) || string.IsNullOrWhiteSpace(PB.Password))
            {
                MessageBox.Show("Заполните логин и пароль.", "Ошибка", MessageBoxButton.OK);
                return;
            }

            if (CaptchaPanel.Visibility == Visibility.Visible)
            {
                MessageBox.Show("Сначала пройдите проверку", "Проверка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var user = Core.Context.Users.FirstOrDefault(u => u.Login == LoginTB.Text && u.Password == PB.Password);

            if (user != null)
            {
                Core.CurrentUser = user;
                failedAttempts = 0;

                if (user.RoleID == 1)
                {
                    NavigationService.Navigate(new ProductsPage());
                }
                else if (user.RoleID == 2)
                {
                    NavigationService.Navigate(new AdminPage(user));
                }
                else if (user.RoleID == 3)
                {

                }
            }
            else
            {
                failedAttempts++;

                MessageBox.Show("Неверный логин или пароль", "Ошибка авторизации", MessageBoxButton.OK);
                LoginTB.Clear();
                PB.Clear();

                if (failedAttempts >= AttemptsBeforeCaptcha)
                {
                    ShowCaptcha();
                }
            }
        }

        private void ShowCaptcha()
        {
            AuthPanel.Visibility = Visibility.Collapsed;
            CaptchaPanel.Visibility = Visibility.Visible;

            captchaInput.Clear();
            GenerateCaptcha();
        }

        private void HideCaptcha()
        {
            CaptchaPanel.Visibility = Visibility.Collapsed;
            AuthPanel.Visibility = Visibility.Visible;

            captchaInput.Clear();

            LoginTB.Clear();
            PB.Clear();

        }


        private void GenerateCaptcha()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ" + "abcdefghijklmnopqrstuvwxyz" + "0123456789";

            Random random = new Random();
            string result = "";

            for (int i = 0; i < 6; i++)
            {
                result += chars[random.Next(chars.Length)];
            }

            captcha.Text = result;
        }


        private void submitCaptcha_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(captchaInput.Text))
            {
                MessageBox.Show("Введите капчу", "Ошибка", MessageBoxButton.OK);
                return;
            }
            if (string.Equals(captchaInput.Text.Trim(), captcha.Text))
            {
                MessageBox.Show("Капча пройдена успешно", "Успех", MessageBoxButton.OK);
                failedAttempts = 0;
                HideCaptcha();
            }
            else
            {
                MessageBox.Show("Капча введена неверно. Попробуйте ещё раз", "Ошибка", MessageBoxButton.OK);
                captchaInput.Clear();
                GenerateCaptcha();
            }
        }

        private void textBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy || e.Command == ApplicationCommands.Cut || e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }
    }


}
