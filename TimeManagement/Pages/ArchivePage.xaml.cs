using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using TimeManagement.Models;
using TimeManagement.Services;

namespace TimeManagement.Pages
{
	/// <summary>
	/// Логика взаимодействия для ArchivePage.xaml
	/// </summary>
	public partial class ArchivePage : Page, INotifyPropertyChanged
    {
		private AppCenter _appCenter = AppCenter.GetInstance();
		public ObservableCollection<TaskInfo> ArchiveTaskList { get; set; }
		public ObservableCollection<TaskInfo> DeletedTaskList { get; set; }


		public ArchivePage()
		{
			DeletedTaskList = new ObservableCollection<TaskInfo>();
			LoadTasks();
			InitializeComponent();
			DataContext = this;
			TaskItemsControl.ItemsSource = ArchiveTaskList; // Связываем список с ItemsControl
		}


		public void Update()
		{
			if (ArchiveTaskList.Any())
                B_ClearAll.Visibility = Visibility.Visible;
			else
                B_ClearAll.Visibility = Visibility.Collapsed;
        }


		public void SaveTasks()
		{
			try
			{
				_appCenter.TaskStorage.SaveTasksToArchive(new List<TaskInfo>(ArchiveTaskList));
			}
			catch (Exception ex)
			{
				_appCenter.LogService.SaveLogError(ex, "Ошибка при сохранении задач");
			}
		}


		private void LoadTasks()
		{
			try
			{
				ArchiveTaskList = new ObservableCollection<TaskInfo>(_appCenter.TaskStorage.LoadTasksFromArchive());
			}
			catch (Exception ex)
			{
				_appCenter.MainWindow.SaveDatasByExit = false;
				_appCenter.LogService.SaveLogError(ex, "Ошибка при загрузке задач");
				ArchiveTaskList = new ObservableCollection<TaskInfo>();
			}
		}


		private void PopupButton_Click(object sender, RoutedEventArgs e)
		{
			// Находим кнопку, которая вызвала событие
			Button actionButton = sender as Button;
			if (actionButton != null)
			{
				// Находим связанный с кнопкой Popup
				var parent = actionButton.Parent as Grid;
				if (parent != null)
				{
					Popup actionPopup = parent.FindName("ActionPopup") as Popup;
					if (actionPopup != null)
					{
						// Показываем или скрываем Popup
						actionPopup.IsOpen = !actionPopup.IsOpen;
					}
				}
			}
		}


		private void PopupComeBackAction_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var task = ((Button)sender).DataContext as TaskInfo;
			if (task != null)
			{
				task.Status = Models.TaskStatus.New;
				ArchiveTaskList.Remove(task);
				_appCenter.TaskMonitoringPage.MainTaskList.Add(task);
            }
            Update();
        }


		private void PopupDeleteAction_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var task = ((Button)sender).DataContext as TaskInfo;
			if (task != null)
			{
				task.Status = Models.TaskStatus.Deleted;
				ArchiveTaskList.Remove(task);
				DeletedTaskList.Add(task);
			}
            Update();
        }


        private void DeletedGoBack_Click(object sender, RoutedEventArgs e)
        {
            if (DeletedTaskList.Count == 0)
            {
                var message = "Не найдено недавно удалённых из архива задач. Удаленные из архива задачи невозможно восстановить после перезапуска приложения.";
                _appCenter.NotificationService.ShowNotification(NotificationType.Hint, "Нечего восстанавливать", message);
            }

            foreach (var task in DeletedTaskList.ToList())
            {
                task.Status = Models.TaskStatus.InArchive;
                ArchiveTaskList.Add(task);
                DeletedTaskList.Remove(task);
            }
            Update();
        }


        private void B_ClearAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var task in ArchiveTaskList.ToList())
            {
                task.Status = Models.TaskStatus.Deleted;
                ArchiveTaskList.Remove(task);
                DeletedTaskList.Add(task);
            }
            Update();
        }


        public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string prop = "")
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(prop));
		}
    }
}
