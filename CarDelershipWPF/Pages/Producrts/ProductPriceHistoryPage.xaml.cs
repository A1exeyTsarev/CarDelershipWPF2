using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CarDelershipWPF.AppData;

namespace CarDelershipWPF.Pages
{
    public partial class ProductPriceHistoryPage : Page
    {
        private int? _selectedProductId;

        public ProductPriceHistoryPage()
        {
            InitializeComponent();
            LoadProducts();
        }

        public ProductPriceHistoryPage(int productId)
        {
            InitializeComponent();
            _selectedProductId = productId;
            LoadProducts();

            // Если передан ID товара, выбираем его в ComboBox и загружаем историю
            if (_selectedProductId.HasValue)
            {
                cmbProducts.SelectedValue = _selectedProductId.Value;
                LoadPriceHistory();
            }
        }

        private void LoadProducts()
        {
            try
            {
                // Загружаем список товаров для выбора
                var products = AppConnect.model01.Cars.ToList();

                if (products != null && products.Any())
                {
                    cmbProducts.ItemsSource = products;
                }
                else
                {
                    cmbProducts.ItemsSource = null;
                    MessageBox.Show("Нет доступных товаров", "Информация",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CmbProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (cmbProducts.SelectedItem != null)
                {
                    LoadPriceHistory();
                }
                else
                {
                    dgPriceHistory.Visibility = Visibility.Collapsed;
                    txtNoData.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadPriceHistory()
        {
            try
            {
                if (cmbProducts.SelectedValue == null)
                {
                    dgPriceHistory.Visibility = Visibility.Collapsed;
                    txtNoData.Visibility = Visibility.Visible;
                    return;
                }

                int productId = (int)cmbProducts.SelectedValue;

                // Загружаем историю цен для выбранного товара
                var priceHistory = AppConnect.model01.PriceHistory
                    .Where(ph => ph.Car_Id == productId)
                    .OrderByDescending(ph => ph.ChangeDate)
                    .ToList();

                if (priceHistory != null && priceHistory.Any())
                {
                    dgPriceHistory.ItemsSource = priceHistory;
                    dgPriceHistory.Visibility = Visibility.Visible;
                    txtNoData.Visibility = Visibility.Collapsed;
                }
                else
                {
                    dgPriceHistory.ItemsSource = null;
                    dgPriceHistory.Visibility = Visibility.Collapsed;
                    txtNoData.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки истории цен: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                dgPriceHistory.Visibility = Visibility.Collapsed;
                txtNoData.Visibility = Visibility.Visible;
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (AppFrame.FrameMain.CanGoBack)
                AppFrame.FrameMain.GoBack();
            else
                AppFrame.FrameMain.Navigate(new DashboardPage());
        }
    }
}