using Pharmacy.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;

namespace Pharmacy.Pages
{
    public partial class SupplyAcceptancePage : Page
    {
        private List<SupplyProductRow> productRows =
            new List<SupplyProductRow>();

        private int selectedSupplyId = 0;

        public SupplyAcceptancePage()
        {
            InitializeComponent();

            if (Core.CurrentUser == null)
            {
                MessageBox.Show("Не удалось определить сотрудника!");
                return;
            }

            LoadPharmacies();
        }

        private void LoadPharmacies()
        {
            List<int> pharmacyIds = Core.Context.UserPharmacy
                .Where(up => up.UserID == Core.CurrentUser.ID)
                .Select(up => up.PharmacyID)
                .Distinct()
                .ToList();

            PharmacyCB.ItemsSource = Core.Context.Pharmacies
                .Where(p => pharmacyIds.Contains(p.ID))
                .ToList();

            if (PharmacyCB.Items.Count > 0)
            {
                PharmacyCB.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show("У сотрудника не назначена аптека!");
            }
        }

        private void PharmacyCB_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            selectedSupplyId = 0;

            SuppliesDG.ItemsSource = null;
            ProductsDG.ItemsSource = null;

            productRows.Clear();

            AcceptBtn.IsEnabled = false;

            SummaryTB.Text = "Выберите поставку";

            LoadSupplies();
            LoadHistory();
        }

        private void LoadSupplies()
        {
            Pharmacies pharmacy = PharmacyCB.SelectedItem as Pharmacies;

            if (pharmacy == null)
                return;

            var supplies = Core.Context.Supplies
                .Where(s =>
                    s.PharmaciesID == pharmacy.ID &&
                    !s.IsReceived)
                .OrderBy(s => s.Date)
                .ToList();

            List<SupplyHeaderRow> result =
                new List<SupplyHeaderRow>();

            foreach (Supplies supply in supplies)
            {
                Storages storage = Core.Context.Storages
                    .FirstOrDefault(st => st.ID == supply.StoragesID);

                result.Add(new SupplyHeaderRow
                {
                    ID = supply.ID,
                    Date = supply.Date,
                    Price = supply.Price,
                    StorageAddress = storage == null
                        ? ""
                        : storage.Address
                });
            }

            SuppliesDG.ItemsSource = result;
        }

        private void SuppliesDG_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            SupplyHeaderRow selected =
                SuppliesDG.SelectedItem as SupplyHeaderRow;

            if (selected == null)
                return;

            selectedSupplyId = selected.ID;

