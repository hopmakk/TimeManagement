using System.ComponentModel;
using System.Runtime.CompilerServices;
using YouTrackSharp.TimeTracking;

namespace TimeManagement.Models
{
    public class WorkActiveEvent : INotifyPropertyChanged
	{
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public WorkType WorkType { get; set; }


		#region ShortDateStr
		public string ShortDateStr
		{
			get { return _shortDateStr; }
			set
			{
				_shortDateStr = value;
				OnPropertyChanged(nameof(ShortDateStr));
			}
		}
		private string _shortDateStr;
		#endregion

		#region DurationStr
		public string DurationStr
		{
			get { return _durationStr; }
			set
			{
				_durationStr = value;
				OnPropertyChanged(nameof(DurationStr));
			}
		}
		private string _durationStr;
		#endregion


		#region TimeBetweenStr
		public string TimeBetweenStr
		{
			get { return _timeBetweenStr; }
			set
			{
				_timeBetweenStr = value;
				OnPropertyChanged(nameof(TimeBetweenStr));
			}
		}
		private string _timeBetweenStr;
		#endregion


		public WorkActiveEvent(WorkType workType)
		{
			WorkType = workType;

			TimeBetweenStr = "-";
			DurationStr = "-";
		}


		public void Start() 
		{
			StartTime = DateTime.Now;
			ShortDateStr = StartTime.ToString().Substring(0, 10).Remove(6, 2);
		}


		public void StopIfActive()
		{
			if (EndTime == default(DateTime))
				EndTime = DateTime.Now;
		}


		public double GetDurationSec()
		{
			var endTime = EndTime == DateTime.MinValue ? DateTime.Now : EndTime;
			var durationSec = (endTime - StartTime).TotalSeconds;

			var durationSecStr = TaskInfo.SecToStrTime(durationSec);
			DurationStr = durationSecStr;
			TimeBetweenStr = $"{StartTime.TimeOfDay.ToString().Substring(0, 5)} - {endTime.TimeOfDay.ToString().Substring(0, 5)}";

			return durationSec;
		}



		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string prop = "")
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(prop));
		}
	}
}
