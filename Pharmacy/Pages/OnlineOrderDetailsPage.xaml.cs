using Pharmacy.Model;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Pharmacy.Pages
{
    public partial class OnlineOrderDetailsPage : Page
    {
        private int orderId;
        private int pharmacyId;

        public OnlineOrderDetailsPage(int selectedOrderId, int selectedPharmacyId)
        {
            InitializeComponent();

            orderId = selectedOrderId;
            pharmacyId = selectedPharmacyId;

            LoadOrder();
        }

        private void LoadOrder()
        {
            var order = Core.Context.OnlineOrder
                .FirstOrDefault(o => o.ID == orderId);

            if (order == null)
            {
                MessageBox.Show("Заказ не найден!");
                return;
            }

            HeaderTB.Text = "Интернет-заказ №" + order.ID;

            OrderIDTB.Text += order.ID.ToString();
            OrderDateTB.Text += order.Date.ToString("dd.MM.yyyy");
            UserIDTB.Text += order.UserID.ToString();
            StatusTB.Text += order.Status.Name;
            PriceTB.Text += order.Price.ToString("N2") + " руб.";

            var products = Core.Context.SpecificallyProducts
                .Where(sp =>
                    sp.OnlineOrderID == orderId &&
                    sp.PharmaciesID == pharmacyId)
                .Select(sp => new
                {
                    Name = sp.Products.Name,
                    Price = sp.Products.Price,
                    ExpirationDate = sp.ExpirationDate
                })
                .ToList();

            OrderProductsDG.ItemsSource = products;
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        private void GiveOrderBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ReturnOrderBtn_Click(object sender, RoutedEventArgs e)
        {
            OnlineOrder order = Core.Context.OnlineOrder
                .FirstOrDefault(o => o.ID == orderId);

            if (order == null)
            {
                MessageBox.Show("Заказ не найден!");
                return;
            }

            if (NavigationService == null)
            {
                MessageBox.Show("Не удалось выполнить переход на страницу!");
                return;
            }

            NavigationService.Navigate(new ReturnOrderPage(orderId, pharmacyId));
        }

        private void GiveBtn_Click(object sender, RoutedEventArgs e)
        {

            Core.Context.OnlineOrder.Where(o => o.StatusID == 5);

            Core.Context.SaveChanges();

            StatusTB.Text += "Выдан";
        }
    }
}
