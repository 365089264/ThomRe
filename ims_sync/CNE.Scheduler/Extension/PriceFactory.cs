using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CNE.Scheduler.Extension.Model;


namespace CNE.Scheduler.Extension
{
    public static class PriceFactory
    {
        public static FetchDataBase GetInstance(string type)
        {
            FetchDataBase instance = null;
            switch (type)
            {
                case "chuchang":
                    instance = new FetchPriceEntity<ChuChangPrice>(type, "LongZhong_LeaveFactoryPrice");
                    break;
                case "guoji":
                    instance = new FetchPriceEntity<GuoJiPrice>(type, "LongZhong_InternationalPrice");
                    break;
                case "guonei":
                    instance = new FetchPriceEntity<GuoNeiPrice>(type, "LongZhong_NationalPrice");
                    break;
                case "oil":
                    instance = new FetchPriceEntity<OilPrice>(type, "LongZhong_OilPrice");
                    break;
                default:
                    break;
            }
            return instance;
        }
    }
}
