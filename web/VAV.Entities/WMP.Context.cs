﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class Genius_HistEntities : DbContext
    {
        public Genius_HistEntities()
            : base("name=Genius_HistEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<BANK_FIN_DETAIL> BANK_FIN_DETAIL { get; set; }
        public DbSet<BANK_FIN_PRD> BANK_FIN_PRD { get; set; }
        public DbSet<BANK_FIN_PRD_PROSP> BANK_FIN_PRD_PROSP { get; set; }
        public DbSet<CFP_FEE_CHNG> CFP_FEE_CHNG { get; set; }
        public DbSet<CFP_ISS_ORG> CFP_ISS_ORG { get; set; }
        public DbSet<CFP_PROFITSHEET> CFP_PROFITSHEET { get; set; }
        public DbSet<DISC_ACCE_CFP> DISC_ACCE_CFP { get; set; }
        public DbSet<FIN_PRD_RPT> FIN_PRD_RPT { get; set; }
        public DbSet<GEN_REF> GEN_REF { get; set; }
        public DbSet<ORG_PROFILE> ORG_PROFILE { get; set; }
        public DbSet<TRUST_PROFILE> TRUST_PROFILE { get; set; }
        public DbSet<v_WMP_BANK_PROD> v_WMP_BANK_PROD { get; set; }
        public DbSet<v_WMP_CFP> v_WMP_CFP { get; set; }
        public DbSet<v_WMP_TRUST> v_WMP_TRUST { get; set; }
        public DbSet<v_WMP_AREA_DIC> v_WMP_AREA_DIC { get; set; }
    }
}