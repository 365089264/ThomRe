namespace VAV.Scheduler.Jobs
{
    public class FinPrdFileSyncJob : FileSyncBaseJob
    {
        #region overrides of FileSyncBaseJob
        public override string ConfigFilePath
        {
            get { return @"config\Fin-prd-file-sync.xml"; }
        }
        #endregion



        #region Overrides of LunaJobObject
        public override string JobType
        {
            get { return "Genius_FIN_PRD_RPT_DATA"; }
        }
        #endregion

    }
}