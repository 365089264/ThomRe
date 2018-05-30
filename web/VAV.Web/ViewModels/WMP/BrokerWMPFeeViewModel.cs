using VAV.DAL.WMP;
using VAV.Entities;
using System.Collections.Generic;
using VAV.Model.Data.WMP;
using System.Linq;
using System;

namespace VAV.Web.ViewModels.WMP
{
    public class BrokerWMPFeeViewModel
    {
        public int InnerCode { get; private set; }
        public List<BrokerFee> BrokerFeeData {get; private set;}
        public List<Tuple<string, string, string, string>> ManageFee { get; private set; }
        public List<Tuple<string, string, string, string>> CustodianFee { get; private set; }
        public List<Tuple<string, string, string>> SubscribeFee { get; private set; }
        public List<Tuple<string, string, string>> PurchaseFee { get; private set; }
        public List<Tuple<string, string, string>> RedeemFee { get; private set; }

        public BrokerWMPFeeViewModel(int innerCode, WMPRepository repository)
        {
            BrokerFeeData = repository.GetWmpBrokerFeeById(innerCode);
            ManageFee = BrokerFeeData.Where(m => m.FEE_TYPE == 5).Select(m => new Tuple<string, string, string, string>(m.ANNU_YLD, m.UNT_NV,  m.FEE, m.REMARK)).ToList();
            CustodianFee = BrokerFeeData.Where(m => m.FEE_TYPE == 6).Select(m => new Tuple<string, string, string, string>(m.ANNU_YLD, m.UNT_NV, m.FEE, m.REMARK)).ToList(); ;
            SubscribeFee = BrokerFeeData.Where(m => m.FEE_TYPE == 1).Select(m => new Tuple<string, string, string>(m.MNY, m.TERM, m.FEE)).ToList();
            PurchaseFee = BrokerFeeData.Where(m => m.FEE_TYPE == 3).Select(m => new Tuple<string, string, string>(m.MNY, m.TERM, m.FEE)).ToList();
            RedeemFee = BrokerFeeData.Where(m => m.FEE_TYPE == 7).Select(m => new Tuple<string, string, string>(m.MNY, m.TERM, m.FEE)).ToList();
            InnerCode = innerCode;
        }
    }
}