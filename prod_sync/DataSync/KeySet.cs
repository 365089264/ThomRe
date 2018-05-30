using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luna.DataSync.Core
{
    public class KeySet
    {
        private readonly Dictionary<string, Tuple<DbColumnType, object>> keyNameTypeValueSet = new Dictionary<string, Tuple<DbColumnType, object>>();

        public IEnumerable<Tuple<string, DbColumnType, object>> Keys
        {
            get
            {
                foreach (var kvp in keyNameTypeValueSet)
                    yield return Tuple.Create(
                        kvp.Key, // Key name
                        kvp.Value.Item1, // Key type
                        kvp.Value.Item2); // Key value
            }
        }

        public KeySet Add(string keyName, DbColumnType keyType, object keyValue)
        {
            if (string.IsNullOrEmpty(keyName))
                throw new ArgumentNullException(string.Format("The key name (key type: {0}) cannot be null or empty", keyType));

            if (keyType == null)
                throw new ArgumentNullException(string.Format("The key type (key name: {0}) cannot be null", keyName));

            if (keyValue == null)
                throw new ArgumentNullException(string.Format("The key value (key name: {0}) cannot be null", keyName));

            if (keyNameTypeValueSet.ContainsKey(keyName))
                throw new Exception(string.Format("The key ({0}) already exists", keyName));

            keyNameTypeValueSet.Add(keyName, Tuple.Create(keyType, keyValue));
            return this;
        }

        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(this, obj))
                return true;

            KeySet other = obj as KeySet;
            if (other == null)
                return false;

            if (this.keyNameTypeValueSet.Count != other.keyNameTypeValueSet.Count)
                return false;

            
            foreach (var kvp in this.keyNameTypeValueSet)
            {
                if (!other.keyNameTypeValueSet.ContainsKey(kvp.Key))
                    return false;

                var thisKeyTypeValueTuple = kvp.Value;
                var otherKeyTypeValueTuple = other.keyNameTypeValueSet[kvp.Key];

                if (!(thisKeyTypeValueTuple.Item1 == otherKeyTypeValueTuple.Item1
                    && GetStringValue(thisKeyTypeValueTuple.Item1, thisKeyTypeValueTuple.Item2) == GetStringValue(otherKeyTypeValueTuple.Item1, otherKeyTypeValueTuple.Item2)))
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hash = 17;

            foreach (var kvp in keyNameTypeValueSet)
            {
                hash = hash * 23 + Tuple.Create(kvp.Key, kvp.Value.Item1, GetStringValue(kvp.Value.Item1, kvp.Value.Item2)).GetHashCode();
            }

            return hash;
        }

        public override string ToString()
        {
            return string.Join(
                ",",
                Keys.Select(
                    key => string.Format("Key Name: {0} | Key Type: {1} | Key Value: {2}", key.Item1, key.Item2, key.Item3)).ToArray());
        }

        private string GetStringValue(DbColumnType type, object value)
        {
            if (value.GetType() == typeof(byte[]) && (type == DbColumnType.CHAR || type == DbColumnType.VARCHAR))
            {
                return ((byte[])value).ToStringBySybaseStandard();
            }

            if (type == DbColumnType.DATETIME && value is DateTime)
            {
                return ((DateTime)value).ToString("yyyy-MM-dd hh:mm:ss tt");
            }

            return value.ToString();
        }
    }
}