            LoadProducts(selectedSupplyId);
        }

        private void LoadProducts(int supplyId)
        {
            productRows.Clear();

            var details = Core.Context.ProductsSupply
                .Where(ps => ps.SupplyID == supplyId)
                .ToList();

            foreach (ProductsSupply detail in details)
            {
                Products product = Core.Context.Products
                    .FirstOrDefault(p => p.ID == detail.ProductID);

                if (product == null)
                    continue;

                productRows.Add(new SupplyProductRow
                {
                    ProductSupplyID = detail.ID,
                    ProductID = detail.ProductID,
                    ProductName = product.Name,
                    ExpectedCount = detail.Count,
                    AcceptedCount = detail.AcceptedCount ?? detail.Count,
                    ExpirationDate = detail.ExpirationDate,
                    Price = product.Price
                });
            }

            ProductsDG.ItemsSource = null;
            ProductsDG.ItemsSource = productRows;

            UpdateSummary();

            AcceptBtn.IsEnabled = productRows.Count > 0;
        }

        private void UpdateSummary()
        {
            int expected = productRows.Sum(p => p.ExpectedCount);

            int accepted = productRows.Sum(p => p.AcceptedCount);

            int difference = accepted - expected;

            SummaryTB.Text =
                "Позиций: " + productRows.Count +
                "; по накладной: " + expected +
                "; принято: " + accepted +
                "; расхождение: " + difference;
        }

        private void ProductsDG_CellEditEnding(
            object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        ProductsDG.Items.Refresh();
                        UpdateSummary();
                    }),
                    System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        private void AcceptBtn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedSupplyId == 0)
            {
                MessageBox.Show("Выберите поставку!");
                return;
            }

            ProductsDG.CommitEdit(DataGridEditingUnit.Cell, true);
            ProductsDG.CommitEdit(DataGridEditingUnit.Row, true);

            if (productRows.Count == 0)
            {
                MessageBox.Show("В поставке нет товаров!");
                return;
            }

            if (productRows.Any(p => p.AcceptedCount < 0))
            {
                MessageBox.Show(
                    "Количество принятого товара не может быть отрицательным!");
                return;
            }

            if (productRows.Any(p =>
                p.AcceptedCount > 0 &&
                p.ExpirationDate == null))
            {
                MessageBox.Show(
                    "Укажите срок годности для каждого принятого препарата!");
                return;
            }

            if (productRows.Any(p =>
                p.AcceptedCount > 0 &&
                p.ExpirationDate.Value.Date <= DateTime.Today))
            {
                MessageBox.Show(
                    "Срок годности принятого препарата должен быть позже текущей даты!");
                return;
            }

            int expected = productRows.Sum(p => p.ExpectedCount);
            int accepted = productRows.Sum(p => p.AcceptedCount);

            if (accepted > expected)
            {
                MessageBoxResult answer = MessageBox.Show(
                    "Фактически принято товара больше, чем указано в накладной.\n" +
                    "По накладной: " + expected +
                    "\nФактически: " + accepted +
                    "\n\nПродолжить приёмку?",
                    "Обнаружено расхождение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (answer != MessageBoxResult.Yes)
                    return;
            }

            MessageBoxResult confirm = MessageBox.Show(
                "Подтвердить приёмку поставки №" + selectedSupplyId +
                "?\n\nПо накладной: " + expected +
                "\nФактически принято: " + accepted +
                "\nРасхождение: " + (accepted - expected),
                "Подтверждение приёмки",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes)
                return;

            using (var transaction =
                Core.Context.Database.BeginTransaction())
            {
                Supplies supply = Core.Context.Supplies
                    .FirstOrDefault(s => s.ID == selectedSupplyId);

                if (supply == null)
                {
                    throw new Exception("Поставка не найдена!");
                }

                if (supply.IsReceived)
                {
                    throw new Exception(
                        "Эта поставка уже была принята!");
                }

                Pharmacies pharmacy =
                    PharmacyCB.SelectedItem as Pharmacies;

                if (pharmacy == null ||
                    pharmacy.ID != supply.PharmaciesID)
                {
                    throw new Exception(
                        "Поставка не относится к выбранной аптеке!");
                }

                foreach (SupplyProductRow row in productRows)
                {
                    ProductsSupply detail =
                        Core.Context.ProductsSupply
                        .FirstOrDefault(ps =>
                            ps.ID == row.ProductSupplyID);

                    if (detail == null ||
                        detail.SupplyID != selectedSupplyId)
                    {
                        throw new Exception(
                            "Не удалось найти товар в поставке!");
                    }

                    detail.AcceptedCount = row.AcceptedCount;

                    detail.ExpirationDate = row.AcceptedCount > 0
                        ? row.ExpirationDate.Value.Date
                        : (DateTime?)null;

                    if (row.AcceptedCount == 0)
                        continue;

                    ProductPharmacies stock =
                        Core.Context.ProductPharmacies
                        .FirstOrDefault(p =>
                            p.ProductID == row.ProductID &&
                            p.PharmacyID == pharmacy.ID);

                    if (stock == null)
                    {
                        stock = new ProductPharmacies
                        {
                            ProductID = row.ProductID,
                            PharmacyID = pharmacy.ID,
                            Count = 0
                        };

                        Core.Context.ProductPharmacies.Add(stock);
                    }

                    stock.Count += row.AcceptedCount;

                    for (int i = 0; i < row.AcceptedCount; i++)
                    {
                        SpecificallyProducts newProduct =
                            new SpecificallyProducts
                            {
                                ProductsID = row.ProductID,
                                PharmaciesID = pharmacy.ID,
                                ExpirationDate =
                                    row.ExpirationDate.Value.Date,
                                OnlineOrderID = null,
                                InStock = true
                            };

                        Core.Context.SpecificallyProducts.Add(
                            newProduct);
                    }
                }

                supply.IsReceived = true;
                supply.ReceivedDate = DateTime.Today;

                Core.Context.SaveChanges();

                transaction.Commit();
            }



            MessageBox.Show("Приёмка поставки успешно завершена!\n" + "Принято товаров: " + accepted + "\nРасхождение: " + (accepted - expected),
            "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            selectedSupplyId = 0;

            ProductsDG.ItemsSource = null;
            SuppliesDG.SelectedItem = null;

            productRows.Clear();

            AcceptBtn.IsEnabled = false;

            SummaryTB.Text = "Выберите поставку";

            LoadSupplies();
            LoadHistory();
        }

     

        private void LoadHistory()
        {
            Pharmacies pharmacy = PharmacyCB.SelectedItem as Pharmacies;

            if (pharmacy == null)
                return;

            var history = Core.Context.Supplies
                .Where(s =>
                    s.PharmaciesID == pharmacy.ID &&
                    s.IsReceived)
                .OrderByDescending(s => s.ReceivedDate)
                .ToList();

            List<SupplyHistoryRow> result =
                new List<SupplyHistoryRow>();

            foreach (Supplies supply in history)
            {
                result.Add(new SupplyHistoryRow
                {
                    ID = supply.ID,
                    Date = supply.Date,
                    ReceivedDate = supply.ReceivedDate,
                    Price = supply.Price
                });
            }

            HistoryDG.ItemsSource = result;
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            selectedSupplyId = 0;

            SuppliesDG.SelectedItem = null;
            SuppliesDG.ItemsSource = null;

            ProductsDG.ItemsSource = null;

            productRows.Clear();

            AcceptBtn.IsEnabled = false;

            SummaryTB.Text = "Выберите поставку";

            LoadSupplies();
            LoadHistory();
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null &&
                NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    } 
}

    public class SupplyHeaderRow
    {
        public int ID { get; set; }

        public DateTime Date { get; set; }

        public string StorageAddress { get; set; }

        public decimal Price { get; set; }
    }

    public class SupplyProductRow : INotifyPropertyChanged
    {
        private int acceptedCount;
        private DateTime? expirationDate;

        public int ProductSupplyID { get; set; }

        public int ProductID { get; set; }

        public string ProductName { get; set; }

        public int ExpectedCount { get; set; }

        public decimal Price { get; set; }

        public int AcceptedCount
        {
            get { return acceptedCount; }
            set
            {
                acceptedCount = value;
                OnPropertyChanged("AcceptedCount");
                OnPropertyChanged("Difference");
            }
        }

        public DateTime? ExpirationDate
        {
            get { return expirationDate; }
            set
            {
                expirationDate = value;
                OnPropertyChanged("ExpirationDate");
            }
        }

        public int Difference
        {
            get { return AcceptedCount - ExpectedCount; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class SupplyHistoryRow
    {
        public int ID { get; set; }

        public DateTime Date { get; set; }

        public DateTime? ReceivedDate { get; set; }

        public decimal Price { get; set; }
    }

