using System;
using System.Collections.Generic;
using System.Data.Common;
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
using DemoWpf.Db;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;
using System.Globalization;

namespace DemoWpf.Pages
{
    /// <summary>
    /// Логика взаимодействия для RegistrationForServicesPage.xaml
    /// </summary>
    public partial class RegistrationForServicesPage : Page
    {
        public static ObservableCollection<ClientService> clientServices { get; set; }
        public static ObservableCollection<Client> clients { get; set; }
        private int uniqueServiceId;
        private string timeMyServ;
        public RegistrationForServicesPage(Service regService)
        {
            InitializeComponent();
            uniqueServiceId = regService.ID;
            timeMyServ = regService.Description.ToString();
            NameServiceTb.Text = regService.Title;
            DurationServiceTb.Text = regService.Description.ToString() + " " + "минут";
            ServiceImg.Source = null;
            clientServices = new ObservableCollection<ClientService>(DbConnections.demoEntities.ClientService.ToList());
            clients = new ObservableCollection<Client>(DbConnections.demoEntities.Client.ToList());
            // Заполняем комбобокс ФИО
            foreach (var client in clients)
            {
                // Предполагаем, что у вас есть свойства FirstName, LastName и MiddleName
                string fullName = $"{client.LastName} {client.FirstName} {client.Patronymic}";
                FIOTb.Items.Add(new ComboBoxItem { Content = fullName, Tag = client });
            }
            DateServiceDt.DisplayDateStart = DateTime.Now;

        }

        private void BackBtn_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new ClientPage());
        }

        private void RedServClientBtn_Click(object sender, RoutedEventArgs e)
        {
            if (FIOTb.SelectedItem != null && DateServiceDt.SelectedDate != null && !string.IsNullOrEmpty(TimeTb.Text))
            {
                var selectedItem = FIOTb.SelectedItem as ComboBoxItem;
                if (selectedItem == null)
                {
                    MessageBox.Show("Пожалуйста, выберите клиента.");
                    return;
                }

                // Извлекаем объект клиента из тега
                var selectedClient = selectedItem.Tag as Client;

                if (DateTime.TryParseExact(TimeTb.Text, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime time))
                {
                    DateTime combinedDateTime = DateServiceDt.SelectedDate.Value.Date + time.TimeOfDay;
                    var newClientService = new ClientService
                    {
                        ClientID = selectedClient.ID, // Используем ID клиента
                        ServiceID = uniqueServiceId,
                        StartTime = combinedDateTime,
                    };

                    // Добавляем новый объект в коллекцию
                    clientServices.Add(newClientService);

                    // Сохраняем изменения в базе данных
                    DbConnections.demoEntities.ClientService.Add(newClientService);
                    DbConnections.demoEntities.SaveChanges();
                    int timeInMinutes;

                    // Пробуем распарсить строку в int
                    if (int.TryParse(timeMyServ, out timeInMinutes))
                    {
                        // Создаем TimeSpan из минут
                        TimeSpan timeSpan = TimeSpan.FromMinutes(timeInMinutes);

                        // Объединяем дату из combinedDateTime с временем из timeSpan
                        DateTime result = combinedDateTime.Date + timeSpan;

                        // Устанавливаем результат в текстовое поле
                        EndServicesTb.Text = result.ToString("G"); // форматируем результат в нужный формат
                    }
                    else
                    {
                        EndServicesTb.Text = "Некорректное время"; // Обработка ошибки
                    }


                    MessageBox.Show("Услуга успешно добавлена!");

                    }
                    else
                    {
                        MessageBox.Show("Заполните все поля!");
                    }
                }
                else
                {
                    // Не удалось преобразовать строку
                    MessageBox.Show("Некорректный формат времени.");
                }
                // Объединяем выбранную дату и время


                // Создаем новый объект ClientService
                
        }

        private void TimeTb_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!IsTextAllowed(e.Text))
            {
                e.Handled = true; // Блокируем ввод
            }
        }

        private bool IsTextAllowed(string text)
        {
            Regex regex = new Regex(@"^[0-9:]*$");
            return regex.IsMatch(text);
        }

        private bool IsValidTime(string time)
        {
            if (string.IsNullOrWhiteSpace(time)) return false;
            Regex regex = new Regex(@"^([01]?[0-9]|2[0-3]):([0-5][0-9])$");
            return regex.IsMatch(time);
        }
        private void TimeTb_LostFocus(object sender, RoutedEventArgs e)
        {
            // Проверяем на корректный формат времени
            if (!IsValidTime(TimeTb.Text))
            {
                ErrorTextBlock.Text = "Неверный формат времени. Используйте чч:мм.";
            }
            else
            {
                ErrorTextBlock.Text = string.Empty; // Убираем сообщение об ошибке
            }
        }
    }
}
