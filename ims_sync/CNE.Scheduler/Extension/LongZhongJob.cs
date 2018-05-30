using System.Text;

namespace CNE.Scheduler.Extension
{
    public class LongZhongJob
    {
        private StringBuilder _logMsg;
        public StringBuilder LogMsg
        {
            get { return _logMsg ?? (_logMsg = new StringBuilder()); }
        }
        public void Execute()
        {
            FetchDataBase chuChangPrice = PriceFactory.GetInstance("chuchang");
            FetchDataBase guojiPrice = PriceFactory.GetInstance("guoji");
            FetchDataBase guoNeiPrice = PriceFactory.GetInstance("guonei");
            FetchDataBase oilPrice = PriceFactory.GetInstance("oil");

            oilPrice.FetchAndFill(LogMsg);
            chuChangPrice.FetchAndFill(LogMsg);
            guojiPrice.FetchAndFill(LogMsg);
            guoNeiPrice.FetchAndFill(LogMsg);
           
        }
    }
}
