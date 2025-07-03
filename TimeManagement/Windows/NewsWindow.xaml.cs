using System.Windows;
using TimeManagement.Services;

namespace TimeManagement.Windows
{
    /// <summary>
    /// Логика взаимодействия для NewsWindow.xaml
    /// </summary>
    public partial class NewsWindow : Window
    {
        private AppCenter _appCenter = AppCenter.GetInstance();

        public NewsWindow()
        {
            InitializeComponent();
            TB_Version.Text = _appCenter.Version;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
