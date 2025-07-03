using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using TimeManagement.Models;
using TimeManagement.Services;
using YouTrackSharp.TimeTracking;

namespace TimeManagement.Pages
{
	/// <summary>
	/// Логика взаимодействия для TaskMonitoringPage.xaml
	/// </summary>
	public partial class TaskMonitoringPage : Page, INotifyPropertyChanged
    {
		private AppCenter _appCenter = AppCenter.GetInstance();
		public ObservableCollection<TaskInfo> MainTaskList { get; set; }

		private TaskInfo _taskInfoForWorkTypeSelect;
		private DispatcherTimer _taskSaveTimer;
		private const int _taskSavePeriodInSec = 60;

		#region ActiveTaskStatus
		public string ActiveTaskStatus
		{
			get { return _activeTaskStatus; }
			set
			{
				_activeTaskStatus = value;
				OnPropertyChanged(nameof(ActiveTaskStatus));
			}
		}
		private string _activeTaskStatus;
		#endregion


		public TaskMonitoringPage()
        {
			LoadTasks();
            
			InitializeComponent();
            DataContext = this;
            TaskItemsControl.ItemsSource = MainTaskList; // Связываем список с ItemsControl

			_taskSaveTimer = new DispatcherTimer();
			_taskSaveTimer.Interval = new TimeSpan(0, 0, _taskSavePeriodInSec);
			_taskSaveTimer.Tick += TaskSaveTimer_Tick;
            _taskSaveTimer.Start();

			// обновить время по задачам
            foreach (var task in MainTaskList)
				task.UpdateShowingTime();

			ActiveTaskStatus = "Активных задач нет";

			AddWorkTypesToSidePanel(_appCenter.SettingsPage.ConfigData.WorkTypes);
		}


		private void TaskSaveTimer_Tick(object? sender, EventArgs e)
		{
            SaveTasks();
            _appCenter.ArchivePage.SaveTasks();
		}


		// Сохранить текущие задачи
		public void SaveTasks()
        {
            try
            {
				var savingTasks = new List<TaskInfo>(MainTaskList);
				_appCenter.TaskStorage.SaveTasks(savingTasks);
			}
            catch (Exception ex)
            {
				_appCenter.LogService.SaveLogError(ex, "Ошибка при сохранении задач");
			}
		}


		// Загрузить сохраненные задачи
		private void LoadTasks()
		{
            try
            {
				var loadedTasks = _appCenter.TaskStorage.LoadTasks();
				MainTaskList = new ObservableCollection<TaskInfo>(loadedTasks);
			}
			catch (Exception ex)
			{
				_appCenter.MainWindow.SaveDatasByExit = false;
				_appCenter.LogService.SaveLogError(ex, "Ошибка при загрузке задач");
				MainTaskList = new ObservableCollection<TaskInfo>();
			}
		}


		public void AddWorkTypesToSidePanel(ObservableCollection<WorkType> workTypes)
		{
			WorkTypeList.Children.Clear();
			foreach (var wt in workTypes)
			{
				TextBlock newTextBlock = new TextBlock
				{
					Text = wt.Name,
					Style = (Style)WorkTypeList.Resources["WTTextBlockStyle"],
				};
				newTextBlock.MouseLeftButtonDown += WorkTypeTB_MouseLeftButtonDown; ;
				WorkTypeList.Children.Add(newTextBlock);
			}
		}


		private void WorkTypeTB_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var WTName = (sender as TextBlock).Text;

			if (_taskInfoForWorkTypeSelect != null)
			{
				_taskInfoForWorkTypeSelect.StandartWorkType = 
					_appCenter.SettingsPage.ConfigData.WorkTypes
					.Where(wt => wt.Name == WTName)
					.FirstOrDefault();

				WorkTypeListPanel.Width = 0;
			}
		}


		private void WorkTypeSelectTB_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var task = (TaskInfo)((TextBlock)sender).DataContext;

