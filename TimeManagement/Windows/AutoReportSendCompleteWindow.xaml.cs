using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using TimeManagement.Models;
using TimeManagement.Services;

namespace TimeManagement.Windows
{
    /// <summary>
    /// Логика взаимодействия для AutoReportSendCompleteWindow.xaml
    /// </summary>
    public partial class AutoReportSendCompleteWindow : Window, INotifyPropertyChanged
	{
		private AppCenter _appCenter = AppCenter.GetInstance();
		public ObservableCollection<TaskInfoForReport> TasksForReport { get; set; }

		private DateTime _date;


		public AutoReportSendCompleteWindow(DateTime date)
		{
			_date = date.Date;

			//TB_MainDate.Text = date.Date.ToString().Substring(0, 10);

			TasksForReport = new ObservableCollection<TaskInfoForReport>();
			DataContext = this;
			InitializeComponent();
			TaskItemsControl.ItemsSource = TasksForReport;

			_appCenter.MainWindow.IsEnabled = false;
			OkButton.IsEnabled = false;
			OkButton.Visibility = Visibility.Hidden;
			TB_MainDate.Text = "В процессе";
		}


		public async Task Start()
		{
			// есть ли связь с YT
			var connectionSuccess = await _appCenter.YTApi.CheckSendRequest();
			if (!connectionSuccess)
			{
				_appCenter.MainWindow.IsEnabled = true;
				OkButton.IsEnabled = true;
				OkButton.Visibility = Visibility.Visible;
				TB_MainDate.Text = "Ошибка связи с YT";
				return;
			}

			// есть ли незатреканное время
			var haveUntrackedTimeToday = false;
			var tasks = new List<TaskInfo>(_appCenter.TaskMonitoringPage.MainTaskList);
			foreach (var task in tasks)
			{
				// если в этот день по этой задаче есть незатреканное время
				if (task.GetUntrackedTimeInDay(_date) != 0)
				{
					haveUntrackedTimeToday = true;
					break;
				}
			}

			// Если нет затреканного времени - завершаемся
			if (!haveUntrackedTimeToday)
			{
				_appCenter.MainWindow.IsEnabled = true;
				Close();
				return;
			}

			// начинаем автотрек
			_appCenter.MainWindow.Navigate(_appCenter.ReportPage);
			_appCenter.ReportPage.NextPage(_date);
			_appCenter.ReportPage2.RoundTimeInAllTasks();
			_appCenter.ReportPage2.NextPage();
			await _appCenter.ReportPage3.SendTimeToYT();

			// отображаем окончание
			_appCenter.MainWindow.IsEnabled = true;
			OkButton.IsEnabled = true;
			OkButton.Visibility = Visibility.Visible;
			TB_MainDate.Text = $"Завершён в {DateTime.Now.TimeOfDay.ToString().Substring(0, 5)}";

			foreach (var task in _appCenter.ReportPage3.TasksForReport)
				TasksForReport.Add(task);
		}


		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
        }


        // <!--COMMENT FEATURE-->
        //     private void OpenComment_Click(object sender, RoutedEventArgs e)
        //     {
        //var button = sender as Button;
        //var container = (button).Parent as Grid;
        //var commentPanel = (Grid)container.FindName("CommentPanel");
        //         commentPanel.Height = 30;
        //button.Visibility = Visibility.Hidden;
        //     }


        // <!--COMMENT FEATURE-->
        //     private async void ChangeWorkEventDescription_Click(object sender, RoutedEventArgs e)
        //     {
        //         var container = ((Button)sender).Parent as Grid;
        //var taskInfo = container.DataContext as TaskInfoForReport;
        //         var commentText = ((TextBox)container.FindName("TB_Comment")).Text;

        //         var workEvents = await _appCenter.YTApi.GetWorkTimesByIssueIdAsync(taskInfo.TaskId);
        //WorkItem workEvent = null;

        //         try
        //{
        //	for (int i = workEvents.Count - 1; i >= 0 ; i--)
        //	{
        //                 if (workEvents[i].Duration.TotalMinutes == (int)Math.Round(taskInfo.UntrackedSeconds / 60))
        //		{
        //                     workEvent = workEvents[i];
        //			break;
        //                 }
        //	}
        //         }
        //         catch (Exception ex)
        //{
        //             _appCenter.NotificationService.ShowNotification(NotificationType.Warning, "Ивент работы не найден", $"Возможно вы изменяли этот ивент напрямую через YT.");
        //             return;
        //}

        //         workEvent.Description = commentText;
        //         await _appCenter.YTApi.UpdateWorkItemByIssueIdAsync(taskInfo.TaskId, workEvent.Id, workEvent);
        //     }


        public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string prop = "")
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
