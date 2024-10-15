using DemoWpf.Db;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32.SafeHandles;
using System.Collections.ObjectModel;
using Microsoft.Win32;

namespace DemoWpf.Pages
{
    /// <summary>
    /// Логика взаимодействия для EditServiceWindow.xaml
    /// </summary>
    
    public partial class EditServiceWindow : Window
    {
        public event EventHandler DataUpdated;
        public static Db.Service saveEditService = new Db.Service();
        public static Db.ServicePhoto savePhoto = new Db.ServicePhoto();
        int numberServ;
        public EditServiceWindow(Service editService)
        {
            InitializeComponent();
            saveEditService = editService;
            TitleServiceTb.Text = editService.Title;
            DescriptionServiceTb.Text = editService.Description;
            IdServiceTb.Text = editService.ID.ToString();
            // Предполагаем, что ServicePhoto — это строка с путем к файлу
            string photoPath = editService.MainImagePath.ToString();

            // Проверяем, что файл существует
            SetServiceImage(editService.MainImagePath);

            CostServiceTb.Text = Convert.ToString(editService.Cost);
            DurationServiceTb.Text = Convert.ToString(editService.DurationInSeconds);
            DiscountServiceTb.Text = Convert.ToString((int)editService.Discount);
            numberServ = Convert.ToInt32(IdServiceTb.Text);
            DisplayLastSixPhotos(numberServ);
            
            

        }
        private void CloseWindowBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveServiceBtn_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(TitleServiceTb.Text) || string.IsNullOrWhiteSpace(CostServiceTb.Text) || string.IsNullOrWhiteSpace(DurationServiceTb.Text))
            {
                MessageBox.Show("Заполните поля: название, стоимость, длительность");
            }
            else if(Convert.ToInt32(DurationServiceTb.Text) > 240 || Convert.ToInt32(DurationServiceTb.Text) < 0)
            {
                MessageBox.Show("Длительность не может быть больше 4 часов и меньше 0");
            }
            else
            {
                UpdateServiceDetails();

                DbConnections.demoEntities.SaveChanges();
                RefreshServiceList();
                DataUpdated?.Invoke(this, EventArgs.Empty);
                
            }
        }

        private void AddServiceBtn_Click(object sender, RoutedEventArgs e)
        {
            // Проверяем, существует ли услуга с таким названием
            var existingService = DbConnections.demoEntities.Service.FirstOrDefault(s => s.Title == TitleServiceTb.Text);

            if (existingService != null)
            {
                // Услуга с таким названием уже существует, показываем сообщение
                MessageBox.Show("Услуга с таким названием уже существует!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Выходим из метода, не добавляя дубликат
            }
            else
            {
                if (string.IsNullOrWhiteSpace(TitleServiceTb.Text) || string.IsNullOrWhiteSpace(CostServiceTb.Text) || string.IsNullOrWhiteSpace(DurationServiceTb.Text))
                {
                    MessageBox.Show("Заполните поля: название, стоимость, длительность");
                }
                else if (Convert.ToInt32(DurationServiceTb.Text) > 240 || Convert.ToInt32(DurationServiceTb.Text) < 0)
                {

                    MessageBox.Show("Длительность не может быть больше 4 часов и меньше 0");
                }
                else
                {
                    var newService = new Service
                    {
                        Title = TitleServiceTb.Text,
                        Cost = Convert.ToDecimal(CostServiceTb.Text),
                        DurationInSeconds = Convert.ToInt32(DurationServiceTb.Text),
                        Description = DescriptionServiceTb.Text,
                        Discount = Convert.ToInt32(DiscountServiceTb.Text),
                    };

                    DbConnections.demoEntities.Service.Add(newService);
                    DbConnections.demoEntities.SaveChanges();

                    string selectedPath = OpenImageDialog();
                    if (selectedPath != null)
                    {
                        newService.MainImagePath = selectedPath;
                        savePhoto.ServiceID = newService.ID;
                        savePhoto.PhotoPath = selectedPath;
                        numberServ = newService.ID;
                        DbConnections.demoEntities.ServicePhoto.Add(savePhoto);
                        DbConnections.demoEntities.SaveChanges();
                    }

                    
                    RefreshServiceList();

                    //var newTitleService = DbConnections.demoEntities.Service.FirstOrDefault(x => x.Title == TitleServiceTb.Text);
                    IdServiceTb.Text = newService.ID.ToString();
                    
                    PhotoServiceImg.Source = new BitmapImage(new Uri(newService.MainImagePath, UriKind.Absolute));
                    App.servicesPhoto = new ObservableCollection<ServicePhoto>(DbConnections.demoEntities.ServicePhoto.Where(x => x.ServiceID == newService.ID ).ToList());
                    PhotoServiseLv.ItemsSource = App.servicesPhoto;
                    //numberServ = Convert.ToInt32(IdServiceTb.Text);
                    //DisplayLastSixPhotos(numberServ);

                    DataUpdated?.Invoke(this, EventArgs.Empty);
                }
                // Если услуги не существует, продолжаем добавление
               
            }
           

            
        }

        private void PhotoServiceImg_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            var serviceToUpdate = App.services.FirstOrDefault(service => service.ID == numberServ);
            var serviceToUpdateNewPhoto = App.servicesPhoto.FirstOrDefault(service => service.ServiceID == numberServ);    
            // Получаем путь к выбранному изображению
            string selectedPath = OpenImageDialog();

                // Обновляем свойство MainImagePath для editService
                if (selectedPath != null)
                {
                    serviceToUpdate.MainImagePath = selectedPath;
                serviceToUpdateNewPhoto.PhotoPath = selectedPath;
                serviceToUpdateNewPhoto.ServiceID = numberServ;
                    SetServiceImage(selectedPath);
                DbConnections.demoEntities.ServicePhoto.Add(serviceToUpdateNewPhoto);
                    DbConnections.demoEntities.SaveChanges();
                    // Вызываем событие обновления данных
                    DataUpdated?.Invoke(this, EventArgs.Empty);
                }

                RefreshPhotoList();
            RefreshServiceList();
            
        }

        private void UpdateServiceDetails()
        {
            saveEditService.Title = TitleServiceTb.Text;
            saveEditService.Description = DescriptionServiceTb.Text;
            saveEditService.Cost = Convert.ToInt32(CostServiceTb.Text);
            saveEditService.DurationInSeconds = Convert.ToInt32(DurationServiceTb.Text);
            saveEditService.Discount = Convert.ToInt32(DiscountServiceTb.Text);
        }

        private string OpenImageDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Image Files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg",
                Title = "Выберите изображение"
            };

            return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : null;
        }

        private void SetServiceImage(string photoPath)
        {
            if (File.Exists(photoPath))
            {
                PhotoServiceImg.Source = new BitmapImage(new Uri(photoPath, UriKind.Absolute));
            }
            else
            {
                PhotoServiceImg.Source = null; // Заглушка
            }
        }

        private void RefreshServiceList()
        {
            var uniqueServices = DbConnections.demoEntities.Service
                .GroupBy(s => s.Title)
                .Select(g => g.FirstOrDefault())
                .ToList();

            App.services = new ObservableCollection<Service>(uniqueServices);
        }

        private void AddNewPhotoBtn_Click(object sender, RoutedEventArgs e)
        {

            string selectedPath = OpenImageDialog();
            if (selectedPath != null)
            {
                savePhoto.PhotoPath = selectedPath;
                if (File.Exists(selectedPath))
                {
                    savePhoto.ServiceID = Convert.ToInt32(IdServiceTb.Text);
                    DbConnections.demoEntities.ServicePhoto.Add(savePhoto);
                    DbConnections.demoEntities.SaveChanges();
                }
                else
                {
                    PhotoServiceImg.Source = null; // Заглушка
                }
            }
            
            numberServ = Convert.ToInt32(IdServiceTb.Text);
            DisplayLastSixPhotos(numberServ);


        }

        private void DisplayLastSixPhotos(int serviceId)
        {

            App.servicesPhoto = new ObservableCollection<ServicePhoto>(DbConnections.demoEntities.ServicePhoto.Where(x => x.ServiceID == serviceId).ToList());
            if (App.services != null)
            {
                PhotoServiseLv.ItemsSource = App.servicesPhoto;
                PhotoServiseLv.Visibility = Visibility.Visible;
            }
            else
            {
                PhotoServiseLv.Visibility = Visibility.Collapsed;
            }



        }

        private void DeleteImageBtn_Click(object sender, RoutedEventArgs e)
        {
            if (PhotoServiseLv.SelectedItem is ServicePhoto selectedPhoto)
            {
                // Удаляем элемент из коллекции
                DbConnections.demoEntities.ServicePhoto.Remove(selectedPhoto);
                DbConnections.demoEntities.SaveChanges();
                App.servicesPhoto = new ObservableCollection<ServicePhoto>(DbConnections.demoEntities.ServicePhoto.Where(x => x.ServiceID == numberServ).ToList());
                PhotoServiseLv.ItemsSource = App.servicesPhoto;
                MessageBox.Show("Изображение успешно удалено!");
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите изображение для удаления.");
            }
        }

        private void RefreshPhotoList()
        {
            App.servicesPhoto = new ObservableCollection<ServicePhoto>(DbConnections.demoEntities.ServicePhoto.Where(x => x.ServiceID == numberServ).ToList());
            PhotoServiseLv.ItemsSource = App.servicesPhoto;



        }
    }
}
