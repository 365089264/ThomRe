using System;
using System.Collections.Generic;
using System.Linq;

namespace VAV.Model.Data.ZCX
{
    public class NonlistIssuer : BaseModel
    {

        const int DISPLAY_COUNT = 4;

        /// <summary>
        /// 公司统一编码
        /// </summary>
        public long COM_UNI_CODE { get; set; }

        /// <summary>
        /// 中文名称
        /// </summary>
        public string COM_CHI_NAME { get; set; }

        /// <summary>
        /// 中文简称
        /// </summary>
        public string COM_CHI_SHORT_NAME { get; set; }

        /// <summary>
        /// 拼音简称
        /// </summary>
        public string COM_SPE_SHORT_NAME { get; set; }

        /// <summary>
        /// 英文名称
        /// </summary>
        public string COM_ENG_NAME { get; set; }

        /// <summary>
        /// 英文简称
        /// </summary>
        public string COM_ENG_SHORT_NAME { get; set; }


        public string COM_NAME
        {
            get
            {
                if (Culture != "zh-CN")
                    return COM_ENG_NAME ?? COM_CHI_NAME;
                else
                    return COM_CHI_NAME;
            }
        }

        public string COM_SHORT_NAME
        {
            get
            {
                if (Culture != "zh-CN")
                    return COM_ENG_SHORT_NAME ?? COM_CHI_SHORT_NAME;
                else
                    return COM_CHI_SHORT_NAME;
            }
        }


        private string _CompanyType;

        /// <summary>
        /// 公司类型参数
        /// </summary>
        public string TYPE_BIG
        {
            get
            {
                if (!string.IsNullOrEmpty(_CompanyType))
                {
                    if (_CompanyType.Trim().Equals("(非政府机构、事业单位、金融机构)一般企业"))
                        return "一般企业";
                    if (_CompanyType.Trim().Equals("非机构之其他类型"))
                        return "其它";
                }
                else
                    return "其它";
                return _CompanyType;
            }
            set
            {
                _CompanyType = value;
            }
        }

        /// <summary>
        /// 公司性质
        /// </summary>
        //public string COM_ORGA_FORM { get; set; }

        /// <summary>
        /// 母公司
        /// </summary>
        //public string PARE_COM { get; set; }

        /// <summary>
        /// 董事会秘书
        /// </summary>
        public string COM_STP { get; set; }

        /// <summary>
        /// 董秘联系电话
        /// </summary>
        public string STP_TEL { get; set; }

        /// <summary>
        /// 董秘传真
        /// </summary>
        public string STP_TAX { get; set; }

        /// <summary>
        /// 董秘联系地址
        /// </summary>
        public string STP_CON_ADDR { get; set; }

        /// <summary>
        /// 董秘电子邮件地址
        /// </summary>
        public string STP_MAIL { get; set; }

        /// <summary>
        /// 注册地址
        /// </summary>
        public string REG_ADDR { get; set; }

        /// <summary>
        /// 注册地邮编
        /// </summary>
        public string REG_ADDR_POST { get; set; }

        /// <summary>
        /// 办公地址
        /// </summary>
        public string OFFI_ADDR { get; set; }

        /// <summary>
        /// OFFI_ADDR_POST
        /// </summary>
        public string OFFI_ADDR_POST { get; set; }

        /// <summary>
        /// 联系地址
        /// </summary>
        public string COM_ADDR { get; set; }

        /// <summary>
        /// 联系地邮编
        /// </summary>
        public string COM_ADDR_POST { get; set; }

        /// <summary>
        /// 公司网址
        /// </summary>
        public string COM_WEB { get; set; }

        /// <summary>
        /// 公司联系人
        /// </summary>
        public string COM_CON_PER { get; set; }

        /// <summary>
        /// 公司电话
        /// </summary>
        public string COM_TEL { get; set; }

        /// <summary>
        /// 客服联系电话
        /// </summary>
        public string CUS_CON_TEL { get; set; }

        /// <summary>
        /// 公司传真
        /// </summary>
        public string COM_FAX { get; set; }

        /// <summary>
        /// 公司电子邮件地址
        /// </summary>
        public string MAIL_ADDR { get; set; }

        //private IEnumerable<string> _bondList;

        ///// <summary>
        ///// 债券列表
        ///// </summary>
        //public IEnumerable<string> BOND_LIST
        //{
        //    set
        //    {
        //        _bondList = value;
        //    }
        //}

        //public string BOND_LIST_STRING
        //{
        //    get
        //    {
        //        if (!string.IsNullOrEmpty(SearchBond))
        //        {
        //            var result = "";
        //            if (_bondList.Count() <= DISPLAY_COUNT)
        //                result = _bondList.Take(DISPLAY_COUNT).Aggregate((a, b) => a + "," + b);
        //            else
        //            {
        //                if (_bondList.Where(x => x.Contains(SearchBond)).Count() > DISPLAY_COUNT)
        //                    result = _bondList.Where(x => x.Contains(SearchBond)).Take(DISPLAY_COUNT).Aggregate((a, b) => a + "," + b) + "...";
        //                else
        //                {
        //                    var leftCount = DISPLAY_COUNT - _bondList.Where(x => x.Contains(SearchBond)).Count();
        //                    result = _bondList.Where(x => x.Contains(SearchBond)).Take(DISPLAY_COUNT).Union(_bondList.Where(x => !x.Contains(SearchBond)).Take(leftCount)).Aggregate((a, b) => a + "," + b) + "...";

        //                }
        //            }
        //            return result;
        //        }
        //        return _bondList.Count() > 0
        //            ? (_bondList.Count() > DISPLAY_COUNT
        //                ? _bondList.Take(DISPLAY_COUNT).Aggregate((a, b) => a + "," + b) + "..."
        //                : _bondList.Take(DISPLAY_COUNT).Aggregate((a, b) => a + "," + b))
        //            : "";
        //    }
        //}

        ///// <summary>
        ///// bond searched 
        ///// </summary>
        //public string SearchBond { get; set; }
    }
}
