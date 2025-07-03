using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TimeManagement.Models;
using TimeManagement.Services;
using TimeManagement.Windows;

namespace TimeManagement.Pages
{
	/// <summary>
	/// Логика взаимодействия для ReportPage.xaml
	/// </summary>
	public partial class ReportPage : Page
    {
		private AppCenter _appCenter = AppCenter.GetInstance();


		public ReportPage()
        {
            InitializeComponent();
		}

        
        public void Update()
        {
			UntrackDateItemContainer.Children.Clear();

			var tasks = new List<TaskInfo>(_appCenter.TaskMonitoringPage.MainTaskList);

            var periodOfCheck = 14;

            // смотрим за последние несколько дней
            for (DateTime i = DateTime.Now.AddDays(-periodOfCheck); i <= DateTime.Now; i = i.AddDays(1))
            {
                // проверяем каждую из задач
				foreach (var task in tasks)
				{
                    // если в этот день по этой задаче есть незатреканное время
					if (task.GetUntrackedTimeInDay(i) != 0)
                    {
                        // добавляем дату на форму
						TextBlock newDateTextBlock = new TextBlock
						{
							Text = i.Date.ToString().Substring(0, 10),
						    Style = (Style)UntrackDateItemContainer.Resources["DateTextBlockStyle"],
						};
						newDateTextBlock.MouseLeftButtonDown += DateTextBlock_MouseLeftButtonDown;
						UntrackDateItemContainer.Children.Add(newDateTextBlock);

						// переходим к след дню
						break;
					}

			    }
			}
		}


		private void DateTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var date = DateTime.Parse((sender as TextBlock).Text);
			NextPage(date);
		}


		public void NextPage(DateTime date)
		{
			_appCenter.TaskMonitoringPage.ToggleActiveTasksByButtons();
			_appCenter.ReportPage2.Update(date);
			_appCenter.MainWindow.Navigate(_appCenter.ReportPage2);
		}


		// принудительный автотрек
		private async void B_AutoTrack_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (UntrackDateItemContainer.Children.Count != 0)
			{
				var autoTrackWindow = new AutoReportSendCompleteWindow(DateTime.Now.Date);
				autoTrackWindow.Show();
				await autoTrackWindow.Start();
			}
			else
			{
				_appCenter.NotificationService.ShowNotification(NotificationType.Hint, "Нечего трекать", "Отсутствует незатреканное время по какой либо из ваших задач.");
			}
		}
    }
}
