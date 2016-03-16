using System.Collections.Generic;

namespace YouTrackSharp.Import
{
	public class ImportedItem
	{
		private List<ImportError> _errors;

		public bool Imported { get; set; }

		public string Id { get; set; }

		public bool Empty { get; set; }

		public List<ImportError> Errors
		{
			get
			{
				return _errors ?? (_errors = new List<ImportError>());
			}
			set
			{
				_errors = value;
			}
		}
	}
}