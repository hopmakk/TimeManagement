using YouTrackSharp.Issues;
using YouTrackSharp.Projects;
using YouTrackSharp.TimeTracking;
using YouTrackSharp;
using TimeManagement.Services;
using YouTrackSharp.Users;
using TimeManagement.Models;
using System.Diagnostics;
using YouTrackSharp.Management;

namespace TimeManagement.YouTrack
{
    public class YTApi
    {
		private AppCenter _appCenter = AppCenter.GetInstance();

		private readonly IIssuesService _issuesService;
		private readonly IProjectsService _projectsService;
		private readonly ITimeTrackingService _timeTrackingService;
		private readonly UserService _userService;
        private readonly IUserManagementService _userManagementService;
		private bool _tokenIsExist = false;


		public YTApi(string baseUrl)
		{
			// Создаем соединение с YouTrack
			var token = _appCenter.WinCredService.GetYoutrackToken();
			if (token == null || token == "")
			{
				_tokenIsExist = false;
				ShowCantFindTokenNotification();
				return;
			}
			_tokenIsExist = true;

			var connection = new BearerTokenConnection(baseUrl, token);
			_issuesService = connection.CreateIssuesService();
			_projectsService = connection.CreateProjectsService();
			_timeTrackingService = connection.CreateTimeTrackingService();
            _userService = new UserService(connection);
            _userManagementService = connection.CreateUserManagementService();

        }


		public YTApi(string baseUrl, string token)
		{
			// Создаем соединение с YouTrack
			_tokenIsExist = true;
			var connection = new BearerTokenConnection(baseUrl, token);
			_issuesService = connection.CreateIssuesService();
			_projectsService = connection.CreateProjectsService();
			_timeTrackingService = connection.CreateTimeTrackingService();
			_userService = new UserService(connection);
		}


		public void ShowCantFindTokenNotification()
		{
			_appCenter.SettingsPage.ConfigData.TokenStatus = "Токен не найден";
			var message = "Мы не смоги найти токен в Windows Credential Manager.\n\nПерейдите в настройки и в разделе данных о приложении нажмите \"Обновить\".";
			_appCenter.NotificationService.ShowNotification(NotificationType.Warning, "Токен не найден", message);
		}


		public async Task<bool> CheckSendRequest()
		{
			var response = await GetProjectsAsync();

			if (response == null)
				return false;
			else
				return true;
		}


		public async Task<List<Project>> GetProjectsAsync()
		{
			try
			{
				// Получаем все проекты
				var projects = await _projectsService.GetAccessibleProjects();
				return projects.ToList();
			}
			catch (Exception ex)
			{
				// используется только на этапе настройки, где лучше не показывать уведомления
				//_appCenter.LogService.SaveLogError(ex, "Не удается отправить запрос в YT");
				return null;
			}
		}


		public async Task<List<Issue>> GetIssuesAsync()
		{
			if (!_tokenIsExist)
			{
				ShowCantFindTokenNotification();
				return null;
			}
			try
			{
				// Получаем все задачи
				var issues = await _issuesService.GetIssuesInProject(_appCenter.SettingsPage.ConfigData.ProjectName);
				return issues.ToList();
			}
			catch (Exception ex)
			{
				_appCenter.LogService.SaveLogError(ex, "Не удается получить задачи из YT");
				return null;
			}
		}


		public async Task<Issue> GetIssuesByIdAsync(string id)
		{
			if (!_tokenIsExist)
			{
				ShowCantFindTokenNotification();
				return null;
			}
			try
			{
				// Получаем все задачи
				var issue = await _issuesService.GetIssue(id);
				return issue;
			}
			catch (Exception ex)
			{
				_appCenter.LogService.SaveLogError(ex, "Не удается получить задачу из YT");
				return null;
			}
		}


		public async Task<List<WorkItem>> GetWorkTimesByIssueIdAsync(string id)
		{
			if (!_tokenIsExist)
			{
				ShowCantFindTokenNotification();
				return null;
			}
			try
			{
				var workItems = await _timeTrackingService.GetWorkItemsForIssue(id);
				var userLogin = (await _userService.GetCurrentUserInfo()).Login;
				return workItems.Where(wi => wi.Author.Login == userLogin).ToList();
			}
			catch (Exception ex)
			{
				_appCenter.LogService.SaveLogError(ex, "Не удается получить время в треке из YT");
				return null;
			}
		}


		public async Task UpdateWorkItemByIssueIdAsync(string issueId, string workItemId, WorkItem workItem)
		{
			if (!_tokenIsExist)
                ShowCantFindTokenNotification();
            try
			{
				var userInfo = await _userService.GetCurrentUserInfo();
                var result = await _userManagementService.GetUser(userInfo.Login);
				workItem.Author.Login = result.RingId;
                await _timeTrackingService.UpdateWorkItemForIssue(issueId, workItemId, workItem);
            }
			catch (Exception ex)
			{
				_appCenter.LogService.SaveLogError(ex, "Не удается изменить комментарий по задаче в YT");
			}
		}


		public async Task<List<WorkType>> GetWorkTypesInProject(string projectName)
		{
			if (!_tokenIsExist)
			{
				ShowCantFindTokenNotification();
				return null;
			}
			try
			{
				return (await _timeTrackingService.GetWorkTypesForProject(projectName)).ToList();
			}
			catch (Exception ex)
			{
				_appCenter.LogService.SaveLogError(ex, "Не удается получить типы работ в проекте");
				return null;
			}
		}


		public async Task<bool> TrackTimeForTask(DateTime date, string TaskId, int minutes, WorkType workType, DateTime createdTime, string comment)
		{
			if (!_tokenIsExist)
			{
				ShowCantFindTokenNotification();
				return false;
			}
			try
			{
				var projName = _appCenter.SettingsPage.ConfigData.ProjectName;

				var workItem = new WorkItem()
				{
					Duration = new TimeSpan(0, minutes, 0),
					WorkType = workType,
					Description = comment,
					Date = date.Date,
					Created = createdTime,
                };

				await _timeTrackingService.CreateWorkItemForIssue(TaskId, workItem);
				return true;
			}
			catch (Exception ex)
			{
				_appCenter.LogService.SaveLogError(ex, $"{TaskId} - Не удается списать время в YT");
				return false;
			}
		}


		// Открытие задачи в браузере
		public void OpenIssueInBrowser(string issueId)
		{
			string issueLink = $"{_appCenter.SettingsPage.ConfigData.BaseURL}/issue/{issueId}";
			try
			{
				// Открытие ссылки в браузере (для Windows)
				Process.Start(new ProcessStartInfo
				{
					FileName = issueLink,
					UseShellExecute = true
				});
			}
			catch (Exception ex)
			{
				_appCenter.LogService.SaveLogError(ex, $"Не удаётся открыть задачу в браузере");
			}
		}
	}
}

