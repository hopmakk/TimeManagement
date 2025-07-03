using System.Collections.ObjectModel;
using System.Windows.Controls;
using TimeManagement.Models;
using TimeManagement.Services;

namespace TimeManagement.Pages
{
	/// <summary>
	/// Логика взаимодействия для ReportPage2.xaml
	/// </summary>
	public partial class ReportPage2 : Page
    {
		private AppCenter _appCenter = AppCenter.GetInstance();

		public ObservableCollection<TaskInfoForReport> TasksForReport { get; set; }

		private DateTime _date;
		private const int IncOrDecMinutes = 5;
		private const int IncOrDecMinutesBig = 60;


		public ReportPage2()
        {
			TasksForReport = new ObservableCollection<TaskInfoForReport>();
			DataContext = this;
            InitializeComponent();
			TaskItemsControl.ItemsSource = TasksForReport;
		}


        public void Update(DateTime date)
		{
			_date = date;
			TB_MainDate.Text = date.Date.ToString().Substring(0, 10);

			TasksForReport.Clear();
			var tasks = new List<TaskInfo>(_appCenter.TaskMonitoringPage.MainTaskList);
			var workTypeMas = _appCenter.SettingsPage.ConfigData.WorkTypes;

			foreach (var task in tasks) // проверяем каждую из задач
			{
				foreach (var workType in workTypeMas) // для каждого типа работ
				{
					var untrackedTimeInDay = task.GetUntrackedTimeInDay(date, workType);
					if (untrackedTimeInDay != 0) // если в этот день по этой задаче есть незатреканное время 
					{
						TasksForReport.Add(new TaskInfoForReport(task, date, workType)); // добавляем задачу
					}
				}
			}
				
		}


		private void TimeUp_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			IncOrDecTimeInTask(sender, IncOrDecMinutes * 60);
		}


		private void TimeDown_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			IncOrDecTimeInTask(sender, -IncOrDecMinutes * 60);
		}


		private void TimeBigUp_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			IncOrDecTimeInTask(sender, IncOrDecMinutesBig * 60);
		}


		private void TimeBigDown_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			IncOrDecTimeInTask(sender, -IncOrDecMinutesBig * 60);
		}


		private void RoundTime_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			var taskForReport = (sender as Button).DataContext as TaskInfoForReport;
			RoundTaskReportTime(taskForReport);
		}


		public void RoundTimeInAllTasks()
		{
			foreach (var taskForReport in TasksForReport)
				RoundTaskReportTime(taskForReport);
		}


		private void RoundTaskReportTime(TaskInfoForReport taskForReport)
		{
			var minutes = taskForReport.UntrackedSeconds / 60;

			if (minutes < 7.5)
				minutes = 5;
			else
				minutes = Math.Round(minutes / 5) * 5;

			if (minutes > 0)
				taskForReport.UntrackedSeconds = minutes * 60;
		}


		private void IncOrDecTimeInTask(object sender, int seconds)
		{
			var taskForReport = (sender as Button).DataContext as TaskInfoForReport;
			if (taskForReport.UntrackedSeconds + seconds > 0)
				taskForReport.UntrackedSeconds += seconds;
			else
				_appCenter.NotificationService.ShowNotification(NotificationType.Hint,
					"Время не может быть отрицательным",
					$"Вы попытались уменьшить значение времени, но оно стало отрицательным или равным нулю.\nВыражение: {taskForReport.UntrackedTime}  -  {Math.Abs(seconds)/60}м  <=  0м");
		}


		private void NextPage_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			NextPage();
		}


		public void NextPage()
		{
			_appCenter.ReportPage3.Update(_date, TasksForReport);
			_appCenter.MainWindow.Navigate(_appCenter.ReportPage3);
		}


        private void B_SelectAll_Click(object sender, System.Windows.RoutedEventArgs e)
        {
			if (TasksForReport.Where(t => t.AllowToSend == false).Any())
			{
				foreach (var task in TasksForReport)
					task.AllowToSend = true;
            }
			else
			{
                foreach (var task in TasksForReport)
                    task.AllowToSend = false;
            }
        }
    }
}
