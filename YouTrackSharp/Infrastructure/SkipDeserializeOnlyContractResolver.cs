using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace YouTrackSharp.Infrastructure
{
	public class SkipDeserializeOnlyContractResolver : DefaultContractResolver
	{
		protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{
			var properties = base.CreateProperties(type, memberSerialization);


			// return only properties that are not marked to be for deserialization only
			return (
				from prop in properties 
				let deserializeOnlyAttributes = prop.AttributeProvider.GetAttributes(typeof(JsonDeserializeOnlyAttribute), true) 
				where !deserializeOnlyAttributes.Any() 
				select prop).ToList();
		}
	}
}
