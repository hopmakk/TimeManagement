using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TimeManagement.Models;
using TimeManagement.Services;

namespace TimeManagement.Pages
{
	/// <summary>
	/// Логика взаимодействия для ReportPage3.xaml
	/// </summary>
	public partial class ReportPage3 : Page
	{
		private AppCenter _appCenter = AppCenter.GetInstance();
		public ObservableCollection<TaskInfoForReport> TasksForReport { get; set; }

		private DateTime _date;


		public ReportPage3()
		{
			TasksForReport = new ObservableCollection<TaskInfoForReport>();
			DataContext = this;
			InitializeComponent();
			TaskItemsControl.ItemsSource = TasksForReport;
		}


		public void Update(DateTime date, ObservableCollection<TaskInfoForReport> _tasksForReport)
		{
			_date = date;
			TB_MainDate.Text = date.Date.ToString().Substring(0, 10);

			TasksForReport.Clear();
			foreach (var task in _tasksForReport)
				if (task.AllowToSend)
					TasksForReport.Add(task);

			SendYTButton.IsEnabled = true;
			SendYTButton.Style = (Style)FindResource("ButtonRect_pink");
		}


		private async void SendYTButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			await SendTimeToYT();
		}


		public async Task SendTimeToYT()
		{
			SendYTButton.IsEnabled = false;
			SendYTButton.Style = (Style)FindResource("ButtonRect_gray");
			var hasErrors = false;

			// проверим на возможность отправки
			var sucсessSendAvalibale = await _appCenter.YTApi.CheckSendRequest(); // пытаемся послать запрос
			if (sucсessSendAvalibale)
			{
				// проходим все задачи для отправки
				foreach (var task in TasksForReport)
				{
					task.SendSuccess = SendStepTypes.Sending;
					task.CreatedTime = DateTime.Now;

                    // трекаем время
                    var sucсessTrack = await _appCenter.YTApi.TrackTimeForTask(_date, task.TaskId, (int)Math.Round(task.UntrackedSeconds / 60), task.WorkType, task.CreatedTime, task.Description);
					if (sucсessTrack)
					{
						// обновляем время в приложении
						var sucсessAfterTrack = await task.Original.AfterTrackTimeActions(_date, task.WorkType);
						if (sucсessAfterTrack)
							task.SendSuccess = SendStepTypes.Success;
						else
						{
							hasErrors = true;
							var message = $"Время по задаче {task.TaskId} отправлено в YT, но произошла ошибка при обновлении времени внутри приложения. Данные в приложении и в YT рассинхронизированны. Перезапустите приложение, после чего обновите время по задаче вручную.";
							_appCenter.NotificationService.ShowNotification(NotificationType.Warning, "Время не обновлено", message);
							task.SendSuccess = SendStepTypes.Error;
						}
					}
					else
					{
						hasErrors = true;
						task.SendSuccess = SendStepTypes.Error;
					}
				}
			}
			else
			{
				hasErrors = true;
				var message = "Кажется возникли проблемы в соединении с YouTrack. Далее приведены пункты по решению проблемы, их нужно выполнять последовательно:\n" +
					"1) Подождите от нескольких минут до нескольких часов, возможно проблема на стороне YT. После чего снова попробуйте отправить отчёт.\n" +
					"2) Перезапустите приложение, проверьте статус токена в настройках.\n" +
					"3) Обновите токен в настройках.\n" +
					"4) Если вы уверены, что YT работает стабильно, а ваш токен корректен, напишите разработчику. Контакты находятся на вкладке \"Информация\".\n";
				_appCenter.NotificationService.ShowNotification(NotificationType.Error, "Не удалось отправить", message);

				foreach (var task in TasksForReport)
					task.SendSuccess = SendStepTypes.Error;
			}

			if (hasErrors)
			{
				var message = "Отчёт не отправлен или отправлен с ошибками. Проверьте списанное на задачу время в YT и локальную информацию по задачам в приложении.";
				_appCenter.NotificationService.ShowNotification(NotificationType.Warning, "Есть ошибки", message);
			}
			else
			{
				var message = "Отчёт отправлен без ошибок. Приложение синхронизированно с YT.";
				_appCenter.NotificationService.ShowNotification(NotificationType.Success, "Успешно", message);
			}
		}
	}
}
