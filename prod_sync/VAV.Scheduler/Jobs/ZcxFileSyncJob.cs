namespace VAV.Scheduler.Jobs
{
    public class ZcxFileSyncJob : FileSyncBaseJob
    {
        #region overrides of FileSyncBaseJob
        public override string ConfigFilePath
        {
            get { return @"config\ZCX-rate-file-sync.xml"; }
        }
        #endregion



        #region Overrides of LunaJobObject
        public override string JobType
        {
            get { return "Zcx_RATE_REP_DATA"; }
        }
        #endregion


    }
}