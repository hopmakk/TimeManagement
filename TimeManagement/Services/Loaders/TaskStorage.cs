using TimeManagement.Models;
using Newtonsoft.Json;
using System.IO;

namespace TimeManagement.Services.Loaders
{
	public class TaskStorage
	{
		public string TasksFilePath { get; private set; }
		public string ArchiveTasksFilePath { get; private set; }


		public TaskStorage()
		{
			string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TimeManagementApp");
			Directory.CreateDirectory(folderPath);  // Создаем папку, если её нет
			var dataPath = Path.Combine(folderPath, "Data");
			Directory.CreateDirectory(dataPath);  // Создаем папку, если её нет
			TasksFilePath = Path.Combine(dataPath, "tasks.json");
			ArchiveTasksFilePath = Path.Combine(dataPath, "archiveTasks.json");
		}


		public void SaveTasks(List<TaskInfo> tasks)
		{
			var jsonData = JsonConvert.SerializeObject(tasks, Formatting.Indented);
			File.WriteAllText(TasksFilePath, jsonData);
		}


		public List<TaskInfo> LoadTasks()
		{
			if (File.Exists(TasksFilePath))
			{
				var jsonData = File.ReadAllText(TasksFilePath);
				var tasks = JsonConvert.DeserializeObject<List<TaskInfo>>(jsonData);
				return tasks ?? new List<TaskInfo>();
			}
			return new List<TaskInfo>();
		}


		public void SaveTasksToArchive(List<TaskInfo> tasks)
		{
			var jsonData = JsonConvert.SerializeObject(tasks, Formatting.Indented);
			File.WriteAllText(ArchiveTasksFilePath, jsonData);
		}


		public List<TaskInfo> LoadTasksFromArchive()
		{
			if (File.Exists(ArchiveTasksFilePath))
			{
				var jsonData = File.ReadAllText(ArchiveTasksFilePath);
				var tasks = JsonConvert.DeserializeObject<List<TaskInfo>>(jsonData);
				return tasks ?? new List<TaskInfo>();
			}
			return new List<TaskInfo>();
		}
	}
}
