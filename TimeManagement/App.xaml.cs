using System.Configuration;
using System.Data;
using System.Windows;
using TimeManagement.Services;

namespace TimeManagement
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
		private AppCenter _appCenter = AppCenter.GetInstance();


		protected override void OnStartup(StartupEventArgs e)
		{
			DispatcherUnhandledException += App_DispatcherUnhandledException;
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			base.OnStartup(e);
		}


		private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var ex = e.ExceptionObject as Exception;
			if (ex != null)
			{
				_appCenter.LogService.SaveLogError(ex, "Произошла фоновая ошибка");
			}
			else
			{
				_appCenter.LogService.SaveLogError(new Exception(), "Произошла критическая ошибка в фоновом потоке");
			}
		}


		private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
		{
			_appCenter.LogService.SaveLogError(e.Exception, "Произошла непредвиденная ошибка");
			e.Handled = true;
		}
	}

}
