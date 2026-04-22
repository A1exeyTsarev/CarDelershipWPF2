using CarDelershipWPF.AppData;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace CarDelershipWPF.Pages
{
    public partial class ProductEditWindow : Window
    {
        private Cars _editingCar;
        private string _selectedImagePath = null;
        private string _imagesFolder = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Images"));

        public ProductEditWindow()
        {
            InitializeComponent();
            LoadComboBoxes();
            LoadStatuses();

            if (!Directory.Exists(_imagesFolder))
                Directory.CreateDirectory(_imagesFolder);

            // В режиме добавления скрываем кнопку удаления
            btnDelete.Visibility = Visibility.Collapsed;
        }

        public ProductEditWindow(Cars car) : this()
        {
            _editingCar = car;
            LoadCarData();
            btnDelete.Visibility = Visibility.Visible; // Показываем кнопку удаления
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
            cmbStatus.SelectedIndex = 0;
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
                    imgPreview.Source = new BitmapImage(new Uri(fullPath));
            }
        }

        // ==================== БЛОКИРОВКА ВВОДА ====================

        // Только цифры и точка для цен
        private void NumberOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, @"^[0-9.,]+$"))
                e.Handled = true;
        }

        // Только цифры для года и количества
        private void IntegerOnly_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Regex.IsMatch(e.Text, @"^[0-9]+$"))
                e.Handled = true;
        }

        // Разрешаем Backspace, Delete и навигацию
        private void Number_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
                e.Handled = true;
        }

        // ==================== ВЫБОР ИЗОБРАЖЕНИЯ ====================

        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp";
            dialog.Title = "Выберите фото для товара";

            if (dialog.ShowDialog() == true)
            {
                _selectedImagePath = dialog.FileName;
                imgPreview.Source = new BitmapImage(new Uri(dialog.FileName));
            }
        }

        // ==================== СОХРАНЕНИЕ ====================

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Простая проверка
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название товара", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                MessageBox.Show("Введите цену", "Ошибka", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrice.Focus();
                return;
            }

            string priceText = txtPrice.Text.Replace(',', '.');
            if (!decimal.TryParse(priceText, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrice.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtQuantity.Text))
            {
                MessageBox.Show("Введите количество", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtQuantity.Focus();
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Введите корректное количество", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtQuantity.Focus();
                return;
            }

            if (cmbModel.SelectedValue == null)
            {
                MessageBox.Show("Выберите модель", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                cmbModel.Focus();
                return;
            }

            try
            {
                if (_editingCar == null)
                {
                    // ДОБАВЛЕНИЕ НОВОГО ТОВАРА
                    Cars newCar = new Cars
                    {
                        Name = txtName.Text.Trim(),
                        Price = price,
                        DiscountPrice = decimal.TryParse(txtDiscountPrice.Text.Replace(',', '.'), out decimal dp) ? dp : 0,
                        Quantity = quantity,
                        Year = int.TryParse(txtYear.Text, out int year) ? year : DateTime.Now.Year,
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
                        string extension = Path.GetExtension(_selectedImagePath);
                        string newFileName = $"{newCar.Car_Id}_{DateTime.Now.Ticks}{extension}";
                        string destPath = Path.Combine(_imagesFolder, newFileName);
                        File.Copy(_selectedImagePath, destPath, true);

                        CarImages carImage = new CarImages
                        {
                            Car_Id = newCar.Car_Id,
                            ImageName = newFileName
                        };
                        AppConnect.model01.CarImages.Add(carImage);
                        AppConnect.model01.SaveChanges();
                    }

                    MessageBox.Show("Товар успешно добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // РЕДАКТИРОВАНИЕ ТОВАРА
                    _editingCar.Name = txtName.Text.Trim();
                    _editingCar.Price = price;
                    _editingCar.DiscountPrice = decimal.TryParse(txtDiscountPrice.Text.Replace(',', '.'), out decimal dp) ? dp : 0;
                    _editingCar.Quantity = quantity;
                    _editingCar.Year = int.TryParse(txtYear.Text, out int year) ? year : DateTime.Now.Year;
                    _editingCar.model_Id = (int)cmbModel.SelectedValue;
                    _editingCar.Color_Id = (int)cmbColor.SelectedValue;
                    _editingCar.AvailabilityStatus = cmbStatus.SelectedItem?.ToString();

                    // Обновляем фото
                    if (_selectedImagePath != null)
                    {
                        // Удаляем старое фото из БД
                        var oldImage = AppConnect.model01.CarImages.FirstOrDefault(i => i.Car_Id == _editingCar.Car_Id);
                        if (oldImage != null)
                            AppConnect.model01.CarImages.Remove(oldImage);

                        // Сохраняем новое
                        string extension = Path.GetExtension(_selectedImagePath);
                        string newFileName = $"{_editingCar.Car_Id}_{DateTime.Now.Ticks}{extension}";
                        string destPath = Path.Combine(_imagesFolder, newFileName);
                        File.Copy(_selectedImagePath, destPath, true);

                        CarImages newImage = new CarImages
                        {
                            Car_Id = _editingCar.Car_Id,
                            ImageName = newFileName
                        };
                        AppConnect.model01.CarImages.Add(newImage);
                    }

                    AppConnect.model01.SaveChanges();
                    MessageBox.Show("Товар успешно сохранен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==================== УДАЛЕНИЕ ТОВАРА ====================

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_editingCar == null)
            {
                MessageBox.Show("Нет товара для удаления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Вы уверены, что хотите удалить товар \"{_editingCar.Name}\"?\n\nЭто действие невозможно отменить!",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Очищаем предпросмотр фото
                    imgPreview.Source = null;

                    // Принудительная сборка мусора
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    // Удаляем связанные записи из CarImages (файлы оставляем)
                    var images = AppConnect.model01.CarImages.Where(i => i.Car_Id == _editingCar.Car_Id).ToList();
                    foreach (var image in images)
                    {
                        AppConnect.model01.CarImages.Remove(image);
                    }

                    // Удаляем товар
                    AppConnect.model01.Cars.Remove(_editingCar);
                    AppConnect.model01.SaveChanges();

                    MessageBox.Show("Товар успешно удален!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}