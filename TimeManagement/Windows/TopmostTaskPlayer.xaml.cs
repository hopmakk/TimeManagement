using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TimeManagement.Models;
using TimeManagement.Services;

namespace TimeManagement.Windows
{
    /// <summary>
    /// Логика взаимодействия для TopmostTaskPlayer.xaml
    /// </summary>
    public partial class TopmostTaskPlayer : Window, INotifyPropertyChanged
    {
		private AppCenter _appCenter = AppCenter.GetInstance();
        private bool _isResizing = false;
        private Point _lastMousePosition;


        #region SelectedTaskInfo
        public TaskInfo SelectedTaskInfo
		{
			get { return _selectedTaskInfo; }
			set
			{
				_selectedTaskInfo = value;
				OnPropertyChanged(nameof(SelectedTaskInfo));
			}
		}
		private TaskInfo _selectedTaskInfo;
		#endregion


		public TopmostTaskPlayer()
        {
			DataContext = this;
            InitializeComponent();

            TaskItemsControl.ItemsSource = _appCenter.TaskMonitoringPage.MainTaskList; // Связываем список с ItemsControl

            this.ResizeMode = ResizeMode.NoResize;
            this.MouseMove += Window_MouseMove;
            this.MouseDown += Window_MouseDown;
            this.MouseUp += Window_MouseUp;
        }


		private void MinimButton_Click(object sender, RoutedEventArgs e)
		{
			this.Hide();
        }


		private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
				this.DragMove();
		}


        private void OpenMainWindowButton_Click(object sender, RoutedEventArgs e)
		{
			_appCenter.MainWindow.WindowState = WindowState.Normal;
		}


		private void TaskButton_Click(object sender, RoutedEventArgs e)
		{
			var mainTaskButton = _appCenter.TaskMonitoringPage.GetTaskButtonByTask(SelectedTaskInfo);
			_appCenter.TaskMonitoringPage.ChangeActiveButton(mainTaskButton);
		}


        private void TaskListItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var task = ((Grid)sender).DataContext as TaskInfo;
            var mainTaskButton = _appCenter.TaskMonitoringPage.GetTaskButtonByTask(task);
            _appCenter.TaskMonitoringPage.ChangeActiveButton(mainTaskButton);
            ShowMoreTasks_Click(null, null);
        }


        private void ShowMoreTasks_Click(object sender, RoutedEventArgs e)
        {
            if (this.Height == 90)
                this.Height = 90 + 10 + 30 * _appCenter.TaskMonitoringPage.MainTaskList.Count + 10;
            else this.Height = 90;
        }


        // Далее методы для ресайза окна по горизонтали
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Если курсор находится справа, активировать ресайзинг
            if (IsMouseOnRightEdge(e.GetPosition(this)))
            {
                _isResizing = true;
                _lastMousePosition = e.GetPosition(this);
                this.CaptureMouse();
            }
        }
        private void Window_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _isResizing = false;
            this.ReleaseMouseCapture();
        }
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isResizing)
            {
                var currentMousePosition = e.GetPosition(this);
                var delta = currentMousePosition.X - _lastMousePosition.X;

                // Изменяем ширину окна
                if (this.Width + delta > this.MinWidth)
                {
                    this.Width += delta;
                    _lastMousePosition = currentMousePosition;
                }
            }
            else
            {
                // Меняем курсор на "resize" при наведении на правый край
                if (IsMouseOnRightEdge(e.GetPosition(this)))
                {
                    this.Cursor = Cursors.SizeWE;
                }
                else
                {
                    this.Cursor = Cursors.Arrow;
                }
            }
        }
        private bool IsMouseOnRightEdge(Point mousePosition)
        {
            const int edgeThreshold = 5; // Допустимый отступ от правого края
            return mousePosition.X >= this.ActualWidth - edgeThreshold;
        }



        public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string prop = "")
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(prop));
		}
    }
}
