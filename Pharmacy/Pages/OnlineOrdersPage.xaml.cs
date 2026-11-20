
using Pharmacy.Model;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Pharmacy.Pages
{
    public partial class OnlineOrdersPage : Page
    {
        private int? currentPharmacyId;

        public OnlineOrdersPage()
        {
            InitializeComponent();

            GetUserPharmacy();
            LoadOrders();
        }

        private void GetUserPharmacy()
        {
            if (Core.CurrentUser == null)
                return;

            var userPharmacy = Core.Context.UserPharmacy.FirstOrDefault(up => up.UserID == Core.CurrentUser.ID);

            if (userPharmacy != null)
                currentPharmacyId = userPharmacy.PharmacyID;
        }

        private void LoadOrders()
        {
            if (currentPharmacyId == null)
            {
                OrdersLB.ItemsSource = null;
                return;
            }

            string search = SearchBox.Text.Trim();

            var orders = Core.Context.OnlineOrder.Where(o => Core.Context.SpecificallyProducts.Any(sp => sp.OnlineOrderID == o.ID && sp.PharmaciesID == currentPharmacyId.Value))
            .OrderByDescending(o => o.Date).ToList();

            if (!string.IsNullOrEmpty(search))
            {
                orders = orders.Where(o => o.ID.ToString().Contains(search)).ToList();
            }
            OrdersLB.ItemsSource = orders;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadOrders();
        }

        private void OrdersLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnlineOrder selectedOrder = OrdersLB.SelectedItem as OnlineOrder;

            if (selectedOrder == null || currentPharmacyId == null)
                return;

            
            NavigationService.Navigate(new OnlineOrderDetailsPage(selectedOrder.ID, currentPharmacyId.Value));
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }
    }
}
