using VAV.DAL.WMP;
using VAV.Entities;

namespace VAV.Web.ViewModels.WMP
{
    public class BankWMPDetailViewModel
    {

        public v_WMP_BANK_PROD ViewProd { get; private set;}

        public int InnerCode { get; private set; }

        public BankWMPDetailViewModel(int innerCode, WMPRepository repository)
        {
            ViewProd = repository.GetViewProdByInnerCode(innerCode);
            InnerCode = innerCode;
        }
    }
}