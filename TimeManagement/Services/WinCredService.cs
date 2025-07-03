using CredentialManagement;

namespace TimeManagement.Services
{
	public class WinCredService
	{
		private const string _target = "timeTrackerAppToken";


		public string GetYoutrackToken()
		{
			var cm = new Credential { Target = _target };

			if (!cm.Load())
				return null;

			return cm.Password;
		}


		public bool SaveYoutrackToken(string token)
		{
			return new Credential
			{
				Target = _target,
				Username = "",
				Password = token,
				PersistanceType = PersistanceType.LocalComputer,
			}.Save();
		}


		public bool ClearYoutrackToken()
		{
			return new Credential { Target = _target }.Delete();
		}
	}
}
