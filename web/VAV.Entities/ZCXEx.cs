using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace VAV.Entities
{
    public partial class V_BOND_ABS
    {
        public string Bond_Full_Display
        {
            get
            {
                return BOND_FULL_NAME;
            }
        }

        public string Bond_Short_Display
        {
            get
            {
                return BOND_SHORT_NAME;
            }
        }

        public string Issuer_Display
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? ISSUERCN : ISSUEREN;
            }
        }
        public string BondRatingAgencyDisplay
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? BONDRATINGAGENCYCN : BONDRATINGAGENCYEN;
            }
        }
        public string RatingAgencyDisplay
        {
            get
            {
                return Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? RATINGAGENCYCN : RATINGAGENCYEN;
            }
        }

        public string ISS_DECL_DATEDisplay
        {
            get { return ISS_DECL_DATE==null? "" : ISS_DECL_DATE.Value.ToString("yyyy-MM-dd"); }
        }
        public string LIST_DATEDisplay
        {
            get { return LIST_DATE.Value == null ? "" : LIST_DATE.Value.ToString("yyyy-MM-dd"); }
        }
        public string ISS_START_DATEDisplay
        {
             get { return ISS_START_DATE == null ? "" : ISS_START_DATE.Value.ToString("yyyy-MM-dd"); }
        }
        public string ISS_END_DATEDisplay
        {
             get { return ISS_END_DATE == null ? "" : ISS_END_DATE.Value.ToString("yyyy-MM-dd"); }
        }
        public string INTE_START_DATEDisplay
        {
             get { return INTE_START_DATE == null ? "" : INTE_START_DATE.Value.ToString("yyyy-MM-dd"); }
        }
        public string ACTU_END_DATEDisplay
        {
             get { return ACTU_END_DATE == null ? "" : ACTU_END_DATE.Value.ToString("yyyy-MM-dd"); }
        }
        public string FIR_PAY_DATEDisplay
        {
             get { return FIR_PAY_DATE == null ? "" : FIR_PAY_DATE.Value.ToString("yyyy-MM-dd"); }
        }
        public string MATU_PAY_DATEDisplay
        {
             get { return MATU_PAY_DATE == null ? "" : MATU_PAY_DATE.Value.ToString("yyyy-MM-dd"); }
        }
        public string ISS_DATEDisplay
        {
             get { return ISS_DATE == null ? "" : ISS_DATE.Value.ToString("yyyy-MM-dd"); }
        }

        public string ACTU_ISS_AMUTDisplay
        {
            get { return ACTU_ISS_AMUT.HasValue ? ((double)ACTU_ISS_AMUT).ToString("N2") : ""; }
        }

        public string ISS_PRIDisplay
        {
            get { return ISS_PRI.HasValue ? ((double)ISS_PRI).ToString("N2") : ""; }
        }

        public string BOND_MATUDisplay
        {
            get { return BOND_MATU.HasValue ? ((double)BOND_MATU).ToString("N2") : ""; }
        }

        public string ISS_COUP_RATEDisplay
        {
            get { return ISS_COUP_RATE.HasValue ? ((double)ISS_COUP_RATE.Value).ToString("N2") : ""; }
        }

        public string BAS_SPRDisplay
        {
            get { return BAS_SPR.HasValue ? ((double)BAS_SPR.Value).ToString("N2") : ""; }
        }
    }

    public partial class BOND_SIZE_CHAN
    {
        public string CHAN_TYPE_PARNAME { get; set; }

        public string End_DateDisplay
        {
            get { return END_DATE == null ? "" : END_DATE.Value.ToString("yyyy-MM-dd"); }
        }

        public string SIZE_CHANDisplay
        {
            get { return SIZE_CHAN.HasValue ? ((double)SIZE_CHAN).ToString("N2") : ""; }
        }

        public string TOTAL_SIZEDisplay
        {
            get { return TOTAL_SIZE.HasValue ? ((double)TOTAL_SIZE).ToString("N2") : ""; }
        }
    }
}
