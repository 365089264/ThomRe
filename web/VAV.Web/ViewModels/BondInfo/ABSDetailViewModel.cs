using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using VAV.Entities;
using VAV.Model.Data.Bond;
using VAV.Model.Data.ZCX;
using VAV.Web.ViewModels.Bond;

namespace VAV.Web.ViewModels.BondInfo
{
    public class ABSDetailViewModel
    {
        public V_BOND_ABS VBondAbs { get; set; }
        public List<BOND_SIZE_CHAN> ListBondSizeChan { get; set; }
        public List<RATE_ORG_CRED_HIS> ListRateOrgCredHis { get; set; }
        public List<BondRatingHist> BondRatingHist { get; set; }
    }
}