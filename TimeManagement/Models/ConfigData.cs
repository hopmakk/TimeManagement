using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using YouTrackSharp.TimeTracking;

namespace TimeManagement.Models
{
	public class ConfigData : INotifyPropertyChanged
	{
        public AppStage AppStage { get; set; }

		#region BaseURL
		public string BaseURL
		{
			get { return _baseURL; }
			set
			{
				_baseURL = value;
				OnPropertyChanged(nameof(BaseURL));
			}
		}
		private string _baseURL;
		#endregion

		#region ProjectName
		public string ProjectName
		{
			get { return _projectName; }
			set
			{
				_projectName = value;
				OnPropertyChanged(nameof(ProjectName));
			}
		}
		private string _projectName;
		#endregion

		#region TokenStatus
		public string TokenStatus
		{
			get { return _tokenStatus; }
			set
			{
				_tokenStatus = value;
				OnPropertyChanged(nameof(TokenStatus));
			}
		}
		private string _tokenStatus;
		#endregion

		#region AutoTrack
		public bool AutoTrack
		{
			get { return _autoTrack; }
			set
			{
				_autoTrack = value;
				OnPropertyChanged(nameof(AutoTrack));
			}
		}
		private bool _autoTrack;
		#endregion

		#region AutoTrackTime
		public string AutoTrackTime
		{
			get { return _autoTrackTime; }
			set
			{
				_autoTrackTime = value;
				OnPropertyChanged(nameof(AutoTrackTime));
			}
		}
		private string _autoTrackTime;
		#endregion

		#region TaskPlayerActive
		public bool TaskPlayerActive
		{
			get { return _taskPlayerActive; }
			set
			{
				_taskPlayerActive = value;
				OnPropertyChanged(nameof(TaskPlayerActive));
			}
		}
		private bool _taskPlayerActive = true;
		#endregion

		#region TaskPlayerShowUntrackedTime
		public bool TaskPlayerShowUntrackedTime
		{
			get { return _taskPlayerShowUntrackedTime; }
			set
			{
				_taskPlayerShowUntrackedTime = value;
				OnPropertyChanged(nameof(TaskPlayerShowUntrackedTime));
			}
		}
		private bool _taskPlayerShowUntrackedTime = true;
        #endregion

        #region CurrentAppVersion
        public string CurrentAppVersion
        {
            get { return _currentAppVersion; }
            set
            {
                _currentAppVersion = value;
                OnPropertyChanged(nameof(CurrentAppVersion));
            }
        }
        private string _currentAppVersion;
        #endregion


        public ObservableCollection<WorkType> WorkTypes { get; set; } = new ObservableCollection<WorkType>();


		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string prop = "")
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(prop));
		}
	}


    public enum AppStage
    {
        BaseSetterStage,
        Training,
        NormalUse,
    }
}
