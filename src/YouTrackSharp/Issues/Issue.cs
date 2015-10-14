#region License

// Distributed under the BSD License
//   
// YouTrackSharp Copyright (c) 2010-2012, Hadi Hariri and Contributors
// All rights reserved.
//   
//  Redistribution and use in source and binary forms, with or without
//  modification, are permitted provided that the following conditions are met:
//      * Redistributions of source code must retain the above copyright
//         notice, this list of conditions and the following disclaimer.
//      * Redistributions in binary form must reproduce the above copyright
//         notice, this list of conditions and the following disclaimer in the
//         documentation and/or other materials provided with the distribution.
//      * Neither the name of Hadi Hariri nor the
//         names of its contributors may be used to endorse or promote products
//         derived from this software without specific prior written permission.
//   
//   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
//   "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED
//   TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A 
//   PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL 
//   <COPYRIGHTHOLDER> BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
//   SPECIAL,EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
//   LIMITED  TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
//   DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND  ON ANY
//   THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//   (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
//   THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//   

#endregion

using System;
using System.Collections.Generic;
using System.Dynamic;

namespace YouTrackSharp.Issues
{
    public abstract class YouTrackExpando : DynamicObject, IDictionary<string, object>
    {
        readonly IDictionary<string, object> _allFields = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

        public ExpandoObject ToExpandoObject()
        {
            IDictionary<string, object> expando = new ExpandoObject();


            foreach (dynamic field in _allFields)
            {
                if (String.Compare(field.Key, "ProjectShortName", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    expando.Add("project", field.Value);
                }
                else if (String.Compare(field.Key, "permittedGroup", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    expando.Add("permittedGroup", field.Value);
                }
                else
                {
                    expando.Add(field.Key.ToLower(), field.Value);
                }
            }
            return (ExpandoObject)expando;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_allFields.ContainsKey(binder.Name))
            {
                result = _allFields[binder.Name];
                return true;
            }
            return base.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (String.Compare(binder.Name, "field", StringComparison.OrdinalIgnoreCase) == 0)
            {
                foreach (var val in (IEnumerable<dynamic>)value)
                {
                    _allFields[val.name] = val.value;
                }
                return true;
            }
            _allFields[binder.Name] = value;
            return true;
        }

        public virtual void Add(string key, object value)
        {
            _allFields.Add(key, value);
        }

        public virtual bool ContainsKey(string key)
        {
            return _allFields.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get { return _allFields.Keys; }
        }

        public bool Remove(string key)
        {
            return _allFields.Remove(key);
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value)
        {
            return _allFields.TryGetValue(key, out value);
        }

        public ICollection<object> Values
        {
            get { return _allFields.Values; }
        }

        public object this[string key]
        {
            get
            {
                return _allFields.ContainsKey(key) ? _allFields[key] : null;
            }
            set
            {
                _allFields[key] = value;
            }
        }

        public virtual void Add(KeyValuePair<string, object> item)
        {
            _allFields.Add(item);
        }

        public virtual void Clear()
        {
            _allFields.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return _allFields.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            _allFields.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _allFields.Count; }
        }

        public bool IsReadOnly
        {
            get { return _allFields.IsReadOnly; }
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return _allFields.Remove(item);
        }

        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return _allFields.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _allFields.GetEnumerator();
        }
    }


    public class Issue : YouTrackExpando
    {
        public string Id
        {
            get { return base["id"] as string; }
            set { base["id"] = value; }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (binder.Name != null && binder.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
            {
                Id = value != null ? value.ToString() : null;
            }
            
            return base.TrySetMember(binder, value);
        }
    }
}