			if (!task.IsActive)
			{
				_taskInfoForWorkTypeSelect = task;
				TB_TaskInfoForSelectWorkType.Text = _taskInfoForWorkTypeSelect.TaskId;
				WorkTypeListPanel.Width = 250;
			}
		}


		private void WorkTypeListPanelClose_Click(object sender, RoutedEventArgs e)
		{
			WorkTypeListPanel.Width = 0;
		}


		private T FindChild<T>(DependencyObject parent) where T : DependencyObject
		{
			// Проходим по всем дочерним элементам
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
			{
				var child = VisualTreeHelper.GetChild(parent, i);

				if (child is T)
				{
					return (T)child;
				}

				// Рекурсивно ищем дальше в дереве
				var childOfChild = FindChild<T>(child);
				if (childOfChild != null)
				{
					return childOfChild;
				}
			}

			return null;
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
		private void PopupOpenInYTAction_Click(object sender, RoutedEventArgs e)
		{
			var task = ((Button)sender).DataContext as TaskInfo;
			_appCenter.YTApi.OpenIssueInBrowser(task.TaskId);
		}
		private void PopupDeleteAction_Click(object sender, RoutedEventArgs e)
		{
			var task = ((Button)sender).DataContext as TaskInfo;
			if (task != null)
			{
				// Получаем контейнер
				var container = TaskItemsControl.ItemContainerGenerator.ContainerFromItem(task) as ContentPresenter;
				var button = (Button)container.ContentTemplate.FindName("TaskButton", container);

				// если задача активная - деактивировать
				if (button.Style == (Style)FindResource("ButtonRectVolumeActive1"))
				{
					DeactiveTaskButton(button);
					_appCenter.TopmostTaskPlayer.Hide();
					_appCenter.TopmostTaskPlayer.SelectedTaskInfo = null;
				}

				MainTaskList.Remove(task);
				_appCenter.ArchivePage.ArchiveTaskList.Add(task);
				task.Status = Models.TaskStatus.InArchive;
			}
		}
		private async void PopupUpdateAction_Click(object sender, RoutedEventArgs e)
		{
			var button = (Button)sender;
			var task = (button).DataContext as TaskInfo;

			button.IsEnabled = false;
			var workItems = await _appCenter.YTApi.GetWorkTimesByIssueIdAsync(task.TaskId);
			if (workItems == null)
			{
				button.IsEnabled = true;
				return;
			}

			button.IsEnabled = true;
			task.SetActualDaysWorkedTime(workItems);
			var message = "Время в треке обновлено в соответствии с затреканным временем в YouTrack. Подробности можно посмотреть, раскрыв дополнительную информацию в обновленной задаче.";
			_appCenter.NotificationService.ShowNotification(NotificationType.Success, "Задача синхронизированна с YT", message);
		}
		private void PopupInfoAction_Click(object sender, RoutedEventArgs e)
		{
			var task = ((Button)sender).DataContext as TaskInfo;
			ShowOrCloseTaskLogs(task);
        }


        // открыть логи двойным кликом
        private void TaskBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
			if (e.ClickCount == 2)
				ShowOrCloseTaskLogs(((Border)sender).DataContext as TaskInfo);
        }


		// открыть или закрыть логи
		private void ShowOrCloseTaskLogs(TaskInfo task)
        {
            var minSize = new GridLength(0.0);
            var maxSize = new GridLength(150.0);

            if (task != null)
            {
                var container = TaskItemsControl.ItemContainerGenerator.ContainerFromItem(task) as ContentPresenter;
                if (container != null)
                {
                    var rowDefinition = container.ContentTemplate.FindName("LogsRowSize", container) as RowDefinition;

                    if (rowDefinition.Height != minSize)
                        rowDefinition.Height = minSize;
                    else rowDefinition.Height = maxSize;

                    // закрываем попап
                    Popup actionPopup = container.ContentTemplate.FindName("ActionPopup", container) as Popup;
                    actionPopup.IsOpen = false;
                }
            }
        }


        // активация задачи
        private void TaskButton_Click(object sender, RoutedEventArgs e)
		{
			ChangeActiveButton(sender);
		}


		public void ChangeActiveButton(object sender)
		{
			var button = (Button)sender;
			var task = (TaskInfo)button.DataContext;
			var buttonIsActive = button.Style == (Style)FindResource("ButtonRectVolumeActive1");

			if (buttonIsActive) // если кнопка активная, значит мы хотим ее деактивировать
			{
				DeactiveTaskButton(button);
			}
			else // если неактивная, хотим активироывть
			{
				if (task.StandartWorkType != null && !string.IsNullOrEmpty(task.StandartWorkType.Id))
					ActiveTaskButton(button, task);
				else
				{
					var message = $"Вы не выбрали тип работ. Нажмите на текст \"Выберите тип\" справа от кнопки активации задачи.";
					_appCenter.NotificationService.ShowNotification(NotificationType.Warning, "Выберите тип работ", message);
				}
			}
		}


		private void ActiveTaskButton(Button button, TaskInfo task)
		{
			var activeStyle = (Style)FindResource("ButtonRectVolumeActive1");

            ToggleActiveTasksByButtons();
			task.Start(task.StandartWorkType);
			button.Style = activeStyle;
			ActiveTaskStatus = $"Выполняется {task.TaskId}";

            _appCenter.TopmostTaskPlayer.SelectedTaskInfo = task;
			_appCenter.TopmostTaskPlayer.TaskButton.Style = activeStyle;
        }
		private void DeactiveTaskButton(Button button)
		{
			var deactiveStyle = (Style)FindResource("ButtonRectVolume1");

			var task = (TaskInfo)button.DataContext;
			task.Stop();
			button.Style = deactiveStyle;
			ActiveTaskStatus = $"Последняя активная задача {task.TaskId}";

			_appCenter.TopmostTaskPlayer.TaskButton.Style = deactiveStyle;
		}


        // выключить активную задачу через кнопки
        public void ToggleActiveTasksByButtons()
		{
			var taskList = new List<TaskInfo>(MainTaskList);

			foreach (var item in TaskItemsControl.Items)
			{
				try
				{
                    // Получаем контейнер
                    var container = TaskItemsControl.ItemContainerGenerator.ContainerFromItem(item) as ContentPresenter;
                    var button = (Button)container.ContentTemplate.FindName("TaskButton", container);

                    // если задача активная - деактивировать
                    if (button.Style == (Style)FindResource("ButtonRectVolumeActive1"))
                        DeactiveTaskButton(button);
                }
				catch (Exception ex)
				{
					// исключение возникает если закинуть задачу в архив, вынуть из него, после чего попытаться отправить отчет
					// пока что заглушка
				}
			}

			foreach (var task in taskList)
			{
				task.Stop();

				// на всякий случай попробуем остановить все логи
				foreach (var log in task.ActiveLog)
				{
					log.StopIfActive();
				}
			}
		}


		public Button GetTaskButtonByTask(TaskInfo taskInfo)
		{
			var container = TaskItemsControl.ItemContainerGenerator.ContainerFromItem(taskInfo) as ContentPresenter;
			var button = (Button)container.ContentTemplate.FindName("TaskButton", container);
			return button;
		}


		private void TB_CreatingTaskID_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				CreateTask_Click(B_CreateTask, null);
			}
		}


		private async void CreateTask_Click(object sender, RoutedEventArgs e)
		{
			var button = sender as Button;
			var id = TB_CreatingTaskID.Text;

			LockButton(button);

			var issue = await _appCenter.YTApi.GetIssuesByIdAsync(id);
			if (issue == null)
			{
				UnlockButton(button);
				var message = $"Возможно вы ввели несуществующий ID или есть ошибки при подключении к YT. Проверьте уведомления, пришли ли вам ошибки, связанные с доступом к YT. Если таких ошибок нет, перепроверьте вводимый ID: \"{id}\"";
				_appCenter.NotificationService.ShowNotification(NotificationType.Warning, "Не удалось получить задачу", message);
				return;
			}

			var workItems = await _appCenter.YTApi.GetWorkTimesByIssueIdAsync(id);
			if (workItems == null)
			{
				UnlockButton(button);
				return;
			}

			UnlockButton(button);
			MainTaskList.Add(new TaskInfo(issue, workItems));
			CloseTaskCreating_Click(null, null);
		}


		private void LockButton(Button button)
		{
			button.Style = (Style)FindResource("ButtonRect_gray");
			button.IsEnabled = false;
		}
		private void UnlockButton(Button button)
		{
			button.Style = (Style)FindResource("ButtonRect_green");
			button.IsEnabled = true;
		}


		private void CloseTaskCreating_Click(object sender, RoutedEventArgs e)
		{
			TB_CreatingTaskID.Text = string.Empty;
			Footer.Visibility = Visibility.Collapsed;
		}
		private void ShowFooter_Click(object sender, RoutedEventArgs e)
		{
			Footer.Visibility = Visibility.Visible;
		}


		private void DeleteActiveLogItem_Click(object sender, RoutedEventArgs e)
		{
			var button = sender as Button;
			var activeEvent = (WorkActiveEvent)button.DataContext;

			// найдем задачу, у которой в активЛоге есть нужный нам лог
			var taskInfo = MainTaskList
				.Where(t => t.ActiveLog
					.Where(l => l == activeEvent)
					.Any())
				.FirstOrDefault();

			if (taskInfo != null)
			{
				taskInfo.ActiveLog.Remove(activeEvent);
				taskInfo.UpdateShowingTime();
			}
		}


        public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string prop = "")
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(prop));
		}
    }
}
