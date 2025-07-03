using TimeManagement.Pages;
using TimeManagement.Pages.StartPages;
using TimeManagement.Services.Loaders;
using TimeManagement.Windows;
using TimeManagement.YouTrack;

namespace TimeManagement.Services
{
    public class AppCenter
    {
        public string Version { get; } = "v1.1.1";

        private AppCenter()
        {
		}

        private static AppCenter _instance;

        public static AppCenter GetInstance()
        {
            if (_instance == null)
                _instance = new AppCenter();

            return _instance;
        }


        // Окна
        public MainWindow MainWindow { get; set; }
        public TopmostTaskPlayer TopmostTaskPlayer { get; set; }

        // Страницы
        public TaskMonitoringPage TaskMonitoringPage { get; set; }
        public ReportPage ReportPage { get; set; }
        public ReportPage2 ReportPage2 { get; set; }
        public ReportPage3 ReportPage3 { get; set; }
        public ArchivePage ArchivePage { get; set; }
        public NotificationPage NotificationPage { get; set; }
        public SettingsPage SettingsPage { get; set; }
        public InfoPage InfoPage { get; set; }
        public AccessTokenPage AccessTokenPage { get; set; }
        public BaseSettingsSetterPage BaseSettingsSetterPage { get; set; }
        public ProjectSetterPage ProjectSetterPage { get; set; }

        // Сервисы
        public LogService LogService { get; set; }
        public NotificationService NotificationService { get; set; }
        public WinCredService WinCredService { get; set; }

        // Загрузчики
		public TaskStorage TaskStorage { get; set; }
		public ConfigLoader ConfigLoader { get; set; }

        // Другое
		public YTApi YTApi { get; set; }
	}
}
