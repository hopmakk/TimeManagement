using System.IO;
using System.Windows;
using System.Windows.Controls;
using TimeManagement.Models;
using TimeManagement.Services;

namespace TimeManagement.Pages
{
    /// <summary>
    /// Логика взаимодействия для SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
	{
		public ConfigData ConfigData { get; set; }

		private const string DefaultAutoTrackTime = "22:00";
		private AppCenter _appCenter = AppCenter.GetInstance();


        public SettingsPage()
        {
			LoadConfig();
            InitializeComponent();
			DataContext = this;

			if (ConfigData.AutoTrackTime == null)
				ConfigData.AutoTrackTime = DefaultAutoTrackTime;
		}


		public void SaveConfig()
		{
			try
			{
				_appCenter.ConfigLoader.SaveConfig(ConfigData);
			}
			catch (Exception ex)
			{
				_appCenter.LogService.SaveLogError(ex, "Ошибка при сохранении конфигов");
			}
		}


		private void LoadConfig()
		{
			try
			{
				ConfigData = _appCenter.ConfigLoader.LoadConfig();
			}
			catch (Exception ex)
			{
				_appCenter.MainWindow.SaveDatasByExit = false;
				_appCenter.LogService.SaveLogError(ex, "Ошибка при загрузке конфигов");
				ConfigData = new ConfigData();
			}
		}


		private void B_ProjectData_Click(object sender, RoutedEventArgs e)
		{
			var response = MessageBox.Show($"Уверены, что хотите заново ввести данные? \nТокен, проект и базовый URL придётся заполнять заново. Ваши задачи и другая информация останутся без изменений.", "", MessageBoxButton.OKCancel, MessageBoxImage.Question);
			if (response == MessageBoxResult.OK)
			{
				_appCenter.WinCredService.ClearYoutrackToken();
				ConfigData.AppStage = AppStage.BaseSetterStage;

				MessageBox.Show($"Перезагрузите приложение.", "", MessageBoxButton.OK, MessageBoxImage.Information);
				Application.Current.Shutdown();
			}
		}


		private void B_AllDataBreak_Click(object sender, RoutedEventArgs e)
		{
			var response = MessageBox.Show($"Уверены, что хотите сбросить данные приложения? \nИз приложения удалятся конфигурации, ваши задачи и другие данные.", "", MessageBoxButton.OKCancel, MessageBoxImage.Question);
			if (response == MessageBoxResult.OK)
			{
				var response2 = MessageBox.Show($"Процесс будет необратим.", "", MessageBoxButton.OKCancel, MessageBoxImage.Question);
				if (response2 == MessageBoxResult.OK)
				{
					MessageBox.Show($"Перезагрузите приложение.", "", MessageBoxButton.OK, MessageBoxImage.Information);

					_appCenter.WinCredService.ClearYoutrackToken();
					string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TimeManagementApp");
					Directory.Delete(folderPath, true);
					Application.Current.Shutdown();
				}
			}
		}


		private void AutoTrackTimeUp_Click(object sender, RoutedEventArgs e)
		{
			var time = DateTime.Parse(TB_AutoTrackTime.Text);
			time = time.AddMinutes(30);
			ConfigData.AutoTrackTime = time.TimeOfDay.ToString().Substring(0, 5);
		}


		private void AutoTrackTimeDown_Click(object sender, RoutedEventArgs e)
		{
			var time = DateTime.Parse(TB_AutoTrackTime.Text);
			time = time.AddMinutes(-30);
			ConfigData.AutoTrackTime = time.TimeOfDay.ToString().Substring(0, 5);
		}

    }
}
