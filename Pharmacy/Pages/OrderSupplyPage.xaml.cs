using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Pharmacy.Model;

namespace Pharmacy.Pages
{
    public partial class OrderSupplyPage : Page
    {
        private ObservableCollection<ProductsSupply> supplyItems = new ObservableCollection<ProductsSupply>();

        public OrderSupplyPage()
        {
            InitializeComponent();
            SupplyDG.ItemsSource = supplyItems;
            LoadData();
        }

        private void LoadData()
        {
            StorageCB.ItemsSource = Core.Context.Storages.ToList();
            ProductCB.ItemsSource = Core.Context.Products.ToList();
            if (StorageCB.Items.Count > 0)
                StorageCB.SelectedIndex = 0;
            if (ProductCB.Items.Count > 0)
                ProductCB.SelectedIndex = 0;
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductCB.SelectedItem == null)
            {
                MessageBox.Show("Выберите товар.");
                return;
            }

            int count;

            if (!int.TryParse(CountTB.Text, out count) || count <= 0)
            {
                MessageBox.Show("Введите корректное количество.");
                return;
            }

            Products product = ProductCB.SelectedItem as Products;

            if (product == null)
                return;

            ProductsSupply existingItem = supplyItems.FirstOrDefault(p => p.ProductID == product.ID);

            if (existingItem != null)
            {
                existingItem.Count += count;
            }
            else
            {
                supplyItems.Add(new ProductsSupply
                {
                    ProductID = product.ID,
                    Products = product,
                    Count = count
                });
            }

            UpdateTotal();
            SupplyDG.Items.Refresh();

            CountTB.Text = "1";
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            ProductsSupply selectedItem = SupplyDG.SelectedItem as ProductsSupply;

            if (selectedItem == null)
            {
                MessageBox.Show("Выберите товар, который нужно удалить.");
                return;
            }

            supplyItems.Remove(selectedItem);
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            decimal total = supplyItems.Sum(p => p.Products.Price * p.Count);
            TotalTB.Text = total.ToString("") + " руб.";
        }



        private void SaveSupplyBtn_Click(object sender, RoutedEventArgs e)
        {
            if (StorageCB.SelectedItem == null)
            {
                MessageBox.Show("Выберите склад.");
                return;
            }

            if (supplyItems.Count == 0)
            {
                MessageBox.Show("Добавьте хотя бы один товар.");
                return;
            }

            if (Core.CurrentUser == null)
            {
                MessageBox.Show("Не удалось определить текущего пользователя.");
                return;
            }

            Storages storage = StorageCB.SelectedItem as Storages;

            if (storage == null)
            {
                MessageBox.Show("Не удалось определить склад.");
                return;
            }

            var pharmacy = Core.CurrentUser.Pharmacies.FirstOrDefault();

            if (pharmacy == null)
            {
                MessageBox.Show("Для пользователя не найдена аптека.");
                return;
            }

            decimal total = supplyItems.Sum(p => p.Products.Price * p.Count);

            Supplies supply = new Supplies();
            supply.Date = DateTime.Now;
            supply.UserID = Core.CurrentUser.ID;
            supply.Price = total;
            supply.StoragesID = storage.ID;
            supply.PharmaciesID = pharmacy.ID;

            Core.Context.Supplies.Add(supply);
            Core.Context.SaveChanges();

            foreach (ProductsSupply item in supplyItems)
            {
                ProductsSupply newItem = new ProductsSupply();
                newItem.ProductID = item.ProductID;
                newItem.SupplyID = supply.ID;
                newItem.Count = item.Count;

                Core.Context.ProductsSupply.Add(newItem);

                ProductPharmacies stock = Core.Context.ProductPharmacies.FirstOrDefault(
                    p => p.ProductID == item.ProductID &&
                         p.PharmacyID == pharmacy.ID
                );

                if (stock == null)
                {
                    ProductPharmacies newStock = new ProductPharmacies();
                    newStock.ProductID = item.ProductID;
                    newStock.PharmacyID = pharmacy.ID;
                    newStock.Count = item.Count;

                    Core.Context.ProductPharmacies.Add(newStock);
                }
                else
                {
                    stock.Count += item.Count;
                }
            }

            Core.Context.SaveChanges();
            MessageBox.Show("Поставка успешно сохранена!","Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            supplyItems.Clear();
            SupplyDG.Items.Refresh();
            UpdateTotal();
            CountTB.Text = "1";
            NavigationService.Navigate(new ManagerPage());
            
        }



        private void ProductSum_Loaded(object sender, RoutedEventArgs e)
        {
            TextBlock textBlock = sender as TextBlock;

            if (textBlock == null)
                return;

            ProductsSupply item = textBlock.DataContext as ProductsSupply;

            if (item != null && item.Products != null)
            {
                decimal sum = item.Products.Price * item.Count;
                textBlock.Text = sum.ToString("") + " руб.";
            }
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}