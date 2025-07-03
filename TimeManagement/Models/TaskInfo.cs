using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Threading;
using TimeManagement.Services;
using YouTrackSharp.Issues;
using YouTrackSharp.TimeTracking;

namespace TimeManagement.Models
{
    public class TaskInfo : INotifyPropertyChanged
    {
        public Guid Id { get; set; }
        public string TaskId { get; set; }
        public string Name { get; set; }
        public TaskStatus Status { get; set; }
		public DateTime CreateDate { get; set; }

        public ObservableCollection<WorkActiveEvent> ActiveLog { get; set; } // тут находится всё незатреканное время
        public ObservableCollection<DayWorkedTime> DaysWorkedTime { get; set; } // а тут все затреканное

        private DispatcherTimer _timer; // актуализирует затраченное время 
        private const int ACTUALIZE_TIME_IN_SEC = 1;

		private AppCenter _appCenter = AppCenter.GetInstance();


		public bool IsSynchr
		{
			get { return _isSynchr; }
			set
			{
				//if (_isSynchr == value) return;
				_isSynchr = value;

				if (_isSynchr)
					SynchrStatus = "✓";
				else
					SynchrStatus = "*";
			}
		}
		private bool _isSynchr;

		#region StandartWorkType
		public WorkType StandartWorkType
		{
			get { return _standartWorkType; }
			set
			{
				_standartWorkType = value;
				OnPropertyChanged(nameof(StandartWorkType));
			}
		}
		private WorkType _standartWorkType;
		#endregion

		#region SynchrStatus
		public string SynchrStatus
		{
			get { return _synchrStatus; }
			set
			{
				_synchrStatus = value;
				OnPropertyChanged(nameof(SynchrStatus));
			}
		}
		private string _synchrStatus;
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

		#region TrackedTime
		public string TrackedTime
		{
            get { return _trackedTime; }
            set
            {
				_trackedTime = value;
                OnPropertyChanged(nameof(TrackedTime));
            }
        }
        private string _trackedTime;
		#endregion

		#region AllTimeSpent
		public string AllTimeSpent
		{
			get { return _allTimeSpent; }
			set
			{
				_allTimeSpent = value;
				OnPropertyChanged(nameof(AllTimeSpent));
			}
		}
		private string _allTimeSpent;
		#endregion

		#region IsActive
		public bool IsActive
		{
			get { return _isActive; }
			set
			{
				_isActive = value;
				OnPropertyChanged(nameof(IsActive));
			}
		}
		private bool _isActive;
		#endregion


		public TaskInfo()
		{
			_timer = new DispatcherTimer();
			_timer.Interval = new TimeSpan(0, 0, ACTUALIZE_TIME_IN_SEC);
			_timer.Tick += Timer_Tick;
		}

		public TaskInfo(Issue issue, List<WorkItem> workItems)
		{
			Id = Guid.NewGuid();
			Name = issue.Summary;
			TaskId = issue.Id;
			Status = TaskStatus.New;
			CreateDate = DateTime.Now;
			UntrackedTime = "0м";
			TrackedTime = "0м";
			AllTimeSpent = "0м";
			SynchrStatus = "✓";
			IsActive = false;
			StandartWorkType = new WorkType()
			{
				Id = "", Name = "Выберите тип"
			};
			ActiveLog = new ObservableCollection<WorkActiveEvent>();
			DaysWorkedTime = new ObservableCollection<DayWorkedTime>();

			_timer = new DispatcherTimer();
			_timer.Interval = new TimeSpan(0, 0, ACTUALIZE_TIME_IN_SEC);
			_timer.Tick += Timer_Tick;

			SetActualDaysWorkedTime(workItems);
		}


		// Установить актуальные дни, в которые трекалось время
		public void SetActualDaysWorkedTime(List<WorkItem> workItems)
		{
			DaysWorkedTime.Clear();

			foreach (WorkItem wi in workItems)
			{
				if (wi.Date != null)
				{
					// смотрим есть ли в списке день, в котором мы трекали время
					var thisDay = DaysWorkedTime.Where(d => d.Date.Date == wi.Date.Value.Date).FirstOrDefault();

					// если нет, создаем
					if (thisDay is null)
					{
						thisDay = new DayWorkedTime(wi.Date.Value.Date);
						DaysWorkedTime.Add(thisDay);
					}

					thisDay.AddTime(wi.Duration.TotalSeconds);
				}
			}
			UpdateShowingTime();
		}


		private void Timer_Tick(object? sender, EventArgs e)
        {
			UpdateShowingTime();
		}


