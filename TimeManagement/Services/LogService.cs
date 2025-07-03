using System.IO;
using TimeManagement.Models;

namespace TimeManagement.Services
{
	public class LogService
	{
		private AppCenter _appCenter = AppCenter.GetInstance();

		private string _folderPath;

		public LogService()
		{
			string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TimeManagementApp");
			Directory.CreateDirectory(folderPath);  // Создаем папку, если её нет
			var logForderPath = Path.Combine(folderPath, "Logs");
			Directory.CreateDirectory(logForderPath);
			_folderPath = logForderPath;
		}


		public void SaveLogError(Exception ex, string shortTitle)
		{
			var errorInfo = new List<string>
			{
				"ID ―――――――――――――――――――――",
				Guid.NewGuid().ToString(),
				"\nName ―――――――――――――――――――",
				ex.GetType().Name,
				"\nMessage ――――――――――――――――",
				ex.Message,
				"\nSource ―――――――――――――――――",
				ex.Source,
				"\nTargetSite ―――――――――――――",
				ex.TargetSite.ToString(),
				"\nStackTrace ―――――――――――――",
				ex.StackTrace,
				"\nVersion ――――――――――――――――",
				_appCenter.Version,
			};

			string filePath;
			var i = 0;
			do
			{
				filePath = Path.Combine(_folderPath, $"{DateTime.Now} {i}.txt".Replace(':', '_'));
				i++;
			}
			while (File.Exists(filePath));

			File.WriteAllLines(filePath, errorInfo);

			var message = "";
			foreach (var item in errorInfo)
			{
				message += item + '\n';
			}
			_appCenter.NotificationService.ShowNotification(NotificationType.Error, shortTitle, message);
		}
	}
}
