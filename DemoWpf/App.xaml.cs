using DemoWpf.Db;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DemoWpf
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ObservableCollection<Service> services { get; set; }
        public static ObservableCollection<ServicePhoto> servicesPhoto { get; set; }
        public static ObservableCollection<ClientService> clientsService { get; set; }
        public static bool ActivatesStatus = false;
        public static Service editAppServices;

    }
}
