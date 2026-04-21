using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CarDelershipWPF.AppData;

namespace CarDelershipWPF.Pages
{
    public partial class PageLogin : Page
    {
        public PageLogin()
        {
            InitializeComponent();
            txtLogin.Focus();

            txtPassword.KeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                    BtnLogin_Click(null, null);
            };
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = txtLogin.Text.Trim();
                string password = txtPassword.Password;

                if (string.IsNullOrWhiteSpace(login))
                {
                    MessageBox.Show("Введите логин", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtLogin.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Введите пароль", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPassword.Focus();
                    return;
                }

                var user = AppConnect.model01.Users
                    .FirstOrDefault(u => u.Login == login && u.Password == password);

                if (user == null)
                {
                    MessageBox.Show("Неверный логин или пароль!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    txtPassword.Clear();
                    txtLogin.Focus();
                    return;
                }

                if (!user.IsActive)
                {
                    MessageBox.Show("Ваш аккаунт заблокирован.", "Доступ запрещен",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var role = AppConnect.model01.Roles
                    .FirstOrDefault(r => r.Role_Id == user.Role_Id);

                string roleName = role?.Name ?? "Клиент";

                if (roleName != "Администратор" && roleName != "Менеджер")
                {
                    MessageBox.Show("Доступ запрещен! Только для администраторов и менеджеров.",
                        "Ошибка доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                    txtPassword.Clear();
                    txtLogin.Focus();
                    return;
                }

                AppFrame.CurrentUser = user;
                AppFrame.CurrentUserID = user.User_Id;
                AppFrame.CurrentUserName = user.FullName;
                AppFrame.CurrentUserRole = roleName;

                AppFrame.FrameMain.Navigate(new DashboardPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при входе: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}