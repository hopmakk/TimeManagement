using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using TimeManagement.Models;
using TimeManagement.Services;

namespace TimeManagement.Pages.StartPages
{
	/// <summary>
	/// Логика взаимодействия для ProjectSetterPage.xaml
	/// </summary>
	public partial class ProjectSetterPage : Page
	{
		private AppCenter _appCenter = AppCenter.GetInstance();
		private string _baseUrl;
		private string _token;


		public ProjectSetterPage()
		{
			InitializeComponent();
			_token = "";
		}


		public void SetToken(string token)
		{
			_token = token;
		}


		public async Task UpdateAsync(string baseUrl)
		{
			_baseUrl = baseUrl;

			Ahtung.Visibility = Visibility.Hidden;
			Ahtung2.Visibility = Visibility.Hidden;

			ProjectsList.Children.Clear();

			var projects = await _appCenter.YTApi.GetProjectsAsync();

			if (projects.Count == 0)
			{
				Ahtung2.Visibility = Visibility.Visible;
				return;
			}

			if (projects == null)
			{
				Ahtung.Visibility = Visibility.Visible;
				return;
			}

			foreach (var project in projects)
			{
				TextBlock newTextBlock = new TextBlock
				{
					Text = project.ShortName,
					Style = (Style)ProjectsList.Resources["DateTextBlockStyle"],
				};
				newTextBlock.MouseLeftButtonDown += ProjectTB_MouseLeftButtonDown;
				ProjectsList.Children.Add(newTextBlock);
			}
		}


		private async void ProjectTB_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			var projName = (sender as TextBlock).Text;

			(sender as TextBlock).IsEnabled = false;

			_appCenter.SettingsPage.ConfigData.WorkTypes.Clear();
			var workTypes = await _appCenter.YTApi.GetWorkTypesInProject(projName);
			workTypes.ForEach(_appCenter.SettingsPage.ConfigData.WorkTypes.Add);
			_appCenter.TaskMonitoringPage.AddWorkTypesToSidePanel(_appCenter.SettingsPage.ConfigData.WorkTypes);

			_appCenter.SettingsPage.ConfigData.BaseURL = _baseUrl;
			_appCenter.SettingsPage.ConfigData.ProjectName = projName;
			_appCenter.SettingsPage.ConfigData.TokenStatus = "Сохранён";
			_appCenter.SettingsPage.ConfigData.AppStage = AppStage.NormalUse;
			_appCenter.SettingsPage.SaveConfig();

			// проверка, сохранен ли такой токен ранее. Если да, очищаем
			if (_appCenter.WinCredService.GetYoutrackToken() != null)
				_appCenter.WinCredService.ClearYoutrackToken();
			
			// сохраняем токен
			_appCenter.WinCredService.SaveYoutrackToken(_token);

			_appCenter.MainWindow.Navigate(_appCenter.NotificationPage);
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


		private void Back_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			_appCenter.MainWindow.Navigate(_appCenter.BaseSettingsSetterPage);
		}
	}
}
