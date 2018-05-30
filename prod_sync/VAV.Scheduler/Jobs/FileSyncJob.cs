namespace VAV.Scheduler.Jobs
{
    public class FileSyncJob : FileSyncBaseJob
    {

        #region overrides of FileSyncBaseJob
        public override string ConfigFilePath
        {
            get { return @"config\File-data-sync.xml"; }
        }
        #endregion
        


        #region Overrides of LunaJobObject
        public override string JobType
        {
            get { return "File_SYNC"; }
        }
        #endregion

    }
}
