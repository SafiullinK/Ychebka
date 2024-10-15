using DemoWpf.Db;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DemoWpf.Pages
{
    /// <summary>
    /// Логика взаимодействия для LookRegServicesPage.xaml
    /// </summary>
    public partial class LookRegServicesPage : Page
    {
        private Timer updateTimer;
        public LookRegServicesPage(Service serviceId)
        {


            InitializeComponent();
            int today = DateTime.Today.Day;
            // Фильтруем услуги по сегодняшней и завтрашней дате
            var filteredServices = DbConnections.demoEntities.ClientService
                .Where(service => service.StartTime.Day == today && service.ServiceID == serviceId.ID).OrderBy(service => service.StartTime)
                .ToList();

            // Создаем ObservableCollection из отфильтрованных услуг
            App.clientsService = new ObservableCollection<ClientService>(filteredServices);
            ClientsServiceLv.ItemsSource = App.clientsService; 


            updateTimer = new Timer(30000); // 30000 миллисекунд = 30 секунд
            updateTimer.Elapsed += UpdateServicesList; // Подписываемся на событие
            updateTimer.AutoReset = true; // Настраиваем автообновление
            updateTimer.Start(); // Запускаем таймер

            // Первоначальная загрузка услуг
            LoadServices();

        }


        private void UpdateServicesList(object sender, ElapsedEventArgs e)
        {
            LoadServices(); // Перезагрузка услуг
        }

        // Метод для загрузки и фильтрации услуг
        private void LoadServices()
        {
            int today = DateTime.Today.Day;

            // Обновляем список услуг
            var filteredServices = DbConnections.demoEntities.ClientService
                .Where(service => service.StartTime.Day == today)
                .OrderBy(service => service.StartTime)
                .ToList();

            // Обновляем ObservableCollection
            Application.Current.Dispatcher.Invoke(() =>
            {
                App.clientsService.Clear(); // Очищаем старый список
                foreach (var service in filteredServices)
                {
                    App.clientsService.Add(service); // Добавляем новые услуги
                }
            });


        }

        // Не забудьте остановить таймер при закрытии окна
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            updateTimer.Stop(); // Останавливаем таймер
            updateTimer.Dispose(); // Освобождаем ресурсы
        }





    }
}
