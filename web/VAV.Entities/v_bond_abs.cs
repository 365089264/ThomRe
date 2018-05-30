//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VAV.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class V_BOND_ABS
    {
        public long ID { get; set; }
        public Nullable<long> BOND_ID { get; set; }
        public Nullable<long> BOND_UNI_CODE { get; set; }
        public string BOND_FULL_NAME { get; set; }
        public string ENG_FULL_NAME { get; set; }
        public string BOND_CODE { get; set; }
        public string BOND_SHORT_NAME { get; set; }
        public string ENG_SHORT_NAME { get; set; }
        public string ISIN_CODE { get; set; }
        public Nullable<decimal> BOND_MATU { get; set; }
        public Nullable<short> MATU_UNIT_PAR { get; set; }
        public string UNIT { get; set; }
        public Nullable<System.DateTime> LIST_DATE { get; set; }
        public Nullable<short> BOND_TYPE_PAR { get; set; }
        public string BONDCLASSCN { get; set; }
        public string BONDCLASSEN { get; set; }
        public Nullable<short> SEC_MAR_PAR { get; set; }
        public string ISSUEMARKET { get; set; }
        public Nullable<System.DateTime> ISS_DECL_DATE { get; set; }
        public Nullable<System.DateTime> ISS_START_DATE { get; set; }
        public Nullable<System.DateTime> ISS_END_DATE { get; set; }
        public Nullable<decimal> ACTU_ISS_AMUT { get; set; }
        public Nullable<decimal> ISS_PRI { get; set; }
        public Nullable<short> IS_REDEM_PAR { get; set; }
        public Nullable<short> IS_RESA_PAR { get; set; }
        public Nullable<short> IS_GUAR_PAR { get; set; }
        public Nullable<decimal> TOTAL_SIZE { get; set; }
        public Nullable<long> ISSUER_ORG_UNI_CODE { get; set; }
        public string ISSUERCN { get; set; }
        public string ISSUEREN { get; set; }
        public string SPONSOR { get; set; }
        public string LEADUNDERWRITER { get; set; }
        public string VICEUNDERWRITER { get; set; }
        public string UNDERWRITER { get; set; }
        public string FUNDINSTITUTION { get; set; }
        public string REGULATOR { get; set; }
        public string AUDITINGOFFICES { get; set; }
        public string LEGALADVISOR { get; set; }
        public string TRUSTEE { get; set; }
        public string ADVERTISINGAGENCY { get; set; }
        public string UDL_ASE { get; set; }
        public string GUAOR_NAME { get; set; }
        public Nullable<System.DateTime> INTE_START_DATE { get; set; }
        public Nullable<System.DateTime> ACTU_END_DATE { get; set; }
        public Nullable<short> RATE_TYPE_PAR { get; set; }
        public string RATETYPENAME { get; set; }
        public Nullable<short> BASE_RATE_PAR { get; set; }
        public string BASERATENAME { get; set; }
        public Nullable<decimal> ISS_COUP_RATE { get; set; }
        public Nullable<decimal> BAS_SPR { get; set; }
        public Nullable<short> INTE_PAY_FREQ { get; set; }
        public Nullable<System.DateTime> FIR_PAY_DATE { get; set; }
        public Nullable<System.DateTime> MATU_PAY_DATE { get; set; }
        public string BOND_CRED_LEVEL { get; set; }
        public string BONDRATINGAGENCYCN { get; set; }
        public string BONDRATINGAGENCYEN { get; set; }
        public string RATING { get; set; }
        public string RATINGAGENCYCN { get; set; }
        public string RATINGAGENCYEN { get; set; }
        public Nullable<System.DateTime> ISS_DATE { get; set; }
        public string AUCTIONTYPE { get; set; }
        public string AUCTIONMETHOD { get; set; }
        public string AUCTIONCODE { get; set; }
    }
}
