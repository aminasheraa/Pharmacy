using Pharmacy.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Pharmacy.Pages
{
    public partial class InventoryPage : Page
    {
        private List<InventoryRow> inventoryRows = new List<InventoryRow>();

        public InventoryPage()
        {
            InitializeComponent();

            // Получаем только аптеки, которыми руководит текущий пользователь
            var pharmacies = Core.Context.UserPharmacy
                .Where(up => up.UserID == Core.CurrentUser.ID)
                .Select(up => up.Pharmacies)
                .Where(p => p != null)
                .ToList();

            PharmacyCB.ItemsSource = pharmacies;

            if (PharmacyCB.Items.Count > 0)
            {
                PharmacyCB.SelectedIndex = 0;
            }
            else
            {
                InventoryDG.ItemsSource = null;
                HistoryDG.ItemsSource = null;

                MessageBox.Show(
                    "За вами не закреплено ни одной аптеки.",
                    "Информация",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            LoadHistory();
        }

        private void PharmacyCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadInventory();
            LoadHistory();
        }

        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadInventory();
        }

        private void LoadInventory()
        {
            Pharmacies pharmacy = PharmacyCB.SelectedItem as Pharmacies;

            if (pharmacy == null)
                return;

            var stocks = Core.Context.ProductPharmacies.Where(p => p.PharmacyID == pharmacy.ID).ToList();

            List<int> receivedSupplyIds = Core.Context.Supplies.Where(s => s.PharmaciesID == pharmacy.ID && s.IsReceived).Select(s => s.ID).ToList();

            var acceptedDetails = Core.Context.ProductsSupply.Where(ps => receivedSupplyIds.Contains(ps.SupplyID)).ToList();

            var acceptanceDifferences = acceptedDetails
                .GroupBy(ps => ps.ProductID)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(ps =>
                        (ps.AcceptedCount ?? ps.Count) - ps.Count)
                );

            var products = Core.Context.Products.ToList();

            inventoryRows = new List<InventoryRow>();

            foreach (Products product in products)
            {
                var stock = stocks.FirstOrDefault(p => p.ProductID == product.ID);

                int count = stock == null ? 0 : stock.Count;

                int difference = 0;

                if (acceptanceDifferences.ContainsKey(product.ID))
                {
                    difference = acceptanceDifferences[product.ID];
                }

                inventoryRows.Add(new InventoryRow
                {
                    ProductID = product.ID,
                    ProductName = product.Name,

                    ExpectedCount = count,
                    ActualCount = count,

                    Difference = difference,

                    Price = product.Price
                });
            }

            InventoryDG.ItemsSource = null;
            InventoryDG.ItemsSource = inventoryRows;

            UpdateSummary();
        }

        private void UpdateSummary()
        {
            int differences = inventoryRows.Count(
                p => p.Difference != 0);

            SummaryTB.Text = "Товаров: " + inventoryRows.Count + "; расхождений при приёмке: " + differences;
        }

        private void LoadHistory()
        {
            Pharmacies pharmacy = PharmacyCB.SelectedItem as Pharmacies;

            if (pharmacy == null)
                return;

            HistoryDG.ItemsSource = Core.Context.Inventory.Where(i => i.PharmacyID == pharmacy.ID).OrderByDescending(i => i.Date).ToList();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            Pharmacies pharmacy = PharmacyCB.SelectedItem as Pharmacies;
            if (pharmacy == null)
            {
                MessageBox.Show("Выберите аптеку.");
                return;
            }

            InventoryDG.CommitEdit(DataGridEditingUnit.Cell, true);
            InventoryDG.CommitEdit(DataGridEditingUnit.Row, true);
            if (inventoryRows.Count == 0)
            {
                MessageBox.Show("Нет товаров для инвентаризации.");
                return;
            }

            if (inventoryRows.Any(p => p.ActualCount < 0))
            {
                MessageBox.Show("Фактическое количество не может быть отрицательным.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Сохранить результаты инвентаризации?\n\n" +
                "Фактические остатки товаров будут обновлены в базе данных.",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
            {
                return;
            }

            using (var transaction = Core.Context.Database.BeginTransaction())
            {
                Inventory inventory = new Inventory
                {
                    Date = DateTime.Today,
                    PharmacyID = pharmacy.ID,

                    Quantity = inventoryRows.Sum(p => p.ActualCount),
                    Sum = inventoryRows.Sum(p => p.ActualCount * p.Price)
                };

                Core.Context.Inventory.Add(inventory);
                Core.Context.SaveChanges();

                foreach (InventoryRow row in inventoryRows)
                {
                    InventoryDetails detail =
                        new InventoryDetails
                        {
                            InventoryID = inventory.ID,
                            ProductID = row.ProductID,
                            ExpectedCount = row.ExpectedCount,
                            ActualCount = row.ActualCount
                        };

                    Core.Context.InventoryDetails.Add(detail);

                    ProductPharmacies stock = Core.Context.ProductPharmacies.FirstOrDefault(p => p.PharmacyID == pharmacy.ID && p.ProductID == row.ProductID);

                    if (stock == null)
                    {
                        stock = new ProductPharmacies
                        {
                            PharmacyID = pharmacy.ID,
                            ProductID = row.ProductID,
                            Count = row.ActualCount
                        };

                        Core.Context.ProductPharmacies.Add(stock);
                    }
                    else
                    {
                        stock.Count = row.ActualCount;
                    }
                }

                Core.Context.SaveChanges();

                transaction.Commit();

                MessageBox.Show("Инвентаризация успешно сохранена!\n" + "Остатки аптеки обновлены.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadInventory();
                LoadHistory();
            }
        }

        private void ExcelBtn_Click(object sender, RoutedEventArgs e)
        {
            Inventory selected = HistoryDG.SelectedItem as Inventory;

            if (selected == null)
            {
                MessageBox.Show("Выберите инвентаризацию в истории.");
                return;
            }

            var details = Core.Context.InventoryDetails.Where(d => d.InventoryID == selected.ID).ToList();

            Microsoft.Office.Interop.Excel.Application application = new Microsoft.Office.Interop.Excel.Application();

            Microsoft.Office.Interop.Excel.Workbook workbook = application.Workbooks.Add();

            Microsoft.Office.Interop.Excel.Worksheet worksheet = workbook.Worksheets[1];

            worksheet.Cells[1, 1] = "Отчёт об инвентаризации";
            worksheet.Cells[2, 1] = "Номер";
            worksheet.Cells[2, 2] = selected.ID;
            worksheet.Cells[3, 1] = "Дата";
            worksheet.Cells[3, 2] = selected.Date.ToString("dd.MM.yyyy");
            worksheet.Cells[5, 1] = "ID товара";
            worksheet.Cells[5, 2] = "Товар";
            worksheet.Cells[5, 3] = "По учёту";
            worksheet.Cells[5, 4] = "Фактически";
            worksheet.Cells[5, 5] = "Разница инвентаризации";

            int rowIndex = 6;

            foreach (InventoryDetails detail in details)
            {
                Products product = Core.Context.Products.FirstOrDefault(p => p.ID == detail.ProductID);

                worksheet.Cells[rowIndex, 1] = detail.ProductID;

                worksheet.Cells[rowIndex, 2] = product == null ? "" : product.Name;

                worksheet.Cells[rowIndex, 3] = detail.ExpectedCount;

                worksheet.Cells[rowIndex, 4] = detail.ActualCount;

                worksheet.Cells[rowIndex, 5] = detail.ActualCount - detail.ExpectedCount;

                rowIndex++;
            }

            Microsoft.Office.Interop.Excel.Range header = worksheet.Range["A5", "E5"];

            header.Font.Bold = true;

            header.HorizontalAlignment = Microsoft.Office.Interop.Excel.XlHAlign.xlHAlignCenter;

            worksheet.Columns.AutoFit();

            application.Visible = true;
        }

        private void WordBtn_Click(object sender, RoutedEventArgs e)
        {
            Inventory selected = HistoryDG.SelectedItem as Inventory;

            if (selected == null)
            {
                MessageBox.Show("Выберите инвентаризацию в истории.");
                return;
            }

            var details = Core.Context.InventoryDetails.Where(d => d.InventoryID == selected.ID).ToList();
            Microsoft.Office.Interop.Word.Application application = new Microsoft.Office.Interop.Word.Application();
            Microsoft.Office.Interop.Word.Document document = application.Documents.Add();
            Microsoft.Office.Interop.Word.Paragraph title = document.Paragraphs.Add();
            title.Range.Text = "Отчёт об инвентаризации";
            title.Range.Font.Bold = 1;
            title.Range.Font.Size = 16;

            title.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;

            title.Range.InsertParagraphAfter();

            document.Paragraphs.Add().Range.Text = "Номер отчёта: " + selected.ID;

            document.Paragraphs.Add().Range.Text = "Дата: " + selected.Date.ToString("dd.MM.yyyy");

            document.Paragraphs.Add().Range.Text = "Аптека: " + pharmacyName(selected.PharmacyID);

            Microsoft.Office.Interop.Word.Table table = document.Tables.Add(document.Paragraphs.Add().Range, details.Count + 1, 5);
            table.Cell(1, 1).Range.Text = "ID товара";
            table.Cell(1, 2).Range.Text = "Товар";
            table.Cell(1, 3).Range.Text = "По учёту";
            table.Cell(1, 4).Range.Text = "Фактически";
            table.Cell(1, 5).Range.Text = "Разница инвентаризации";

            int rowIndex = 2;

            foreach (InventoryDetails detail in details)
            {
                Products product = Core.Context.Products.FirstOrDefault(p => p.ID == detail.ProductID);

                table.Cell(rowIndex, 1).Range.Text = detail.ProductID.ToString();

                table.Cell(rowIndex, 2).Range.Text = product == null ? "" : product.Name;

                table.Cell(rowIndex, 3).Range.Text = detail.ExpectedCount.ToString();

                table.Cell(rowIndex, 4).Range.Text = detail.ActualCount.ToString();

                table.Cell(rowIndex, 5).Range.Text = (detail.ActualCount - detail.ExpectedCount).ToString();

                rowIndex++;
            }

            table.Borders.Enable = 1;
            table.Rows[1].Range.Bold = 1;

            application.Visible = true;
        }

        private string pharmacyName(int pharmacyID)
        {
            Pharmacies pharmacy = Core.Context.Pharmacies.FirstOrDefault(p => p.ID == pharmacyID);

            return pharmacy == null ? "" : pharmacy.Address;
        }

        private void InventoryDG_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                Dispatcher.BeginInvoke(new Action(UpdateSummary));
            }
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }

    public class InventoryRow
    {
        public int ProductID { get; set; }

        public string ProductName { get; set; }

        public int ExpectedCount { get; set; }

        public decimal Price { get; set; }

        public int ActualCount { get; set; }

        public int Difference { get; set; }
    }
}