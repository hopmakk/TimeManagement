using System.Windows.Threading;
using TimeManagement.Models;

namespace TimeManagement.Services
{
	public class NotificationService
	{
		private AppCenter _appCenter = AppCenter.GetInstance();
		private DispatcherTimer _timer;

		public NotificationService()
		{
			_timer = new DispatcherTimer();
			_timer.Interval = new TimeSpan(0, 0, 5);
			_timer.Tick += Timer_Tick;
		}


		private void Timer_Tick(object? sender, EventArgs e)
		{
			_appCenter.MainWindow.HideNotificationBlock();
			_timer.Stop();
		}


		public void ShowNotification(NotificationType type, string title, string message = "")
		{
			var notif = new NotificationData(type, title)
			{
				Message = message,
			};

			_appCenter.NotificationPage.Notifications.Insert(0, notif);
			_appCenter.MainWindow.ShowNotificationBlock(notif);

			if (_timer.IsEnabled)
				_timer.Stop();

			_timer.Start();
		}
	}
}
