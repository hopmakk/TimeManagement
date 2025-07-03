using Newtonsoft.Json;
using System.IO;
using TimeManagement.Models;

namespace TimeManagement.Services.Loaders
{
	public class ConfigLoader
    {
		public string FilePath { get; private set; }


		public ConfigLoader()
		{
			string folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TimeManagementApp");
			Directory.CreateDirectory(folderPath);  // Создаем папку, если её нет
			var dataPath = Path.Combine(folderPath, "Config");
			Directory.CreateDirectory(dataPath);  // Создаем папку, если её нет
			FilePath = Path.Combine(dataPath, "AppConfig.json");
		}


		public void SaveConfig(ConfigData config)
		{
			var jsonData = JsonConvert.SerializeObject(config, Formatting.Indented);
			File.WriteAllText(FilePath, jsonData);
		}


		public ConfigData LoadConfig()
		{
			if (File.Exists(FilePath))
			{
				var jsonData = File.ReadAllText(FilePath);
				var config = JsonConvert.DeserializeObject<ConfigData>(jsonData);
				if (config != null)
					return config;
			}
			return new ConfigData();
		}
	}
}
