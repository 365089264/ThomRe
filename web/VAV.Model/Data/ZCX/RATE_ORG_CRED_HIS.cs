using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VAV.Model.Data.ZCX
{
    /// <summary>
    /// zcx.RATE_ORG_CRED_HIS in vav db
    /// </summary>
    public class RATE_ORG_CRED_HIS : BaseModel
    {
        public long ID { get; set; }
        public int ISVALID { get; set; }
        public long COM_UNI_CODE { get; set; }

        /// <summary>
        /// 评级机构代码
        /// </summary>
        public long ORG_UNI_CODE { get; set; }

        /// <summary>
        /// 评级机构
        /// </summary>
        public string Org { get; set; }


        /// <summary>
        /// 评级日期
        /// </summary>
        public DateTime RATE_WRIT_DATE { get; set; }

        /// <summary>
        /// 评级类型参数
        /// </summary>
        public int RATE_TYPE_PAR { get; set; }

        /// <summary>
        /// 评级类型
        /// 1-长期;2-短期
        /// </summary>
        public string RATE_TYPE {
            get
            {
                switch (RATE_TYPE_PAR)
                {
                    case 1:
                        return "长期";
                    case 2:
                        return "短期";
                    default:
                        return "-";
                }
            } 
        }

        /// <summary>
        /// 评级种类参数
        /// </summary>
        public int RATE_CLS_PAR { get; set; }

        /// <summary>
        /// 评级种类
        /// 1-首次评级,2-跟踪评级
        /// </summary>
        public string RATE_CLS {
            get
            {
                switch (RATE_CLS_PAR)
                {
                    case 1:
                        return "首次评级";
                    case 2:
                        return "跟踪评级";
                    default:
                        return "-";
                }
            } 
        }

        /// <summary>
        /// 评级
        /// </summary>
        public string ISS_CRED_LEVEL { get; set; }


        /// <summary>
        /// 评级观点
        /// </summary>
        public string RATE_POINT { get; set; }

        /// <summary>
        /// 评级展望参数
        /// </summary>
        public int RATE_PROS_PAR { get; set; }

        /// <summary>
        /// 评级展望
        /// 1、正面；2、观望；3、负面；4、稳定
        /// </summary>
        public string RATE_PROS
        {
            get
            {
                switch (RATE_PROS_PAR)
                {
                    case 1:
                        return "正面";
                    case 2:
                        return "观望";
                    case 3:
                        return "负面";
                    case 4:
                        return "稳定";
                    default:
                        return "-";
                }
            }
        }

        /// <summary>
        /// 报告id
        /// </summary>
        public long RATE_ID { get; set; }

        /// <summary>
        /// 报告名称
        /// </summary>
        public string RATE_TITLE { get; set; }

        public bool ContainFile { get; set; }
    }
}
