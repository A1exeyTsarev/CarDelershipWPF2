using CarDelershipWPF.AppData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CarDelershipWPF.Pages
{
    public partial class ProductsPage : Page
    {
        public ProductsPage()
        {
            InitializeComponent();
            LoadProducts();
        }

        // Метод для получения пути к папке с изображениями
        private string GetImageFolderPath()
        {
            try
            {
                // Получаем путь к папке с программой (bin/Debug)
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                // Поднимаемся из bin/Debug в корень проекта
                string projectPath = Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\.."));

                // Путь к папке Images
                string imageFolder = Path.Combine(projectPath, "Images");

                // Если папка не найдена, пробуем альтернативный путь
                if (!Directory.Exists(imageFolder))
                {
                    imageFolder = Path.Combine(baseDirectory, "Images");
                }

                return imageFolder;
            }
            catch
            {
                return string.Empty;
            }
        }

        // Метод для получения пути к изображению
        private string GetImagePath(Cars car, string imageFolder)
        {
            try
            {
                // Если в базе есть имя файла
                var dbImage = AppConnect.model01.CarImages.FirstOrDefault(i => i.Car_Id == car.Car_Id);

                if (dbImage != null && !string.IsNullOrEmpty(dbImage.ImageName))
                {
                    string fullPath = Path.Combine(imageFolder, dbImage.ImageName);
                    if (File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }

                // Пробуем найти файл по ID товара
                string[] extensions = { ".jpg", ".jpeg", ".png", ".bmp" };
                foreach (string ext in extensions)
                {
                    string pathById = Path.Combine(imageFolder, $"{car.Car_Id}{ext}");
                    if (File.Exists(pathById))
                    {
                        return pathById;
                    }
                }

                // Заглушка - no_image.jpg
                string defaultPath = Path.Combine(imageFolder, "no_image.jpg");
                if (File.Exists(defaultPath))
                {
                    return defaultPath;
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        // КЛАСС ДЛЯ ОТОБРАЖЕНИЯ ТОВАРОВ С КАРТИНКАМИ
        public class ProductDisplay
        {
            public Cars Car { get; set; }
            public string ImagePath { get; set; }

            // Свойства для привязки в XAML
            public int Car_Id => Car?.Car_Id ?? 0;
            public string Name => Car?.Name ?? "";
            public decimal Price => Car?.Price ?? 0;
            public int Quantity => Car?.Quantity ?? 0;
            public string AvailabilityStatus => Car?.AvailabilityStatus ?? "";
            public string CurrentPhoto => Car?.CarImages?.FirstOrDefault()?.ImageName ?? "/Images/no_image.jpg";
        }

        private void LoadProducts()
        {
            try
            {
                var products = AppConnect.model01.Cars.ToList();

                // Получаем путь к папке с изображениями
                string imageFolder = GetImageFolderPath();

                // Создаем список для отображения с картинками
                List<ProductDisplay> displayList = new List<ProductDisplay>();

                foreach (var product in products)
                {
                    string imagePath = GetImagePath(product, imageFolder);

                    displayList.Add(new ProductDisplay
                    {
                        Car = product,
                        ImagePath = imagePath
                    });
                }

                dgProducts.ItemsSource = displayList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }
        private void DgProducts_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgProducts.SelectedItem is ProductDisplay selectedDisplay)
            {
                var dialog = new ProductEditWindow(selectedDisplay.Car);
                dialog.Owner = Window.GetWindow(this);
                if (dialog.ShowDialog() == true)
                    LoadProducts();
            }
            else
            {
                MessageBox.Show("Выберите товар для редактирования!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void UpdateProductsList()
        {
            try
            {
                var products = AppConnect.model01.Cars.ToList();

                // Поиск по названию
                if (!string.IsNullOrWhiteSpace(txtSearch?.Text))
                {
                    string searchText = txtSearch.Text.ToLower();
                    products = products.Where(x =>
                        x.Name.ToLower().Contains(searchText)).ToList();
                }

                // Получаем путь к папке с изображениями
                string imageFolder = GetImageFolderPath();

                // Создаем список для отображения с картинками
                List<ProductDisplay> displayList = new List<ProductDisplay>();

                foreach (var product in products)
                {
                    string imagePath = GetImagePath(product, imageFolder);

                    displayList.Add(new ProductDisplay
                    {
                        Car = product,
                        ImagePath = imagePath
                    });
                }

                dgProducts.ItemsSource = displayList;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateProductsList();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ProductEditWindow();
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true)
                LoadProducts();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (dgProducts.SelectedItem is ProductDisplay selectedDisplay)
            {
                var dialog = new ProductEditWindow(selectedDisplay.Car);
                dialog.Owner = Window.GetWindow(this);
                if (dialog.ShowDialog() == true)
                    LoadProducts();
            }
            else
            {
                MessageBox.Show("Выберите товар для редактирования!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnPriceHistory_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.FrameMain.Navigate(new ProductPriceHistoryPage());
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