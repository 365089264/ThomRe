namespace VAV.Scheduler.Jobs
{
    public class CfpDiscFileSyncJob : FileSyncBaseJob
    {
        #region overrides of FileSyncBaseJob
        public override string ConfigFilePath
        {
            get { return @"config\Cfp-disc-file-sync.xml"; }
        }
        #endregion



        #region Overrides of LunaJobObject
        public override string JobType
        {
            get { return "Genius_DISC_ACCE_CFP_DATA"; }
        }
        #endregion

    }
}