using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace TelegramBOT.Reports
{
    public partial class AnalyticsForm : Form
    {
        private readonly Database _database;
        private TabControl tabControl;
        private Chart statusChart; // График статусов (вкладка 1)
        private Chart cabinetChart; // График кабинетов (вкладка 2)
        private ComboBox timeFilterComboBox;
        private string currentTimeFilter = "month";

        public AnalyticsForm(Database database)
        {
            _database = database;
            InitializeComponent();

            tabControl = new TabControl { Dock = DockStyle.Fill };

            // Создаем графики и сохраняем ссылки
            statusChart = CreateStatusChart();
            cabinetChart = CreateCabinetChart();

            // Вкладка "Статусы задач"
            var statusTab = new TabPage("Статусы задач");
            statusTab.Controls.Add(statusChart); // Используем сохраненный график
            tabControl.TabPages.Add(statusTab);

            // Вкладка "Распределение по кабинетам"
            var cabinetTab = new TabPage("Кабинеты");
            var cabinetPanel = new Panel { Dock = DockStyle.Fill };
            cabinetPanel.Controls.Add(cabinetChart);
            timeFilterComboBox = CreateTimeFilterComboBox();
            cabinetPanel.Controls.Add(timeFilterComboBox);
            cabinetTab.Controls.Add(cabinetPanel);
            tabControl.TabPages.Add(cabinetTab);

            Controls.Add(tabControl);
            Controls.Add(CreateFilterComboBox());
        }

        private ComboBox CreateTimeFilterComboBox()
        {
            var combo = new ComboBox { Dock = DockStyle.Top };
            combo.Items.AddRange(new[] { "За месяц", "За неделю" });
            combo.SelectedIndex = 0;
            combo.SelectedIndexChanged += (s, e) =>
            {
                currentTimeFilter = combo.SelectedIndex == 0 ? "month" : "week";
                UpdateCabinetChart(GetCurrentStatusFilter());
            };
            return combo;
        }

        private void ApplyFilter(string statusFilter)
        {
            UpdateStatusChart(statusFilter);
            UpdateCabinetChart(statusFilter);
        }

        private void UpdateStatusChart(string statusFilter)
        {
            var stats = _database.GetTaskStatusStatistics(statusFilter);
            statusChart.Series["Статусы задач"].Points.Clear();
            foreach (var entry in stats)
            {
                DataPoint point = statusChart.Series["Статусы задач"].Points.Add(entry.Value);
                point.AxisLabel = entry.Key;
                point.LegendText = $"{entry.Key} ({entry.Value})";
                point.Color = GetStatusColor(entry.Key);
            }
        }

        private void UpdateCabinetChart(string statusFilter)
        {
            var cabinetStats = _database.GetTasksPercentagePerCabinet(statusFilter, currentTimeFilter);
            cabinetChart.Series["Задачи"].Points.Clear();

            foreach (var entry in cabinetStats)
            {
                DataPoint point = cabinetChart.Series["Задачи"].Points.Add(entry.Value);
                point.AxisLabel = entry.Key;
                point.Label = $"{entry.Value:0.0}%";
                point.Color = Color.SteelBlue;
            }

            cabinetChart.ChartAreas[0].AxisY.Maximum = 100;
            cabinetChart.Invalidate();
        }

        private string GetCurrentStatusFilter()
        {
            // Найдите ваш основной ComboBox с фильтрами статусов
            foreach (Control control in Controls)
            {
                if (control is ComboBox combo)
                {
                    return combo.SelectedItem?.ToString() ?? "Все";
                }
            }
            return "Все";
        }

        private Chart CreateCabinetChart()
        {
            Chart chart = new Chart();
            chart.Dock = DockStyle.Fill;
            chart.Size = new Size(600, 400);

            ChartArea chartArea = new ChartArea();
            chartArea.AxisX.Title = "Кабинеты";
            chartArea.AxisY.Title = "Доля задач (%)";
            chartArea.AxisY.Maximum = 100;
            chart.ChartAreas.Add(chartArea);

            Series series = new Series("Задачи");
            series.ChartType = SeriesChartType.Column;
            series.IsValueShownAsLabel = true;
            series.LabelFormat = "{0.0}%";
            series.Font = new Font("Arial", 10, FontStyle.Bold);

            // Загрузка начальных данных
            var cabinetStats = _database.GetTasksPercentagePerCabinet(period: "month");
            foreach (var entry in cabinetStats)
            {
                DataPoint point = series.Points.Add(entry.Value);
                point.AxisLabel = entry.Key;
                point.Label = $"{entry.Value:0.0}%";
                point.Color = Color.SteelBlue;
            }

            Legend legend = new Legend();
            legend.Docking = Docking.Bottom;
            chart.Legends.Add(legend);

            chart.Series.Add(series);
            return chart;
        }

        private Chart CreateStatusChart()
        {
            Chart chart = new Chart();
            chart.Dock = DockStyle.Fill;
            chart.Size = new Size(600, 400);

            ChartArea chartArea = new ChartArea();
            chartArea.AxisX.LabelStyle.Enabled = false;
            chartArea.AxisY.LabelStyle.Enabled = false;
            chart.ChartAreas.Add(chartArea);

            Series series = new Series("Статусы задач");
            series.ChartType = SeriesChartType.Pie;
            series.IsValueShownAsLabel = true;
            series.LabelFormat = "{0} ({1})";
            series.LegendText = "#VALX (#PERCENT{P0})";
            series.Font = new Font("Arial", 10);

            // Исправленный блок с тернарными операторами
            var statusStats = _database.GetTaskStatusStatistics()
                .Select(entry => new
                {
                    Key = entry.Key == "Завершено" ? "Завершено" :
                         entry.Key == "Не завершено" ? "Не завершено" :
                         entry.Key,
                    Value = entry.Value
                })
                .ToDictionary(x => x.Key, x => x.Value);

            foreach (var entry in statusStats)
            {
                DataPoint point = series.Points.Add(entry.Value);
                point.AxisLabel = entry.Key;
                point.LegendText = $"{entry.Key} ({entry.Value})";
                point.Color = GetStatusColor(entry.Key);
            }

            Legend legend = new Legend();
            legend.Docking = Docking.Bottom;
            legend.Alignment = StringAlignment.Center;
            chart.Legends.Add(legend);

            // Исправленная проверка с использованием Any()
            if (!statusStats.Any())
            {
                chart.Titles.Add(new Title("Нет данных для отображения",
                    Docking.Top,
                    new Font("Arial", 12),
                    Color.Gray));
            }

            chart.Series.Add(series);
            return chart;
        }
        // Метод для определения цвета по статусу
        private Color GetStatusColor(string status)
        {
            string normalizedStatus = status.Trim().ToLower();

            switch (normalizedStatus)
            {
                case "Завершено":
                case "завершено":
                    return Color.LightGreen;
                case "Не завершено":
                case "не завершено":
                    return Color.Orange;
                default:
                    return Color.Gray;
            }
        }

        private ComboBox CreateFilterComboBox()
        {
            var combo = new ComboBox { Dock = DockStyle.Top };
            combo.Items.AddRange(new[] { "Все", "Не завершено", "Завершено" });
            combo.SelectedIndex = 0; // Выбрать "Все" по умолчанию
            combo.SelectedIndexChanged += (s, e) =>
            {
                if (combo.SelectedItem != null)
                {
                    string filter = combo.SelectedItem.ToString();
                    ApplyFilter(filter);
                }
            };
            return combo;
        }
    }
}