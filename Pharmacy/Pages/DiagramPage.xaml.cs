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
using Pharmacy.Model;
using System.Windows.Forms.DataVisualization.Charting;


namespace Pharmacy.Pages
{
    /// <summary>
    /// Логика взаимодействия для DiagramPage.xaml
    /// </summary>
    public partial class DiagramPage : Page
    {
        private Chart ChartPayments;

        public DiagramPage()
        {
            InitializeComponent();

            ChartPayments = new Chart();

            ChartArea area = new ChartArea("Main");
            area.AxisX.Interval = 1;
            area.AxisX.LabelStyle.Angle = -45;
            area.AxisY.Title = "Количество товара";
            area.AxisY.Minimum = 0;

            ChartPayments.ChartAreas.Add(area);
            ChartPayments.Legends.Add(new Legend());

            ChartHost.Child = ChartPayments;

            int userId = Core.CurrentUser.ID;
            var pharmacies = Core.Context.UserPharmacy.Where(up => up.UserID == userId).Select(up => up.Pharmacies).ToList();
            PharmacyCB.ItemsSource = pharmacies;

            DiagramCB.ItemsSource = new SeriesChartType[]
            {
                SeriesChartType.Column,
                SeriesChartType.Bar,
                SeriesChartType.Pie,
                SeriesChartType.Line
            };

            DiagramCB.SelectedItem = SeriesChartType.Column;

            if (PharmacyCB.Items.Count > 0)
                PharmacyCB.SelectedIndex = 0;
        }

        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            if (PharmacyCB.SelectedItem == null || DiagramCB.SelectedItem == null)
                return;

            var pharmacy = PharmacyCB.SelectedItem as Pharmacies;

            if (pharmacy == null)
                return;

            SeriesChartType chartType = (SeriesChartType)DiagramCB.SelectedItem;

            ChartPayments.Series.Clear();

            Series series = new Series("Количество товаров");
            series.ChartType = chartType;
            series.IsValueShownAsLabel = true;

            ChartPayments.Series.Add(series);

            var pharmacyProducts = Core.Context.ProductPharmacies.ToList().Where(p => p.PharmacyID == pharmacy.ID).ToList();

            foreach (var item in pharmacyProducts)
            {
                var product = Core.Context.Products.FirstOrDefault(p => p.ID == item.ProductID);

                if (product != null)
                {
                    series.Points.AddXY(product.Name, item.Count);
                }
            }

            ChartPayments.ChartAreas[0].RecalculateAxesScale();
        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}

