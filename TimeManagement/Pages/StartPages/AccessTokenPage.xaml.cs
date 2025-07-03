using System.Windows;
using System.Windows.Controls;
using TimeManagement.Models;
using TimeManagement.Services;
using TimeManagement.YouTrack;

namespace TimeManagement.Pages.StartPages
{
	/// <summary>
	/// Логика взаимодействия для AccessTokenPage.xaml
	/// </summary>
	public partial class AccessTokenPage : Page
	{
		private AppCenter _appCenter = AppCenter.GetInstance();


		public AccessTokenPage()
		{
			InitializeComponent();
		}


		private void NextPage_Click(object sender, RoutedEventArgs e)
		{
			Ahtung.Visibility = Visibility.Hidden;

			var token = TB.Text;

			if (token == null || token == "")
			{
				Ahtung.Visibility = Visibility.Visible;
				return;
			}

			//if (token == "test")
			//{
			//	SkipBaseSettings();
			//	return;
			//}

			_appCenter.BaseSettingsSetterPage.SetToken(token);
			_appCenter.MainWindow.Navigate(_appCenter.BaseSettingsSetterPage);
		}


		private void SkipToken_Click(object sender, RoutedEventArgs e)
		{
			SkipBaseSettings();
		}


		private void SkipBaseSettings()
		{
			_appCenter.SettingsPage.ConfigData.BaseURL = "-";
			_appCenter.SettingsPage.ConfigData.ProjectName = "-";
			_appCenter.SettingsPage.ConfigData.TokenStatus = "Отсутствует";
			_appCenter.SettingsPage.SaveConfig();

			_appCenter.YTApi = new YTApi("");

			_appCenter.MainWindow.Navigate(_appCenter.TaskMonitoringPage);
			_appCenter.MainWindow.OpenNavPanel();

			var message =
				"Осуществляйте навигацию по приложению с помощью боковой панели: \n\n" +
				"1) Задачи: добавляйте задачи из своего проекта YouTrack и отслеживайте время. \n\n" +
				"2) Отчёты: списывайте время на задачи в YouTrack. \n\n" +
				"3) Архив: отложите или удалите ненужные задачи. \n\n" +
				"4) Уведомления: отслеживайте работоспособность приложения. \n\n" +
				"5) Настройки: меняйте параметры и включайте автотрек времени. \n\n" +
				"6) Инфо: Узнайте дополнительную информацию. \n\n" +
				"Приятного использования!";
			_appCenter.NotificationService.ShowNotification(NotificationType.Hint, "Обзор", message);
		}
    }
}
