using System;

namespace YouTrackSharp.Admin
{
	public class UserDetail
	{
		public string Login { get; set; }
		public string RingId { get; set; }
		public string FullName { get; set; }
		public string Email { get; set; }
		public DateTimeOffset LastAccess { get; set; }
		public string Jabber { get; set; }
		public string AvatarUrl { get; set; }
		public string GroupsUrl { get; set; }
		public string RolesUrl { get; set; }
	}
}