using System;

namespace YouTrackSharp.Infrastructure
{
	public abstract class ManagementBase
	{
		public IConnection Connection { get; private set; }

		protected ManagementBase(IConnection connection)
		{
			if (connection == null)
			{
				throw new ArgumentNullException("connection");
			}

			Connection = connection;
		}
	}
}