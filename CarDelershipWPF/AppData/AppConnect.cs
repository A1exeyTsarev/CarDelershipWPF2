using System;
using System.Windows;

namespace CarDelershipWPF.AppData
{
    internal class AppConnect
    {
        public static CarDealershipDBEntities1 model01;

        static AppConnect()
        {
            try
            {
                model01 = new CarDealershipDBEntities1();

                // Отключаем создание прокси-объектов
                model01.Configuration.ProxyCreationEnabled = false;

                // Отключаем ленивую загрузку
                model01.Configuration.LazyLoadingEnabled = false;

                if (model01.Database.Exists())
                {
                    System.Diagnostics.Debug.WriteLine("Подключение к БД успешно!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к БД: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}