using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luna.DataSync.Setting.Entity
{
    public class FtpSyncTableMapping
    {
        private List<ColumnMapping> _columnMappings = new List<ColumnMapping>();

		public string Source { get; private set; }
		public string Destination { get; private set; }
		public string Filter { get; private set; }


        private string _isKeepObsoleteDestinationData;
        public bool IsKeepObsoleteDestinationData
        {
            get { return bool.Parse(_isKeepObsoleteDestinationData); }
        }

        private bool _isContainFile;
        public bool IsContainFile{
            get { return _isContainFile; }
            set { _isContainFile = value; }
        }

		public IEnumerable<ColumnMapping> ColumnMappings 
		{
			get { return _columnMappings; }
		}

        public FtpSyncTableMapping(string source, string destination, string filter, string isKeepObsoleteDestinationData)
		{
			this.Source = source;
			this.Destination = destination;
			this.Filter = filter;
            this._isKeepObsoleteDestinationData = isKeepObsoleteDestinationData;
		}

        public string this[string sourceColumnName]
        {
            get
            {
                string destColumnName = _columnMappings.First(columnMapping => string.Compare(columnMapping.Source, sourceColumnName, true) == 0).Destination;

                return destColumnName;
            }
        }

		public void Add(ColumnMapping columnMapping)
		{
            if (_columnMappings.Any(mapping => string.Compare(mapping.Source, columnMapping.Source, true) == 0 || string.Compare(mapping.Destination, columnMapping.Destination, true) == 0))
				throw new Exception(
                    string.Format(
                    "There is already a mapping related to the source column ({0}) or destination column ({1}) in the table mapping (source table: {2})",
                    columnMapping.Source,
                    columnMapping.Destination,
                    Source));

			_columnMappings.Add(columnMapping);
		}

        public IEnumerable<string> GetKeyColumnNames()
        {
            return ColumnMappings.Where(columnMapping => columnMapping.IsKey).Select(columnMapping => columnMapping.Destination);
        }

        public string GetImmutableKeyColumnName()
        {
            IEnumerable<string> immutableKeyNames = ColumnMappings
                .Where(columnMapping => columnMapping.IsKey && columnMapping.IsImmutableKey)
                .Select(columnMapping => columnMapping.Destination);

            if (immutableKeyNames.Count() > 1)
                throw new Exception(string.Format("The table ({0}) contans multiple immutable keys", Destination));

            string immutableKeyName = immutableKeyNames.FirstOrDefault();

            if (string.IsNullOrEmpty(immutableKeyName))
                throw new Exception(string.Format("The table ({0}) should specify one immutable key", Destination));

            return immutableKeyName;
        }
    }
}
