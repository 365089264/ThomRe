using System;
using VAV.DAL.WMP;
using VAV.Entities;
using VAV.DAL.Report;

namespace VAV.Web.ViewModels.WMP
{
    public class TrustWMPDetailViewModel
    {
        public int InnerCode { get; set; }

        public v_WMP_TRUST ViewProd { get; set; }

        public ORG_PROFILE OrgProfile { get; set; }

        public TrustWMPDetailViewModel(int innerCode, WMPRepository repository)
        {
            ViewProd = repository.GetTrustWmpDetailByInnerCode(innerCode);
            OrgProfile = repository.GetOrgProfileByOrgCode(Convert.ToInt32(ViewProd.ORGCODE.Value));
            InnerCode = innerCode;
        }
    }
}