using VAV.DAL.WMP;
using VAV.Entities;
using System.Collections.Generic;
using VAV.Model.Data.WMP;
using System.Linq;
using System;

namespace VAV.Web.ViewModels.WMP
{
    public class BrokerWMPFinIdxViewModel
    {
        public int InnerCode { get; private set; }
        public List<BrokerFinIdx> BrokerFinIdxData { get; private set; }
        
        public BrokerWMPFinIdxViewModel(int innerCode, WMPRepository repository)
        {
            InnerCode = innerCode;
            BrokerFinIdxData = repository.GetWmpBrokerFinIdxById(innerCode);
        }
    }
}