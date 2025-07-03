using System.Windows;
using System.Windows.Controls;
using TimeManagement.Services;
using TimeManagement.YouTrack;

namespace TimeManagement.Pages.StartPages
{
	/// <summary>
	/// Логика взаимодействия для BaseSettingsSetterPage.xaml
	/// </summary>
	public partial class BaseSettingsSetterPage : Page
	{
		private AppCenter _appCenter = AppCenter.GetInstance();
		private string _token;
		

		public BaseSettingsSetterPage()
		{
			InitializeComponent();
			_token = "";
		}


		public void SetToken(string token)
		{
			_token = token;
		}


		private async void NextPage_Click(object sender, RoutedEventArgs e)
		{
			Ahtung.Visibility = Visibility.Hidden;
			Ahtung2.Visibility = Visibility.Hidden;

			var baseUrl = TB.Text;

			if (baseUrl == null || baseUrl == "")
			{
				Ahtung.Visibility = Visibility.Visible;
				return;
			}

			var newApiConnecter = new YTApi(baseUrl, _token); // создаем пробный коннектор

			B_NextPage.IsEnabled = false;
			B_NextPage.Style = (Style)FindResource("ButtonRect_gray"); 

			var sucсess = await newApiConnecter.CheckSendRequest(); // пытаемся послать запрос

			if (!sucсess) // если не выходит, выводим предупреждения
			{
				B_NextPage.IsEnabled = true;
				B_NextPage.Style = (Style)FindResource("ButtonRect_green");

				Ahtung.Visibility = Visibility.Visible;
				Ahtung2.Visibility = Visibility.Visible;
				return;
			}

			// если выходит - сохраняем коннектор и переходим к след странице
			_appCenter.YTApi = newApiConnecter;
			_appCenter.ProjectSetterPage.SetToken(_token);
			await _appCenter.ProjectSetterPage.UpdateAsync(baseUrl);

			B_NextPage.IsEnabled = true;
			B_NextPage.Style = (Style)FindResource("ButtonRect_green");

			_appCenter.MainWindow.Navigate(_appCenter.ProjectSetterPage);
		}


		private void Back_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			_appCenter.MainWindow.Navigate(_appCenter.AccessTokenPage);
		}


		private void B_WhatIsBaseUrl_Click(object sender, RoutedEventArgs e)
		{
			MessageBox.Show(
				$"1) Зайдите на любую задачу в YT.\n" +
				$"2) В строке браузера скопируйте адрес до \"issue\".\n" +
				$"3) Должно выйти что-то похожее на \"https://youtrack.sovcombank.ru/\". Это и есть ваш базовый URL.\n" +
				$"","", MessageBoxButton.OK, MessageBoxImage.Information);
		}
	}
}
