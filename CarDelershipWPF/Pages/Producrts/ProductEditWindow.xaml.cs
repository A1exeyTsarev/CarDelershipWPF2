using CarDelershipWPF.AppData;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace CarDelershipWPF.Pages
{
    public partial class ProductEditWindow : Window
    {
        private Cars _editingCar;
        private string _selectedImagePath = null;

        // Путь к папке Images в корне проекта
        private string _imagesFolder = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Images"));

        public ProductEditWindow()
        {
            InitializeComponent();
            LoadComboBoxes();
            LoadStatuses();

            // Создаем папку если её нет
            if (!Directory.Exists(_imagesFolder))
                Directory.CreateDirectory(_imagesFolder);
        }

        public ProductEditWindow(Cars car)
        {
            InitializeComponent();
            _editingCar = car;
            LoadComboBoxes();
            LoadStatuses();
            LoadCarData();

            if (!Directory.Exists(_imagesFolder))
                Directory.CreateDirectory(_imagesFolder);
        }

        private void LoadComboBoxes()
        {
            cmbModel.ItemsSource = AppConnect.model01.Models.ToList();
            cmbModel.DisplayMemberPath = "Name";
            cmbModel.SelectedValuePath = "Model_Id";

            cmbColor.ItemsSource = AppConnect.model01.Colors.ToList();
            cmbColor.DisplayMemberPath = "Name";
            cmbColor.SelectedValuePath = "Color_Id";
        }

        private void LoadStatuses()
        {
            cmbStatus.ItemsSource = new[] { "В наличии", "Под заказ", "Нет в наличии", "Скоро в продаже" };
        }

        private void LoadCarData()
        {
            txtName.Text = _editingCar.Name;
            txtPrice.Text = _editingCar.Price.ToString();
            txtDiscountPrice.Text = _editingCar.DiscountPrice.ToString();
            txtQuantity.Text = _editingCar.Quantity.ToString();
            txtYear.Text = _editingCar.Year.ToString();

            cmbModel.SelectedValue = _editingCar.model_Id;
            cmbColor.SelectedValue = _editingCar.Color_Id;
            cmbStatus.SelectedItem = _editingCar.AvailabilityStatus;

            // Загружаем фото
            var image = AppConnect.model01.CarImages.FirstOrDefault(i => i.Car_Id == _editingCar.Car_Id);
            if (image != null && !string.IsNullOrEmpty(image.ImageName))
            {
                string fullPath = Path.Combine(_imagesFolder, image.ImageName);
                if (File.Exists(fullPath))
                    imgPreview.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(fullPath));
            }
        }

        private string SelectImageFile()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp";
            dialog.Title = "Выберите фото для товара";

            if (dialog.ShowDialog() == true)
                return dialog.FileName;

            return null;
        }

        private string CopyImageToProject(string sourcePath, int carId)
        {
            string extension = Path.GetExtension(sourcePath);
            string newFileName = $"{carId}_{DateTime.Now.Ticks}{extension}";
            string destPath = Path.Combine(_imagesFolder, newFileName);

            File.Copy(sourcePath, destPath, true);

            return destPath;
        }

        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            string file = SelectImageFile();
            if (file != null)
            {
                _selectedImagePath = file;
                imgPreview.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(file));
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Проверки
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название товара", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("Введите цену", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity))
            {
                MessageBox.Show("Введите количество", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbModel.SelectedValue == null)
            {
                MessageBox.Show("Выберите модель", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (cmbColor.SelectedValue == null)
            {
                MessageBox.Show("Выберите цвет", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (_editingCar == null)
                {
                    // ===== ДОБАВЛЕНИЕ =====
                    Cars newCar = new Cars
                    {
                        Name = txtName.Text,
                        Price = price,
                        DiscountPrice = decimal.TryParse(txtDiscountPrice.Text, out decimal d) ? d : 0,
                        Quantity = quantity,
                        Year = int.TryParse(txtYear.Text, out int y) ? y : DateTime.Now.Year,
                        model_Id = (int)cmbModel.SelectedValue,
                        Color_Id = (int)cmbColor.SelectedValue,
                        AvailabilityStatus = cmbStatus.SelectedItem?.ToString(),
                        CreatedAt = DateTime.Now,
                        Mileage = 0
                    };
                    AppConnect.model01.Cars.Add(newCar);
                    AppConnect.model01.SaveChanges();

                    // Сохраняем фото
                    if (_selectedImagePath != null)
                    {
                        string destPath = CopyImageToProject(_selectedImagePath, newCar.Car_Id);
                        CarImages carImage = new CarImages
                        {
                            Car_Id = newCar.Car_Id,
                            ImageName = Path.GetFileName(destPath)
                        };
                        AppConnect.model01.CarImages.Add(carImage);
                        AppConnect.model01.SaveChanges();
                    }
                }
                else
                {
                    // ===== РЕДАКТИРОВАНИЕ =====
                    _editingCar.Name = txtName.Text;
                    _editingCar.Price = price;
                    _editingCar.DiscountPrice = decimal.TryParse(txtDiscountPrice.Text, out decimal d) ? d : 0;
                    _editingCar.Quantity = quantity;
                    _editingCar.Year = int.TryParse(txtYear.Text, out int y) ? y : DateTime.Now.Year;
                    _editingCar.model_Id = (int)cmbModel.SelectedValue;
                    _editingCar.Color_Id = (int)cmbColor.SelectedValue;
                    _editingCar.AvailabilityStatus = cmbStatus.SelectedItem?.ToString();

                    // Обновляем фото
                    if (_selectedImagePath != null)
                    {
                        // Удаляем старое фото
                        var oldImage = AppConnect.model01.CarImages.FirstOrDefault(i => i.Car_Id == _editingCar.Car_Id);
                        if (oldImage != null)
                            AppConnect.model01.CarImages.Remove(oldImage);

                        // Сохраняем новое
                        string destPath = CopyImageToProject(_selectedImagePath, _editingCar.Car_Id);
                        CarImages newImage = new CarImages
                        {
                            Car_Id = _editingCar.Car_Id,
                            ImageName = Path.GetFileName(destPath)
                        };
                        AppConnect.model01.CarImages.Add(newImage);
                    }
                }

                AppConnect.model01.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}