using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YouTrackSharp.Infrastructure
{
	/// <summary>
	/// When applied to a property this attribute signifies that the property should 
	/// not be serialized to Json; however this property should be deserialized from JSON, when available.
	/// </summary>
	/// <seealso cref="System.Attribute" />
	[AttributeUsage(AttributeTargets.Property)]
	public class JsonDeserializeOnlyAttribute : Attribute
	{
	}
}
