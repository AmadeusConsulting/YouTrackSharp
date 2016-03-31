namespace YouTrackSharp.Admin
{
	public class CurrentUserInfo
	{
		public string FilterFolder { get; set; }
		public string LastCreatedProject { get; set; }
		public string Login { get; set; }
		public string Email { get; set; }
		public string FullName { get; set; }
		public string Avatar { get; set; }
		public bool Guest { get; set; }
	}
}