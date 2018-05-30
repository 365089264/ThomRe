using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VAV.Entities;
using VAV.DAL.Fundamental;
using VAV.Model.Data.ZCX;

namespace VAV.Web.ViewModels.Fundamental
{
    public class NonlistIssuerDetailViewModel
    {
        public IssuerDetail IsserInfo { get; private set; }
        public int ComCode { get; private set; }
        public IEnumerable<IssuerBondInfo> BondList { get; private set; }

        public NonlistIssuerDetailViewModel(int comCode, ZCXRepository repo)
        {
            if (comCode == 0)
            {
                ComCode = 0;
                IsserInfo = null;
                BondList = null;
            }
            else
            {
                ComCode = comCode;
                IsserInfo = repo.GetIsserByComCode(comCode);
                BondList = repo.GetBondListByComCode(comCode);
            }

        }

    }
}