#region License

//   Copyright 2010 John Sheehan
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;
using RestSharp.Extensions;
using System.Globalization;
using RestSharp.Deserializers;
using RestSharp;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Dynamic;
using YouTrackSharp.Issues;

namespace YouTrackSharp.Infrastructure
{
    public class NewtonsoftJsonDeserializer : IDeserializer
    {
        public string RootElement { get; set; }
        public string Namespace { get; set; }
        public string DateFormat { get; set; }
        public CultureInfo Culture { get; set; }

        public NewtonsoftJsonDeserializer()
        {
            Culture = CultureInfo.InvariantCulture;
        }

        public T Deserialize<T>(IRestResponse response)
        {
            if (typeof(T) == typeof(IDynamicMetaObjectProvider))
            {
                return JsonConvert.DeserializeObject<dynamic>(response.Content);
            } 
            
            var target = Activator.CreateInstance<T>();

            if (target is IList)
            {
                var objType = target.GetType();

                if (RootElement.HasValue())
                {
                    var root = FindRoot(response.Content);
                    target = (T) BuildList(objType, root.Children());
                }
                else
                {
                    JArray json = JArray.Parse(response.Content);
                    target = (T) BuildList(objType, json.Root.Children());
                }
            }
            else if (target is IDictionary)
            {
                var root = FindRoot(response.Content);
                target = (T) BuildDictionary(target.GetType(), root.Children());
            }
            else if (target is IDictionary<string, object>)
            {
                var root = FindRoot(response.Content);
                target = (T) PopulateDictionaryType((IDictionary<string, object>) target, root.Children());
            }
            
            else
            {
                var root = FindRoot(response.Content);
                Map(target, root);
            }

            return target;
        }

        private JToken FindRoot(string content)
        {
            JObject json = JObject.Parse(content);
            JToken root = json.Root;

            if (RootElement.HasValue())
                root = json.SelectToken(RootElement);

            return root;
        }

        private void Map(object x, JToken json)
        {
            var objType = x.GetType();

            var props = objType.GetProperties().Where(p => p.CanWrite).ToList();

            foreach (var prop in props)
            {
                var type = prop.PropertyType;
                var jsonPropertyAttr =
                    prop.GetCustomAttributes(typeof (JsonPropertyAttribute), true)
                        .Cast<JsonPropertyAttribute>()
                        .FirstOrDefault();
                var name = jsonPropertyAttr != null ? jsonPropertyAttr.PropertyName : prop.Name;

                var actualName = name.GetNameVariants(Culture).FirstOrDefault(n => json[n] != null);
                var value = actualName != null ? json[actualName] : null;

                if (value == null || value.Type == JTokenType.Null)
                {
                    continue;
                }

                // check for nullable and extract underlying type
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>))
                {
                    type = type.GetGenericArguments()[0];
                }

