using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luna.DataSync.Setting
{
	public class TableMapping
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

        #region file sync setting
        
        private bool _isSyncFile;
        public bool IsSyncFile
        {
            get { return _isSyncFile; }
            set { _isSyncFile = value; }
        }

        private string _pathRoot;
        public string PathRoot
        {
            get { return _pathRoot; }
            set { _pathRoot = value; }
        }

        private string _pathColumn;
        public string PathColumn
        {
            get { return _pathColumn; }
            set { _pathColumn = value; }
        }

        private string _extColumn;
        public string ExtColumn
        {
            get { return _extColumn; }
            set { _extColumn = value; }
        }

        #endregion

        public IEnumerable<ColumnMapping> ColumnMappings 
		{
			get { return _columnMappings; }
            set { _columnMappings = value.ToList(); }
		}

        public TableMapping(string source, string destination, string filter, string isKeepObsoleteDestinationData)
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