		public void UpdateShowingTime()
		{
			var allUntrackedTime = GetAllUntrackedTime();
			var allTrackedTime = GetAllTrackedTime();

			UntrackedTime = SecToStrTime(allUntrackedTime);
			TrackedTime = SecToStrTime(allTrackedTime);
			AllTimeSpent = SecToStrTime(allUntrackedTime + allTrackedTime);

			if (allUntrackedTime == 0)
				IsSynchr = true;
			else 
				IsSynchr = false;
		}


		// выполняем задачу
		public void Start(WorkType workType)
		{
			if (IsActive) return;
			IsActive = true;

			_timer.Start(); // каждую секунду будет обновлять отображаемое время

			var newEvent = new WorkActiveEvent(workType);
			newEvent.Start();
			ActiveLog.Add(newEvent);
		}


		// останавливаем выполнение
		public void Stop()
		{
			if (!IsActive) return;
			IsActive = false;

			_timer.Stop();

			var closingEvent = ActiveLog[^1];
			closingEvent.StopIfActive();
			
			// если ивент длился меньше минуты, удаляем его
			if (closingEvent.GetDurationSec() < 60)
			{
				var message = "Прошло слишком мало времени для того, чтобы его учитывать. Попробуйте работать над задачей больше 1 минуты.";
				_appCenter.NotificationService.ShowNotification(NotificationType.Hint, "Время не засчитано", message);
				ActiveLog.Remove(closingEvent);
				UpdateShowingTime();
			}
		}


		public double GetUntrackedTimeInDay(DateTime date)
		{
			var logsInDate = ActiveLog.Where(l => l.StartTime.Date == date.Date).ToList();
			var timeSec = 0.0;

			foreach (var log in logsInDate)
				timeSec += log.GetDurationSec();

			return timeSec;
		}


		public double GetUntrackedTimeInDay(DateTime date, WorkType workType)
		{
			var logsInDate = ActiveLog.Where(l => l.StartTime.Date == date.Date && l.WorkType.Id == workType.Id).ToList();
			var timeSec = 0.0;

			foreach (var log in logsInDate)
				timeSec += log.GetDurationSec();

			return timeSec;
		}


		public double GetAllUntrackedTime()
		{
			var timeSec = 0.0;

			foreach (var log in ActiveLog)
			{
				timeSec += log.GetDurationSec();
			}
			return timeSec;
		}


		public double GetAllTrackedTime()
		{
			var timeSec = 0.0;

			foreach (var day in DaysWorkedTime)
			{
				timeSec += day.Seconds;
			}
			return timeSec;
		}


		public double GetTrackedTimeInDate(DateTime date)
		{
			return DaysWorkedTime.Where(day => day.Date == date).FirstOrDefault().Seconds;
		}


		// Действия после успешного трека времени
		public async Task<bool> AfterTrackTimeActions(DateTime date, WorkType workType)
		{
			var workItems = await _appCenter.YTApi.GetWorkTimesByIssueIdAsync(TaskId);
			if (workItems != null)
			{
				// установить актуальное затреканное время по задаче
				SetActualDaysWorkedTime(workItems);

				// очистить ActiveLog для этого дня
				var removingLogs = new List<WorkActiveEvent>();
				foreach (var log in ActiveLog)
					if (log.StartTime.Date == date && log.WorkType.Id == workType.Id)
						removingLogs.Add(log);

				foreach (var log in removingLogs)
					ActiveLog.Remove(log);

				// обновить показываемое время 
				UpdateShowingTime();
				return true;
			}
			return false;
		}


		public static string SecToStrTime(double allSeconds)
		{
			var weeks = (int)Math.Floor(allSeconds / (3600 * 8 * 5)); // 5 дней в неделю
			allSeconds %= (3600 * 8 * 5);
			var days = (int)Math.Floor(allSeconds / (3600 * 8)); // 8 рабочих часов в день
			allSeconds %= (3600 * 8);
			var hours = (int)Math.Floor(allSeconds / 3600);
			allSeconds %= 3600;
			var minutes = (int)Math.Floor(allSeconds / 60);
			allSeconds %= 60;
			var seconds = (int)Math.Floor(allSeconds);

			StringBuilder str = new StringBuilder();
			if (weeks != 0)
				str.Append(weeks + "н ");
			if (days != 0)
				str.Append(days + "д ");
			if (hours != 0)
				str.Append(hours + "ч ");
			if (minutes != 0 && weeks == 0)
				str.Append(minutes + "м ");

			if (weeks == 0 && days == 0 && hours == 0)
				str.Append(seconds + "с");

			/* двочиная схема, по которой отображается время
			 * н д ч м с
			 * 1 1 1 0 0
			 * 0 1 1 1 0
			 * 0 0 1 1 0
			 * 0 0 0 1 1
			 * 0 0 0 0 1
			*/

			return str.ToString().Trim(' ');
		}


		public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}
