using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace DemoWpf.Pages
{
    /// <summary>
    /// Логика взаимодействия для ClientPage.xaml
    /// </summary>
    public partial class ClientPage : Page
    {

        private int itemCount;
        private int immutableCount;
        private int minDiscount = 0;
        private int maxDiscount = 100;
        public ClientPage()
        {
            InitializeComponent();
            LoadServices();
            SetDefaultSorts();
            UpdateListView();
            if (App.ActivatesStatus)
            {
                ActivateAdminMode();
            }
        }

        private void LoadServices()
        {
            var uniqueServices = DbConnections.demoEntities.Service.GroupBy(s => s.Title).Select(g => g.FirstOrDefault()).ToList();
            App.services = new ObservableCollection<Service>(uniqueServices);

            foreach (var serv in App.services)
            {
                serv.CostWithDiscount = 0;
                serv.Cost = (decimal)serv.Cost;
                serv.CostWithDiscount = CalculateCostWithDiscount(serv);
            }
        }

        private decimal CalculateCostWithDiscount(Service serv)
        {
            if (serv.Discount != 0)
            {
                return (decimal)(serv.Cost - (serv.Cost * (serv.Discount / 100)));
            }
            else
                return (decimal)(serv.Cost);

        }

        private void SetDefaultSorts()
        {
            CostSortCb.SelectedIndex = 0;
            SortDiscountCb.SelectedIndex = 0;
        }

        private void UpdateListView()
        {
            ServiсeLv.ItemsSource = App.services;
            immutableCount = ServiсeLv.Items.Count;
            itemCount = immutableCount;
            CountItemsTb.Text = $"{itemCount} из {immutableCount}";
        }

        private void ActivateAdminMode()
        {
            CodeActivateTb.Text = "0000";
            ActivateBtn_Click(this, new RoutedEventArgs());
            Dispatcher.BeginInvoke((Action)(() => SetStackPanelsVisibility(Visibility.Visible)), System.Windows.Threading.DispatcherPriority.Input);
        }

        private void ActivateBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CodeActivateTb.Text == "0000")
            {
                ActivateAdmin();
            }
            else
            {
                ShowActivationError();
            }
        }

        private void ActivateAdmin()
        {
            App.ActivatesStatus = true;
            ActivateSt.Visibility = Visibility.Collapsed;
            NotActivateBtn.Visibility = Visibility.Visible;
            SetStackPanelsVisibility(Visibility.Visible);
        }

        private void ShowActivationError()
        {
            MessageBox.Show("Режим админа не активирован! Причина: Неверный код активации");
        }

        private void NotActivateBtn_Click(object sender, RoutedEventArgs e)
        {
            App.ActivatesStatus = false;
            ToggleAdminVisibility(Visibility.Visible);
        }

        private void ToggleAdminVisibility(Visibility visibility)
        {
            ActivateSt.Visibility = visibility;
            NotActivateBtn.Visibility = visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            SetStackPanelsVisibility(visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible);
        }

        private void SetStackPanelsVisibility(Visibility visibility)
        {
            foreach (var item in ServiсeLv.Items)
            {
                if (ServiсeLv.ItemContainerGenerator.ContainerFromItem(item) is ListViewItem container)
                {
                    var stackPanel = FindVisualChild<StackPanel>(container);
                    if (stackPanel != null)
                    {
                        var activateAdminLvSt = FindChildByName(stackPanel, "ActivateAdminLvSt");
                        if (activateAdminLvSt != null)
                        {
                            activateAdminLvSt.Visibility = visibility;
                        }
                    }
                }
            }

        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T variable)
                {
                    return variable;
                }

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }
            return null;
        }

        private StackPanel FindChildByName(DependencyObject parent, string childName)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is StackPanel panel && panel.Name == childName)
                {
                    return panel;
                }

                var foundChild = FindChildByName(child, childName);
                if (foundChild != null)
                {
                    return foundChild;
                }
            }
            return null;
        }

        private void SortBtn_Click(object sender, RoutedEventArgs e)
        {
            ToggleSortVisibility(Visibility.Collapsed);
        }

        private void NotSortBtn_Click(object sender, RoutedEventArgs e)
        {
            ResetSortMethod();
            ToggleSortVisibility(Visibility.Visible);
            //RoutedEventArgs args = new RoutedEventArgs();
            //ActivateBtn_Click(this, args);
            if (App.ActivatesStatus == true)
            {
                Dispatcher.BeginInvoke((Action)(() => SetStackPanelsVisibility(Visibility.Visible)), System.Windows.Threading.DispatcherPriority.Input);

            }
        }
        private void ToggleSortVisibility(Visibility visibility)
        {
            FullSortSt.Visibility = visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            SortBtn.Visibility = visibility;
            NotSortBtn.Visibility = visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        private void UpdateServiceList()
        {
            var filteredServices = App.services.AsQueryable();

            filteredServices = FilterByTitle(filteredServices);
            filteredServices = FilterByDescription(filteredServices);
            filteredServices = FilterByDiscount(filteredServices);
            filteredServices = SortServices(filteredServices);

            var serviceList = filteredServices.ToList();
            ServiсeLv.ItemsSource = serviceList;
            itemCount = serviceList.Count;
            CountItemsTb.Text = $"{itemCount} из {immutableCount}";

            if (App.ActivatesStatus)
            {
                Dispatcher.BeginInvoke((Action)(() => SetStackPanelsVisibility(Visibility.Visible)), System.Windows.Threading.DispatcherPriority.Input);
            }
        }

        private IQueryable<Service> FilterByTitle(IQueryable<Service> services)
        {
            string titleSearchText = SortTitleTb.Text.ToLower();
            return string.IsNullOrWhiteSpace(titleSearchText) ? services : services.Where(item => item.Title.ToLower().Contains(titleSearchText));
        }

        private IQueryable<Service> FilterByDescription(IQueryable<Service> services)
        {
            string descriptionSearchText = SortDescreptionTb.Text.ToLower();
            return string.IsNullOrWhiteSpace(descriptionSearchText)
                ? services
                : services.Where(item => item.Description.ToLower().Contains(descriptionSearchText));
        }

        private IQueryable<Service> FilterByDiscount(IQueryable<Service> services)
        {
            return services.Where(item => item.Discount >= minDiscount && item.Discount < maxDiscount);
        }

        private IQueryable<Service> SortServices(IQueryable<Service> services)
        {
            if (CostSortCb.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedContent = selectedItem.Content.ToString();

                switch (selectedContent)
                {
                    case "По возрастанию":
                        return services.OrderBy(item => item.Cost);
                    case "По убыванию":
                        return services.OrderByDescending(item => item.Cost);
                    default:
                        break; // Просто ничего не делаем
                }
            }

            return services; // Возвращаем исходный список, если ничего не выбрано или нет соответствия
        }

        private void CostSortCb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateServiceList();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateServiceList();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortDiscountCb.SelectedItem is ComboBoxItem selectedItem)
            {
                string selectedContent = selectedItem.Content.ToString();

                // Сброс значений мин и макс скидки
                minDiscount = 0;
                maxDiscount = 100; // Например, максимальная скидка — 100

                // Устанавливаем значения мин и макс скидки в зависимости от выбора
                switch (selectedContent)
                {
                    case "от 0 до 5":
                        maxDiscount = 5;
                        break;
                    case "от 5 до 15":
                        minDiscount = 5;
                        maxDiscount = 15;
                        break;
                    case "от 15 до 30":
                        minDiscount = 15;
                        maxDiscount = 30;
                        break;
                    case "от 30 до 70":
                        minDiscount = 30;
                        maxDiscount = 70;
                        break;
                    case "от 70 до 100":
                        minDiscount = 70;
                        maxDiscount = 100;
                        break;
                    case "Пусто":
                        // Задаем значения так, чтобы не было найденных услуг

                        break;
                }

                UpdateServiceList();
            }
        }

        private void SortDescreptionTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateServiceList();
        }

        private void SortReset_Click(object sender, RoutedEventArgs e)
        {

            ResetSortMethod();
        }

        private void ResetSortMethod()
        {
            LoadServices();
            SortDiscountCb.SelectedIndex = 0;
            CostSortCb.SelectedIndex = 0;
            SortTitleTb.Clear();
            SortDescreptionTb.Clear();
            UpdateListView();
        }

        private void EditServiceBtn_Click(object sender, RoutedEventArgs e)
        {
            // Получаем кнопку, которая вызвала событие
            var button = sender as Button;

            // Получаем родительский ListViewItem
            var item = FindAncestor<ListViewItem>(button);
            if (item != null)
            {
                // Получаем данные из DataContext элемента
                var serviceData = item.DataContext as Service; // замените YourServiceModel на ваш класс модели

                // Передаем данные в новое окно

                var editWindow = new EditServiceWindow(serviceData);
                editWindow.DataUpdated += EditWindow_DataUpdated;
                editWindow.ShowDialog();
            }
        }
        private T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T ancestor)
                {
                    return ancestor;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private void EditWindow_DataUpdated(object sender, EventArgs e)
        {
            RefreshListView();
        }

        private void RefreshListView()
        {
            ServiсeLv.ItemsSource = App.services;
            foreach (var serv in App.services)
            {
                serv.Cost = (int)serv.Cost;
                serv.CostWithDiscount = (int)(serv.Cost - (serv.Cost * (serv.Discount / 100)));
                serv.CostWithDiscount = (int)serv.CostWithDiscount;
                //serv.DurationInSeconds = (int)serv.DurationInSeconds / 60;
                // Если скидка больше 0, умножаем на 100, иначе оставляем как есть
                //if (serv.Discount > 0)
                //{
                // serv.Discount = (int)(serv.Discount * 100);

                //    //DbConnections.demoEntities.Service.Add(serv);
                //    //DbConnections.demoEntities.SaveChanges();
                //}  
            }
            RoutedEventArgs args = new RoutedEventArgs();
            ActivateBtn_Click(this, args);
            Dispatcher.BeginInvoke((Action)(() => SetStackPanelsVisibility(Visibility.Visible)), System.Windows.Threading.DispatcherPriority.Input);
        }

        private void DeleteServiceBtn_Click(object sender, RoutedEventArgs e)
        {

            // Получаем кнопку, которая вызвала событие
            var button = sender as Button;

            // Получаем родительский ListViewItem
            var item = FindAncestor<ListViewItem>(button);
            if (item != null)
            {
                // Получаем данные из DataContext элемента
                var serviceData = item.DataContext as Service; // замените YourServiceModel на ваш класс модели

                App.services.Remove(serviceData);

                // Удаляем из базы данных

                DbConnections.demoEntities.Service.Remove(serviceData);
                var photosToRemove = DbConnections.demoEntities.ServicePhoto.Where(x => x.ServiceID == serviceData.ID).ToList();
                DbConnections.demoEntities.ServicePhoto.RemoveRange(photosToRemove);
                DbConnections.demoEntities.SaveChanges();
                ServiсeLv.ItemsSource = App.services;
                UpdateServiceList();
                Dispatcher.BeginInvoke((Action)(() => SetStackPanelsVisibility(Visibility.Visible)), System.Windows.Threading.DispatcherPriority.Input);

            }
            // Удаляем из ObservableCollection



        }

        private void RegServicesBtn_Click(object sender, RoutedEventArgs e)
        {
            // Получаем кнопку, которая вызвала событие
            var button = sender as Button;

            // Получаем родительский ListViewItem
            var item = FindAncestor<ListViewItem>(button);
            if (item != null)
            {
                // Получаем данные из DataContext элемента
                var serviceData = item.DataContext as Service; // замените YourServiceModel на ваш класс модели

                // Передаем данные в новое окно

                NavigationService.Navigate(new RegistrationForServicesPage( serviceData));
            }
        }

        private void LookServiceBtn_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            // Получаем родительский ListViewItem
            var item = FindAncestor<ListViewItem>(button);
            if (item != null)
            {
                // Получаем данные из DataContext элемента
                var serviceData = item.DataContext as Service; // замените YourServiceModel на ваш класс модели

                // Передаем данные в новое окно
                NavigationService.Navigate(new LookRegServicesPage(serviceData));

            }
        }
    }
}
