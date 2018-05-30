using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luna.DataSync.Setting
{
	public class ColumnMapping
	{
		public string Source { get; private set; }
        public string SourceTableAlias { get; private set; }
		public string Destination { get; private set; }
        public string Value { get; private set; }
        public bool IsKey 
        { 
            get { return bool.Parse(_isKey); }
        }

        public bool IsImmutableKey
        {
            get { return bool.Parse(_isImmutableKey); }
        }

        private string _isKey;
        private string _isImmutableKey;

        public ColumnMapping(
            string source, string sourceTableAlias, string destination, string value, string isKey, string isImmutableKey)
		{
			this.Source = source;
            this.SourceTableAlias = sourceTableAlias;
			this.Destination = destination;
            this.Value = value;
            this._isKey = isKey;
            this._isImmutableKey = isImmutableKey;
		}
	}
}
