using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TimeManagement.Models
{
	public class DayWorkedTime : INotifyPropertyChanged
	{
        public DateTime Date { get; set; }
        public double Seconds { get; set; }


		#region ShortDate
		public string ShortDate
		{
			get { return _shortDate; }
			set
			{
				_shortDate = value;
				OnPropertyChanged(nameof(ShortDate));
			}
		}
		private string _shortDate;
		#endregion

		#region Time
		public string Time
		{
			get { return _time; }
			set
			{
				_time = value;
				OnPropertyChanged(nameof(Time));
			}
		}
		private string _time;
		#endregion


		public DayWorkedTime(DateTime date)
		{
			Date = date;
			Seconds = 0;
			Time = "-";
			ShortDate = "-";
		}


		public void AddTime(double seconds)
		{
			Seconds += seconds;
			Time = TaskInfo.SecToStrTime(Seconds);
			ShortDate = Date.ToString().Substring(0, 10).Remove(6, 2);
		}


		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged([CallerMemberName] string prop = "")
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(prop));
		}
	}
}
