using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TimeManagement.Models;

namespace TimeManagement.Pages
{
    /// <summary>
    /// Логика взаимодействия для NotificationPage.xaml
    /// </summary>
    public partial class NotificationPage : Page
    {
        public ObservableCollection<NotificationData> Notifications { get; set; }

        public Border SelectedNotifyBorder
        {
            get { return _selectedNotifyBorder; }
            set 
            {
                if (_lastBackColor != null)
                    _selectedNotifyBorder.Background = _lastBackColor;

                _selectedNotifyBorder = value;
                _lastBackColor = _selectedNotifyBorder.Background;
                _selectedNotifyBorder.Background = (Brush)FindResource("Color_5");
            }
        }
        private Border _selectedNotifyBorder;
        private Brush _lastBackColor;


        public NotificationPage()
        {
            InitializeComponent();
            DataContext = this;

            _selectedNotifyBorder = new Border();

            Notifications = new ObservableCollection<NotificationData>();
            NotificationsItemsControl.ItemsSource = Notifications;

            SelectedNotificationGrid.DataContext = NotificationData.Empty;
		}


        private void DeleteNotif_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var notification = (NotificationData)((Button)sender).DataContext;

            if (SelectedNotificationGrid.DataContext == notification)
            {
                SelectedNotificationGrid.DataContext = NotificationData.Empty;
                B_SendToDeveloper.Visibility = System.Windows.Visibility.Hidden;
            }

            Notifications.Remove(notification);
        }


        private void Notif_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var notif = (NotificationData)((Border)sender).DataContext;

            SelectedNotifyBorder = (Border)sender;
            SelectedNotificationGrid.DataContext = notif;

            if (notif.Type == NotificationType.Error)
				// пришлось пока что отключить показ кнопки отправки ошибки разработчику, не знаю как реализовать этот функционал
				//B_SendToDeveloper.Visibility = System.Windows.Visibility.Visible;
				B_SendToDeveloper.Visibility = System.Windows.Visibility.Hidden;
            else
                B_SendToDeveloper.Visibility = System.Windows.Visibility.Hidden;
        }


        private void DeleteAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SelectedNotificationGrid.DataContext = NotificationData.Empty;
            B_SendToDeveloper.Visibility = System.Windows.Visibility.Hidden;
            Notifications.Clear();
        }


        private void SendToDeveloper_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