                if (type.IsPrimitive)
                {
                    // no primitives can contain quotes so we can safely remove them
                    // allows converting a json value like {"index": "1"} to an int
                    var tmpVal = value.AsString().Replace("\"", string.Empty);
                    prop.SetValue(x, tmpVal.ChangeType(type, Culture), null);
                }
                else if (type.IsEnum)
                {
                    var converted = type.FindEnumValue(value.AsString(), Culture);
                    prop.SetValue(x, converted, null);
                }
                else if (type == typeof (Uri))
                {
                    string raw = value.AsString();
                    var uri = new Uri(raw, UriKind.RelativeOrAbsolute);
                    prop.SetValue(x, uri, null);
                }
                else if (type == typeof (string))
                {
                    string raw = value.AsString();
                    prop.SetValue(x, raw, null);
                }
                else if (type == typeof (DateTime) || type == typeof (DateTimeOffset))
                {
                    DateTime dt;
                    if (DateFormat.HasValue())
                    {
                        var clean = value.AsString();
                        dt = DateTime.ParseExact(clean, DateFormat, Culture);
                    }
                    else if (value.Type == JTokenType.Date)
                    {
                        dt = value.Value<DateTime>().ToUniversalTime();
                    }
                    else
                    {
                        Int64 dateInt = Int64.Parse(value.AsString())/1000;
                        dt = dateInt.ToString().ParseJsonDate(Culture);
                       
                        
                    }

                    if (type == typeof (DateTime))
                        prop.SetValue(x, dt, null);
                    else if (type == typeof (DateTimeOffset))
                        prop.SetValue(x, (DateTimeOffset) dt, null);
                }
                else if (type == typeof (Decimal))
                {
                    var dec = Decimal.Parse(value.AsString(Culture), Culture);
                    prop.SetValue(x, dec, null);
                }
                else if (type == typeof (Guid))
                {
                    string raw = value.AsString();
                    var guid = string.IsNullOrEmpty(raw) ? Guid.Empty : new Guid(raw);
                    prop.SetValue(x, guid, null);
                }
                else if (type.IsGenericType)
                {
                    var genericTypeDef = type.GetGenericTypeDefinition();
                    if (genericTypeDef == typeof (List<>))
                    {
                        var list = BuildList(type, value.Children());
                        prop.SetValue(x, list, null);
                    }
                    else if (genericTypeDef == typeof (Dictionary<,>))
                    {
                        var keyType = type.GetGenericArguments()[0];

                        // only supports Dict<string, T>()
                        if (keyType == typeof (string))
                        {
                            var dict = BuildDictionary(type, value.Children());
                            prop.SetValue(x, dict, null);
                        }
                    }
                    else
                    {
                        // nested property classes
                        var item = CreateAndMap(type, json[actualName]);
                        prop.SetValue(x, item, null);
                    }
                }
                else
                {
                    // nested property classes
                    var item = CreateAndMap(type, json[actualName]);
                
                    prop.SetValue(x, item, null);
                }
            }
        }

       
        private object CreateAndMap(Type type, JToken element)
        {
            object instance = null;

            if (type.IsGenericType)
            {
                var genericTypeDef = type.GetGenericTypeDefinition();
                if (genericTypeDef == typeof (Dictionary<,>))
                {
                    instance = BuildDictionary(type, element.Children());
                }
                else if (genericTypeDef == typeof (List<>))
                {
                    instance = BuildList(type, element.Children());
                }
                else if (type == typeof (string))
                {
                    instance = (string) element;
                }
                else
                {
                    instance = Activator.CreateInstance(type);
                    Map(instance, element);
                }
            }
            else if (type == typeof (string))
            {
                instance = (string) element;
            }
            else if (type == typeof (object))
            {
	            Type targetType = null;
                switch (element.Type)
                {
                    case JTokenType.String:
                        targetType = typeof (string);
                        break;
                    case JTokenType.Float:
                        targetType = typeof (float);
                        break;
                    case JTokenType.Date:
                        targetType = typeof (DateTime);
                        break;
                    case JTokenType.Integer:
                        targetType = typeof (int);
                        break;
                    case JTokenType.Boolean:
                        targetType = typeof (bool);
                        break;
                    case JTokenType.Uri:
                        targetType = typeof (Uri);
                        break;
					case JTokenType.Array:
		                instance = BuildList(typeof(ArrayList), element.Children());
		                break;
					case JTokenType.Object:
		                instance = BuildDictionary(typeof(Dictionary<string, object>), element.Children());
		                break;
                    default:
                       throw new NotImplementedException(string.Format("Target type of {0} is not implemented", element.Type));
                }

	            if (targetType != null)
	            {
		            instance = element.ToObject(targetType);
	            }
            }
            else
            {
                instance = Activator.CreateInstance(type);
                Map(instance, element);
            }
            return instance;
        }

        private IDictionary BuildDictionary(Type type, JEnumerable<JToken> elements)
        {
            var dict = (IDictionary) Activator.CreateInstance(type);
            var valueType = type.GetGenericArguments()[1];
            foreach (JProperty child in elements)
            {
                var key = child.Name;
                var item = CreateAndMap(valueType, child.Value);
                dict.Add(key, item);
            }

            return dict;
        }

        private IDictionary<string, object> PopulateDictionaryType(IDictionary<string, object> dict,
            JEnumerable<JToken> elements)
        {
            //TODO: Deal with child objects and array values

            foreach (JToken child in elements)
            {
                var jproperty = child as JProperty;
                if (jproperty == null)
                {
                    throw new FormatException(string.Format("Expected JSON property, instead got {0}", child.Type));
                }

                var unsupportedJTokenTypes = new[] {JTokenType.Object, JTokenType.Array};

                if (unsupportedJTokenTypes.Contains(jproperty.Value.Type))
                {
                    throw new NotImplementedException(
                        string.Format("Deserialization of JTokenType {0} is not yet implemented!", jproperty.Value.Type));
                }

                Type valueType;
                switch (jproperty.Value.Type)
                {
                    case JTokenType.Integer:
                        valueType = typeof (int);
                        break;
                    case JTokenType.Boolean:
                        valueType = typeof (bool);
                        break;
                    case JTokenType.Float:
                        valueType = typeof (double);
                        break;
                    case JTokenType.Uri:
                        valueType = typeof (Uri);
                        break;
                    case JTokenType.Date:
                        valueType = typeof (DateTime);
                        break;
                    default:
                        valueType = typeof (string);
                        break;
                }

                var key = jproperty.Name;
                dict.Add(key, jproperty.Value.ToObject(valueType));
            }

            return dict;
        }

        private IList BuildList(Type type, JEnumerable<JToken> elements)
        {
            var list = (IList) Activator.CreateInstance(type);
	        var itemType = type.IsGenericType ? type.GetGenericArguments()[0] : typeof(object);

            foreach (var element in elements)
            {
                if (itemType.IsPrimitive)
                {
                    var value = element as JValue;
                    if (value != null)
                    {
                        list.Add(value.Value.ChangeType(itemType, Culture));
                    }
                }
                else if (itemType == typeof (string))
                {
                    list.Add(element.AsString());
                }
                else
                {
                    var item = CreateAndMap(itemType, element);
                    list.Add(item);
                }
            }
            return list;
        }
    }
}