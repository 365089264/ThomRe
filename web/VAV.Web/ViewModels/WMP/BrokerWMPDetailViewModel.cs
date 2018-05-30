using VAV.DAL.WMP;
using VAV.Entities;

namespace VAV.Web.ViewModels.WMP
{
    public class BrokerWMPDetailViewModel
    {
        public v_WMP_CFP ViewProd { get; private set;}

        public int InnerCode { get; private set; }

        public BrokerWMPDetailViewModel(int innerCode, WMPRepository repository)
        {
            ViewProd = repository.GetBrokerWmpDetailByInnerCode(innerCode);
            InnerCode = innerCode;
        }
    }
}