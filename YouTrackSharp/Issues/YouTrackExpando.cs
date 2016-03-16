using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

using log4net;

using Newtonsoft.Json;

namespace YouTrackSharp.Issues
{
	public abstract class YouTrackExpando : DynamicObject
	{
		#region Static Fields

		private static readonly YouTrackExpandoNoValue _noValue = new YouTrackExpandoNoValue();

		#endregion

		#region Fields

		private List<Comment> _comments;

		private List<Field> _fields;

		private ILog _log = LogManager.GetLogger(typeof(YouTrackExpando));

		#endregion

		#region Public Properties

		[JsonProperty(PropertyName = "comment")]
		public List<Comment> Comments
		{
			get
			{
				return _comments ?? (_comments = new List<Comment>());
			}
			set
			{
				_comments = value;
			}
		}

		[JsonProperty(PropertyName = "field")]
		public List<Field> Fields
		{
			get
			{
				return _fields ?? (_fields = new List<Field>());
			}
			set
			{
				_fields = value;
			}
		}

		public static YouTrackExpandoNoValue NoValue
		{
			get
			{
				return _noValue;
			}
		}

		#endregion

		#region Public Methods and Operators

		public virtual ExpandoObject ToExpandoObject()
		{
			IDictionary<string, object> expando = new ExpandoObject();

			foreach (var field in Fields)
			{
				if (String.Compare(field.name, "ProjectShortName", StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					expando.Add("project", field.value);
				}
				else if (String.Compare(field.name, "permittedGroup", StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					expando.Add("permittedGroup", field.value);
				}
				else
				{
					expando.Add(field.name.ToLower(), field.value);
				}
			}
			return (ExpandoObject)expando;
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			_log.DebugFormat("TryGetMember called for {0} ...", binder.Name);

			if (Fields.Any(f => f.name.Equals(binder.Name, StringComparison.OrdinalIgnoreCase)))
			{
				result = Fields.Single(f => f.name.Equals(binder.Name, StringComparison.OrdinalIgnoreCase)).value;
				_log.DebugFormat("Found matching Field in fields list with value {0} ({1})", result, result != null ? result.GetType().FullName : "null");
				return true;
			}

			var adjustedFieldName = AdjustBinderName(binder.Name);

			if (Fields.Any(f => f.name.Equals(adjustedFieldName, StringComparison.OrdinalIgnoreCase)))
			{
				result = Fields.Single(f => f.name.Equals(adjustedFieldName, StringComparison.OrdinalIgnoreCase)).value;
				_log.DebugFormat(
					"Found matching field after adjusting binder name to '{0}' with value = {1} ({2})",
					adjustedFieldName,
					result,
					result != null ? result.GetType().FullName : "null");
				return true;
			}

			// we'll be lenient here and just return default values for non-existent members
			result = NoValue;
			_log.DebugFormat("Binder name {0} was not found in Fields list, returning default value {1}({2}) ...", binder.Name, result, result.GetType());
			return true;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			_log.DebugFormat("Attempting to set member with binder name = {0}", binder.Name);
			if (Fields.Any(f => f.name.Equals(binder.Name, StringComparison.OrdinalIgnoreCase)))
			{
				_log.Debug("Found field with name matching binder; setting value.");
				Fields.Single(f => f.name.Equals(binder.Name, StringComparison.OrdinalIgnoreCase)).value = value;
			}
			else if (Fields.Any(f => f.name.Equals(AdjustBinderName(binder.Name), StringComparison.OrdinalIgnoreCase)))
			{
				var adjustBinderName = AdjustBinderName(binder.Name);
				_log.DebugFormat("Found field with name matching adjusted binder name '{0}'; setting value.", adjustBinderName);
				Fields.Single(f => f.name.Equals(adjustBinderName, StringComparison.OrdinalIgnoreCase)).value = value;
			}
			else
			{
				_log.DebugFormat(
					"Failed to find Field matching binder name or adjusted binder name. Adding new field with name = adjusted binder name ({0})",
					AdjustBinderName(binder.Name));
				Fields.Add(new Field { name = AdjustBinderName(binder.Name), value = value });
			}
			return true;
		}

		#endregion

		#region Methods

		private string AdjustBinderName(string binderName)
		{
			if (string.IsNullOrEmpty(binderName))
			{
				return binderName;
			}

			var adjusted = binderName.Replace('_', ' '); // addresses field names with spaces

			return adjusted;
		}

		#endregion

		public sealed class YouTrackExpandoNoValue
		{
			#region Constructors and Destructors

			internal YouTrackExpandoNoValue()
			{
			}

			#endregion

			public override string ToString()
			{
				return "NO-VALUE";
			}
		}
	}
}