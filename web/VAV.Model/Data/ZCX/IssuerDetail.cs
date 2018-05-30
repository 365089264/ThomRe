namespace VAV.Model.Data.ZCX
{
    public class IssuerDetail : BaseModel
    {
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
                if (_CompanyType.Trim().Equals("(非政府机构、事业单位、金融机构)一般企业"))
                    return "一般企业";
                if (_CompanyType.Trim().Equals("非机构之其他类型"))
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
        public string COM_ORGA_FORM { get; set; }

        /// <summary>
        /// 成立日期
        /// </summary>
        public string EST_DATE { get; set; }

        /// <summary>
        /// 注册地址
        /// </summary>
        public string REG_ADDR { get; set; }

        /// <summary>
        /// 公司网址
        /// </summary>
        public string COM_WEB { get; set; }

        /// <summary>
        /// 公司电话
        /// </summary>
        public string COM_TEL { get; set; }

        /// <summary>
        /// 公司简介
        /// </summary>
        public string COM_PRO { get; set; }

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
        /// 注册地邮编
        /// </summary>
        public string REG_ADDR_POST { get; set; }

        /// <summary>
        /// 办公地址
        /// </summary>
        public string OFFI_ADDR { get; set; }

        /// <summary>
        /// 办公地邮编
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
        /// 公司联系人
        /// </summary>
        public string COM_CON_PER { get; set; }

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

        /// <summary>
        /// 注册资本
        /// </summary>
        public decimal? REG_CAP { get; set; }

        /// <summary>
        /// 法人代表
        /// </summary>
        public string LEG_PER { get; set; }

        /// <summary>
        /// 总经理
        /// </summary>
        public string GEN_MAN { get; set; }

        /// <summary>
        /// 工商登记号_营业执照注册号
        /// </summary>
        public string IC_REG_CODE { get; set; }

        /// <summary>
        /// 国税税务登记号
        /// </summary>
        public string NAT_TAX_REG_CODE { get; set; }

        /// <summary>
        /// 地税税务登记号
        /// </summary>
        public string LOC_TAX_REG_CODE { get; set; }

        /// <summary>
        /// 主营业务
        /// </summary>
        public string MAIN_BUS { get; set; }

        /// <summary>
        /// 兼营业务
        /// </summary>
        public string SID_BUS { get; set; }

        /// <summary>
        /// 成立情况与历史沿革
        /// </summary>
        public string COM_HIS { get; set; }

        /// <summary>
        /// 员工总数
        /// </summary>
        public int? STAFF_SUM { get; set; }
    }
}
