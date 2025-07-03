using System.ComponentModel;
using System.Runtime.CompilerServices;
using YouTrackSharp.TimeTracking;

namespace TimeManagement.Models
{
	public class TaskInfoForReport : INotifyPropertyChanged
	{
		public string TaskId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public TaskInfo Original { get; set; }
		public WorkType WorkType { get; set; }
		public DateTime CreatedTime { get; set; }

		public double UntrackedSeconds
		{
			get { return _untrackedSeconds; }
			set 
			{ 
				_untrackedSeconds = value;
				UntrackedTime = TaskInfo.SecToStrTime(UntrackedSeconds);
			}
		}
		private double _untrackedSeconds;

		#region AllowToSend
		public bool AllowToSend
		{
			get { return _allowToSend; }
			set
			{
				_allowToSend = value;
				OnPropertyChanged(nameof(AllowToSend));
			}
		}
		private bool _allowToSend;
		#endregion

		#region SendSuccess
		public SendStepTypes SendSuccess
		{
			get { return _sendSuccess; }
			set
			{
				_sendSuccess = value;
				OnPropertyChanged(nameof(SendSuccess));
			}
		}
		private SendStepTypes _sendSuccess;
		#endregion

		#region UntrackedTime
		public string UntrackedTime
		{
			get { return _untrackedTime; }
			set
			{
				_untrackedTime = value;
				OnPropertyChanged(nameof(UntrackedTime));
			}
		}
		private string _untrackedTime;
		#endregion


		public TaskInfoForReport(TaskInfo original, DateTime reportDate, WorkType workType)
		{
			AllowToSend = true;
			Original = original;
			TaskId = original.TaskId;
			Name = original.Name;
			WorkType = workType;
            CreatedTime = default(DateTime);
            UntrackedSeconds = original.GetUntrackedTimeInDay(reportDate, workType);
			Description = "";
			SendSuccess = SendStepTypes.None;
		}


		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string prop = "")
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(prop));
		}
	}


	public enum SendStepTypes
	{
		None = 0,
		Sending = 1,
		Success = 2,
		Error = 3,
	}
}
