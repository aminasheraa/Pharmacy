
using Microsoft.Win32;
using Pharmacy.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Pharmacy.Pages
{
    public partial class ReturnOrderPage : Page
    {
        private int orderId;
        private int pharmacyId;

        private OnlineOrder currentOrder;

        private List<ReturnProductRow> products;

        public ReturnOrderPage(int selectedOrderId, int selectedPharmacyId)
        {
            InitializeComponent();

            orderId = selectedOrderId;
            pharmacyId = selectedPharmacyId;

            LoadOrder();
            LoadReasons();
        }

        private void LoadOrder()
        {
            currentOrder = Core.Context.OnlineOrder.FirstOrDefault(o => o.ID == orderId);

            if (currentOrder == null)
            {
                MessageBox.Show("Заказ не найден!");
                return;
            }

            bool belongsToPharmacy = Core.Context.SpecificallyProducts.Any(sp => sp.OnlineOrderID == orderId && sp.PharmaciesID == pharmacyId);

            if (!belongsToPharmacy)
            {
                MessageBox.Show("Заказ не относится к данной аптеке!");
                return;
            }

            HeaderTB.Text = "Оформление возврата №" + orderId;

            OrderIDTB.Text = currentOrder.ID.ToString();
            OrderDateTB.Text = currentOrder.Date.ToString("dd.MM.yyyy");
            UserIDTB.Text = currentOrder.UserID.ToString();
            StatusTB.Text = currentOrder.Status.Name;
            PriceTB.Text = currentOrder.Price.ToString("N2") + " руб.";

            products = Core.Context.SpecificallyProducts
                .Where(sp =>
                    sp.OnlineOrderID == orderId &&
                    sp.PharmaciesID == pharmacyId)
                .Select(sp => new ReturnProductRow
                {
                    Name = sp.Products.Name,
                    Price = sp.Products.Price,
                    ExpirationDate = sp.ExpirationDate,
                    Quantity = 1
                })
                .ToList();

            ProductsDG.ItemsSource = products;

            bool alreadyReturned = Core.Context.OnlineOrderReturns
                .Any(r => r.OnlineOrderID == orderId);

            if (alreadyReturned)
            {
                MessageBox.Show("Для данного заказа возврат уже оформлен!");

                SaveReturnBtn.IsEnabled = false;
            }
        }

        private void LoadReasons()
        {
            ReasonCB.ItemsSource = Core.Context.ReturnReasons.OrderBy(r => r.ID).ToList();
        }

        private void SaveReturnBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentOrder == null)
                return;

            if (ReasonCB.SelectedValue == null)
            {
                MessageBox.Show("Выберите причину возврата!");
                return;
            }

            if (Core.CurrentUser == null)
            {
                MessageBox.Show("Не удалось определить сотрудника!");
                return;
            }

            bool alreadyReturned = Core.Context.OnlineOrderReturns.Any(r => r.OnlineOrderID == orderId);

            if (alreadyReturned)
            {
                MessageBox.Show("Возврат по этому заказу уже оформлен!");
                return;
            }

            try
            {
                OnlineOrderReturns newReturn = new OnlineOrderReturns
                {
                    OnlineOrderID = orderId,
                    UserID = Core.CurrentUser.ID,
                    ReturnDate = DateTime.Today,
                    ReasonID = (int)ReasonCB.SelectedValue,
                    Comment = CommentTB.Text.Trim()
                };

                Core.Context.OnlineOrderReturns.Add(newReturn);

                Core.Context.SaveChanges();

                MessageBox.Show("Возврат успешно оформлен!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                SaveReturnBtn.IsEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при оформлении возврата: " + ex.Message);
            }
        }

        private void GenerateDocumentBtn_Click(object sender, RoutedEventArgs e)
        {
            if (currentOrder == null || products == null || products.Count == 0)
            {
                MessageBox.Show("Нет данных для формирования накладной!");
                return;
            }

            if (ReasonCB.SelectedValue == null)
            {
                MessageBox.Show("Выберите причину возврата!");
                return;
            }

            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Сохранение накладной на возврат",
                Filter = "Документ Word (*.docx)|*.docx",
                FileName = "Накладная_возврат_заказа_" + orderId + ".docx"
            };

            if (dialog.ShowDialog() != true)
                return;

            Microsoft.Office.Interop.Word.Application application = null;
            Microsoft.Office.Interop.Word.Document document = null;

            application = new Microsoft.Office.Interop.Word.Application();

            document = application.Documents.Add();

            document.PageSetup.Orientation = Microsoft.Office.Interop.Word.WdOrientation.wdOrientLandscape;

            document.PageSetup.TopMargin = 35;
            document.PageSetup.BottomMargin = 35;
            document.PageSetup.LeftMargin = 30;
            document.PageSetup.RightMargin = 30;

            document.Content.Font.Name = "Times New Roman";
            document.Content.Font.Size = 9;

            Microsoft.Office.Interop.Word.Paragraph title = document.Paragraphs.Add();

            title.Range.Text = "ТОВАРНО-ТРАНСПОРТНАЯ НАКЛАДНАЯ";
            title.Range.Font.Name = "Times New Roman";
            title.Range.Font.Size = 16;
            title.Range.Font.Bold = 1;

            title.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;

            title.Range.InsertParagraphAfter();

            document.Paragraphs.Add().Range.Text = "Фирма: _________________________________________________";

            document.Paragraphs.Add().Range.Text =
                "Грузоотправитель: Аптека № " + pharmacyId;

            document.Paragraphs.Add().Range.Text = "Грузополучатель: _________________________________________";

            document.Paragraphs.Add().Range.Text = "Основание: Возврат товара по интернет-заказу № " + orderId;

            document.Paragraphs.Add().Range.Text = "Дата заказа: " + currentOrder.Date.ToString("dd.MM.yyyy") + "     Дата возврата: " + DateTime.Today.ToString("dd.MM.yyyy");

            document.Paragraphs.Add().Range.Text = "Причина возврата: " + ReasonCB.Text;

            document.Paragraphs.Add().Range.Text = "Комментарий: " + CommentTB.Text.Trim();

            Microsoft.Office.Interop.Word.Paragraph section = document.Paragraphs.Add();

            section.Range.Text = "I. ТОВАРНЫЙ РАЗДЕЛ (заполняется грузоотправителем)";

            section.Range.Font.Bold = 1;
            section.Range.Font.Size = 12;

            section.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;

            section.Range.InsertParagraphAfter();

            int rowsCount = products.Count + 2;

            Microsoft.Office.Interop.Word.Range tableRange = document.Paragraphs.Add().Range;

            Microsoft.Office.Interop.Word.Table table = document.Tables.Add(tableRange, rowsCount, 11);

            table.Borders.Enable = 1;
            table.Range.Font.Name = "Times New Roman";
            table.Range.Font.Size = 7;

            table.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;

            table.AllowAutoFit = true;

            table.AutoFitBehavior(Microsoft.Office.Interop.Word.WdAutoFitBehavior.wdAutoFitWindow);

            string[] headers =
            {
                    "Наименование товара",
                    "Количество",
                    "Цена, руб.",
                    "Единица измерения",
                    "Количество мест",
                    "Масса, т",
                    "ИШК",
                    "Сумма, руб.",
                    "Сумма с НДС, руб.",
                    "Документ (№ от)"
                };

            for (int i = 0; i < headers.Length; i++)
            {
                table.Cell(1, i + 1).Range.Text = headers[i];

                table.Cell(1, i + 1).Range.Font.Bold = 1;

                table.Cell(1, i + 1).Shading.BackgroundPatternColor =
                    Microsoft.Office.Interop.Word.WdColor.wdColorGray15;
            }

            for (int i = 1; i <= 11; i++)
            {
                table.Cell(2, i).Range.Text = i.ToString();
            }

            decimal total = 0;
            int rowIndex = 3;

            foreach (ReturnProductRow product in products)
            {
                decimal sum = product.Price * product.Quantity;

                total += sum;

                table.Cell(rowIndex, 1).Range.Text = product.Name ?? "";

                table.Cell(rowIndex, 2).Range.Text = product.Quantity.ToString();

                table.Cell(rowIndex, 3).Range.Text = product.Price.ToString("N2");

                table.Cell(rowIndex, 4).Range.Text = "";

                table.Cell(rowIndex, 5).Range.Text = "";

                table.Cell(rowIndex, 6).Range.Text = "";

                table.Cell(rowIndex, 7).Range.Text = "";

                table.Cell(rowIndex, 8).Range.Text = sum.ToString("N2");

                table.Cell(rowIndex, 10).Range.Text = "";

                table.Cell(rowIndex, 11).Range.Text = "Заказ №" + orderId;

                rowIndex++;
            }

            Microsoft.Office.Interop.Word.Range endRange = document.Content;

            endRange.Collapse(Microsoft.Office.Interop.Word.WdCollapseDirection.wdCollapseEnd);

            string resultText =
                "\r\nВсего наименований: " + products.Count +
                "\r\nВсего отпущено на сумму: " + total.ToString("N2") + " руб." +
                "\r\nОснование возврата: " + ReasonCB.Text +
                "\r\n\r\n" +
                "Ответственный сотрудник: __________________________" +
                "\r\nПодпись: __________________" +
                "\r\n\r\n" +
                "Груз к перевозке принял: __________________________";

            endRange.InsertAfter(resultText);

            endRange = document.Content;
            endRange.Font.Name = "Times New Roman";
            endRange.Font.Size = 10;

            document.SaveAs2(dialog.FileName);

            application.Visible = true;

            application.Visible = true;

            MessageBox.Show("Накладная успешно сформирована и открыта в Word!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

        }


        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        public class ReturnProductRow
        {
            public string Name { get; set; }

            public string Series { get; set; }

            public decimal Price { get; set; }

            public DateTime ExpirationDate { get; set; }

            public int Quantity { get; set; }
        }
    }
}
