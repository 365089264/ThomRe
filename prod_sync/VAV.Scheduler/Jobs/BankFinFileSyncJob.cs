namespace VAV.Scheduler.Jobs
{
    public class BankFinFileSyncJob : FileSyncBaseJob
    {

        #region overrides of FileSyncBaseJob
        public override string ConfigFilePath
        {
            get { return @"config\Bank-fin-file-sync.xml"; }
        }
        #endregion



        #region Overrides of LunaJobObject
        public override string JobType
        {
            get { return "Genius_BANK_FIN_PRD_PROSP_DATA"; }
        }
        #endregion

    }
}