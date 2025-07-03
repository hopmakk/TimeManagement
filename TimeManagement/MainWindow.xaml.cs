using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using TimeManagement.Models;
using TimeManagement.Pages;
using TimeManagement.Pages.StartPages;
using TimeManagement.Services;
using TimeManagement.Services.Loaders;
using TimeManagement.Windows;
using TimeManagement.YouTrack;

namespace TimeManagement
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private AppCenter _appCenter = AppCenter.GetInstance();
		private DispatcherTimer _clockTimer;
		private DispatcherTimer _eventTimer;
		private Dictionary<Button, Path> _buttonAndPaths;

		public bool SaveDatasByExit { get; set; }

		private Dictionary<int, string> _dayOfWeekRus = new Dictionary<int, string>()
        {
            { 0, "ВС" },
            { 1, "ПН" },
            { 2, "ВТ" },
            { 3, "СР" },
            { 4, "ЧТ" },
            { 5, "ПТ" },
            { 6, "СБ" },
        };


		#region MainClock
		public string MainClock
		{
			get { return _mainClock; }
			set
			{
				if (_mainClock != value)
				{
					_mainClock = value;
					OnPropertyChanged(nameof(MainClock));
				}
			}
		}
		private string _mainClock;
		#endregion


		public MainWindow()
        {
			SaveDatasByExit = true;
			this.StateChanged += MainWindow_StateChanged;

			InitializeComponent();
			CreateAppCenterObjects();
			DataContext = this;

			int dayNum = (int)DateTime.Now.DayOfWeek;
			DayOfWeekTB.Text = _dayOfWeekRus[dayNum];
			DateTB.Text = DateTime.Now.Date.ToString().Substring(0, 10).Remove(6,2);
            MainClock = DateTime.Now.TimeOfDay.ToString().Substring(0, 5);

			_clockTimer = new DispatcherTimer();
			_clockTimer.Interval = new TimeSpan(0, 0, 1);
			_clockTimer.Tick += ClockTimer_Tick; ;
			_clockTimer.Start();

			_eventTimer = new DispatcherTimer();
			_eventTimer.Interval = new TimeSpan(0, 1, 0);
			_eventTimer.Tick += EventTimer_Tick;
			_eventTimer.Start();

			NotificationBlock.DataContext = new NotificationData(NotificationType.None, "");

			if (_appCenter.SettingsPage.ConfigData.AppStage == AppStage.BaseSetterStage)
			{
				CloseNavPanel();
				Navigate(_appCenter.AccessTokenPage);
			}
			else if (_appCenter.SettingsPage.ConfigData.AppStage == AppStage.NormalUse)
			{
				_appCenter.YTApi = new YTApi(_appCenter.SettingsPage.ConfigData.BaseURL);
				Navigate(_appCenter.TaskMonitoringPage);
			}

			if (_appCenter.SettingsPage.ConfigData.CurrentAppVersion == null ||
                _appCenter.SettingsPage.ConfigData.CurrentAppVersion != _appCenter.Version)
			{
                _appCenter.SettingsPage.ConfigData.CurrentAppVersion = _appCenter.Version;
                var newsWindow = new NewsWindow();
                newsWindow.Show();
            }

			if (_appCenter.TaskMonitoringPage.MainTaskList.Any())
				_appCenter.TopmostTaskPlayer.SelectedTaskInfo = _appCenter.TaskMonitoringPage.MainTaskList.First();
        }


		private void CreateAppCenterObjects()
        {
            _appCenter.MainWindow = this;

            _appCenter.NotificationPage = new NotificationPage();

			_appCenter.LogService = new LogService();
			_appCenter.NotificationService = new NotificationService();
			_appCenter.WinCredService = new WinCredService();

			_appCenter.TaskStorage = new TaskStorage();
			_appCenter.ConfigLoader = new ConfigLoader();

			_appCenter.SettingsPage = new SettingsPage();
			_appCenter.TaskMonitoringPage = new TaskMonitoringPage();
			_appCenter.ReportPage = new ReportPage();
			_appCenter.ReportPage2 = new ReportPage2();
			_appCenter.ReportPage3 = new ReportPage3();
			_appCenter.ArchivePage = new ArchivePage();
			_appCenter.InfoPage = new InfoPage();

			_appCenter.AccessTokenPage = new AccessTokenPage();
			_appCenter.BaseSettingsSetterPage = new BaseSettingsSetterPage();
			_appCenter.ProjectSetterPage = new ProjectSetterPage();

            _appCenter.TopmostTaskPlayer = new TopmostTaskPlayer();
        }


		// событие при сворачивании \ разворачивании окна
		private void MainWindow_StateChanged(object? sender, EventArgs e)
		{
			if (WindowState == WindowState.Minimized) // свернули
			{
				try
				{
					// если у нас есть активные задачи и разрешён плеер задач
					if (_appCenter.TopmostTaskPlayer.SelectedTaskInfo != null
						&& _appCenter.SettingsPage.ConfigData.TaskPlayerActive)
					{
						// смотрим показывать ли незатреканное время
						if (_appCenter.SettingsPage.ConfigData.TaskPlayerShowUntrackedTime)
							_appCenter.TopmostTaskPlayer.TB_UntrackedTime.Visibility = Visibility.Visible;
						else
							_appCenter.TopmostTaskPlayer.TB_UntrackedTime.Visibility = Visibility.Collapsed;

						// открываем его
						_appCenter.TopmostTaskPlayer.Show(); 
					}
				}
				catch { }
			}
			if (WindowState == WindowState.Normal || WindowState == WindowState.Maximized) // развернули
			{
				_appCenter.TopmostTaskPlayer.Hide();
			}
		}


		private void ClockTimer_Tick(object? sender, EventArgs e)
		{
			MainClock = DateTime.Now.TimeOfDay.ToString().Substring(0, 5);
		}


		private async void EventTimer_Tick(object? sender, EventArgs e)
		{
			// автотрек
			if (_appCenter.SettingsPage.ConfigData.AutoTrack)
			{
				var nowTime = DateTime.Now.TimeOfDay;
				var timeToAutoTrack = DateTime.Parse(_appCenter.SettingsPage.ConfigData.AutoTrackTime).TimeOfDay;

				if (nowTime > timeToAutoTrack && nowTime < timeToAutoTrack.Add(new TimeSpan(0,1,0)))
				{
					var autoTrackWindow = new AutoReportSendCompleteWindow(DateTime.Now.Date);
					autoTrackWindow.Show();
					await autoTrackWindow.Start();
				}
			}

			// смена даты
			if (DateTB.Text != DateTime.Now.Date.ToString().Substring(0, 10).Remove(6, 2))
			{
                DateTB.Text = DateTime.Now.Date.ToString().Substring(0, 10).Remove(6, 2);
            }
		}


		public void CloseNavPanel()
		{
			NavPanel.Width = new GridLength(0);
		}


		public void OpenNavPanel()
		{
			NavPanel.Width = new GridLength(50);
		}


		// перейти на другую вкладку
		private void Navigate_Click(object sender, RoutedEventArgs e)
		{
			// отключить подсветку всех элементов
			foreach (var path in _buttonAndPaths.Values)
			{
				path.Fill = (Brush)FindResource("Color_2");
				path.Stroke = (Brush)FindResource("Color_2");
			}

			// найти элемент на который мы кликнули
			var selectedIcon = _buttonAndPaths[(Button)sender];

			// выберем нажатый элемент и перейдем к нему
			switch (selectedIcon.Name)
			{
				case "PathTaskPage":
					MainPageFrame.Navigate(_appCenter.TaskMonitoringPage);
					break;
				case "PathReportPage":
					_appCenter.ReportPage.Update();
					MainPageFrame.Navigate(_appCenter.ReportPage);
					break;
				case "PathArchivePage":
                    _appCenter.ArchivePage.Update();
                    MainPageFrame.Navigate(_appCenter.ArchivePage);
					break;
				case "PathNotificationPage":
					MainPageFrame.Navigate(_appCenter.NotificationPage);
					break;
				case "PathSettingsPage":
					MainPageFrame.Navigate(_appCenter.SettingsPage);
					break;
				case "PathInfoPage":
					MainPageFrame.Navigate(_appCenter.InfoPage);
					break;
			}

			// включим ему подсветку
			selectedIcon.Fill = (Brush)FindResource("Color_1");
			selectedIcon.Stroke = (Brush)FindResource("Color_1");
		}


		// Универсальный метод для поиска дочернего элемента по типу
		private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
		{
			int childCount = VisualTreeHelper.GetChildrenCount(parent);
			for (int i = 0; i < childCount; i++)
			{
				DependencyObject child = VisualTreeHelper.GetChild(parent, i);
				if (child is T typedChild)
					return typedChild;

				T childOfChild = FindVisualChild<T>(child);
				if (childOfChild != null)
					return childOfChild;
			}
			return null;
		}


		private void Window_Closing(object sender, CancelEventArgs e)
		{
			_appCenter.TaskMonitoringPage.ToggleActiveTasksByButtons();

			if (SaveDatasByExit)
			{
				_appCenter.TaskMonitoringPage.SaveTasks();
				_appCenter.ArchivePage.SaveTasks();
				_appCenter.SettingsPage.SaveConfig();
			}

			Environment.Exit(0);
		}


		private void NavInfoPageButton_Loaded(object sender, RoutedEventArgs e)
		{
			_buttonAndPaths = new Dictionary<Button, Path>()
			{
				{NavTasksPageButton, FindVisualChild<Path>(NavTasksPageButton) },
				{NavReportPageButton, FindVisualChild<Path>(NavReportPageButton) },
				{NavArchivePageButton, FindVisualChild<Path>(NavArchivePageButton) },
				{NavNotificationPageButton, FindVisualChild<Path>(NavNotificationPageButton) },
				{NavSettingsPageButton, FindVisualChild<Path>(NavSettingsPageButton) },
				{NavInfoPageButton, FindVisualChild<Path>(NavInfoPageButton) },
			};
		}


		public void Navigate(Page page)
		{
			MainPageFrame.Navigate(page);
		}


		private void NotificationBlock_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			Navigate(_appCenter.NotificationPage);
			HideNotificationBlock();
		}
		private void DeleteNotif_Click(object sender, RoutedEventArgs e)
		{
			HideNotificationBlock();
		}
		public void HideNotificationBlock()
		{
			NotificationBlock.Visibility = Visibility.Collapsed;
		}
		public void ShowNotificationBlock(NotificationData notification)
		{
			NotificationBlock.DataContext = notification;
			NotificationBlock.Visibility = Visibility.Visible;
		}


		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string prop = "")
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(prop));
		}
	}
}