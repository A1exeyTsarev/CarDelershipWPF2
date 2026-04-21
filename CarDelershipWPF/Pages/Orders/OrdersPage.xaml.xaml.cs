using CarDelershipWPF.AppData;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CarDelershipWPF.Pages
{
    public partial class OrdersPage : Page
    {
        // Класс для отображения заказа
        public class OrderInfo
        {
            public int Order_Id { get; set; }
            public string OrderNumber { get; set; }
            public string CreatedDate { get; set; }
            public decimal TotalAmount { get; set; }
            public int OrderStatus_Id { get; set; }
            public string StatusName { get; set; }
            public string UserName { get; set; }
        }

        public OrdersPage()
        {
            InitializeComponent();
            LoadStatusFilter();
            LoadOrders();
        }

        // Загрузка статусов в фильтр
        private void LoadStatusFilter()
        {
            var statuses = AppConnect.model01.OrderStatuses.ToList();
            statuses.Insert(0, new OrderStatuses { OrderStatus_Id = 0, Name = "Все статусы" });
            cmbStatus.ItemsSource = statuses;
            cmbStatus.DisplayMemberPath = "Name";
            cmbStatus.SelectedValuePath = "OrderStatus_Id";
            cmbStatus.SelectedIndex = 0;

            // Варианты сортировки
            cmbSort.Items.Add(new { Id = 0, Name = "Без сортировки" });
            cmbSort.Items.Add(new { Id = 1, Name = "По дате (новые)" });
            cmbSort.Items.Add(new { Id = 2, Name = "По дате (старые)" });
            cmbSort.Items.Add(new { Id = 3, Name = "По сумме (возрастание)" });
            cmbSort.Items.Add(new { Id = 4, Name = "По сумме (убывание)" });
            cmbSort.DisplayMemberPath = "Name";
            cmbSort.SelectedValuePath = "Id";
            cmbSort.SelectedIndex = 0;
        }

        // Загрузка заказов
        private void LoadOrders()
        {
            try
            {
                var orders = AppConnect.model01.Orders.ToList();
                var users = AppConnect.model01.Users.ToDictionary(u => u.User_Id, u => u.FullName);
                var statuses = AppConnect.model01.OrderStatuses.ToDictionary(s => s.OrderStatus_Id, s => s.Name);

                var list = orders.Select(o => new OrderInfo
                {
                    Order_Id = o.Order_Id,
                    OrderNumber = o.OrderNumber,
                    CreatedDate = o.CreatedDate.ToString("dd.MM.yyyy HH:mm"),
                    TotalAmount = o.TotalAmount,
                    OrderStatus_Id = o.OrderStatus_Id,
                    StatusName = statuses.ContainsKey(o.OrderStatus_Id) ? statuses[o.OrderStatus_Id] : "Неизвестно",
                    UserName = users.ContainsKey(o.User_Id) ? users[o.User_Id] : "Неизвестно"
                }).ToList();

                // Поиск
                string search = txtSearch.Text?.Trim();
                if (!string.IsNullOrEmpty(search))
                {
                    list = list.Where(o => o.OrderNumber.Contains(search)).ToList();
                }

                // Фильтр по статусу
                if (cmbStatus.SelectedValue != null && (int)cmbStatus.SelectedValue > 0)
                {
                    int statusId = (int)cmbStatus.SelectedValue;
                    list = list.Where(o => o.OrderStatus_Id == statusId).ToList();
                }

                // Сортировка
                if (cmbSort.SelectedValue != null)
                {
                    int sortId = (int)cmbSort.SelectedValue;
                    switch (sortId)
                    {
                        case 1: list = list.OrderByDescending(o => o.CreatedDate).ToList(); break;
                        case 2: list = list.OrderBy(o => o.CreatedDate).ToList(); break;
                        case 3: list = list.OrderBy(o => o.TotalAmount).ToList(); break;
                        case 4: list = list.OrderByDescending(o => o.TotalAmount).ToList(); break;
                    }
                }

                dgOrders.ItemsSource = list;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        // Поиск
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) => LoadOrders();

        // Фильтр по статусу
        private void cmbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadOrders();

        // Сортировка
        private void cmbSort_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadOrders();

        // Сброс фильтров
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            cmbStatus.SelectedIndex = 0;
            cmbSort.SelectedIndex = 0;
            LoadOrders();
        }

        // Создать заказ
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddOrderWindow();
            dialog.Owner = Window.GetWindow(this);
            if (dialog.ShowDialog() == true) LoadOrders();
        }

        // Изменить статус
        private void btnEditStatus_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button)?.Tag as OrderInfo;
            if (item == null) return;

            var order = AppConnect.model01.Orders.FirstOrDefault(o => o.Order_Id == item.Order_Id);
            if (order != null)
            {
                var statuses = AppConnect.model01.OrderStatuses.ToList();
                var dialog = new StatusDialog(order, statuses);
                dialog.Owner = Window.GetWindow(this);
                if (dialog.ShowDialog() == true) LoadOrders();
            }
        }
    }
}