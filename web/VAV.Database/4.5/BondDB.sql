delete TypeOrder where type=9;
insert into TypeOrder values(1,'SHZ','深圳交易所',9);
insert into TypeOrder values(2,'SHH','上海交易所',9);
insert into TypeOrder values(3,'CFS','银行间',9);

delete TypeOrder where type=10;
insert into TypeOrder values(1,'00038600074336f7','中债登',10);
insert into TypeOrder values(2,'0003860028b56a72','中证登-上海',10);
insert into TypeOrder values(3,'0003860074798e06','中证登-深圳',10);
insert into TypeOrder values(4,'000405048503098a','上清所',10);

Create view v_bondview as
select  assetid
,code
,orig_iss_amt
,maturity_dt
,orig_issue_dt
,bond_name_cn
,bond_name_en
,coupon_class_cn
,coupon_class_en
,freq_cn
,freq_en
,orig_iss_cpn
,listing_dt
,orig_iss_px
,yield
,callorput_cn
,callorput_en
,isin_nm
,rating_src_cn
,rating_src_en
,party_rating_src_cn
,party_rating_src_en
,offer_registrant_name
,float_index
,float_offset
,day_count_cn
,day_count_en
,orig_iss_curr_cd
,seniority
,exchange_name_cn
,exchange_name_en
,trustee_name_cn
,trustee_name_en
,NVL(cdc_asset_class_cd,'OTH')  cdc_asset_class_cd
,(case when cdc_asset_class_cd is null then 'Other Instruments' else cdc_asset_class_en end) cdc_asset_class_en
,(case when cdc_asset_class_cd is null then cast('其它' as nvarchar2(100))  else cdc_asset_class_cn end) cdc_asset_class_cn
,(case when cdc_asset_class_cd is null then 30 else cdc_asset_class_number end) cdc_asset_class_number
,BondTerm_en
,BondTerm_cn
,NVL(bondRatingList.bondclass,'NR')latest_rating_cd
,NVL(issuerRatingList.bondclass,'NR') party_rating_cd
,IssuerInduSectorCd
,IssuerInduSectorEn
,IssuerInduSectorCn
,NVL(PartyCntryIncorpCd,100) PartyCntryIncorpCd
,(case when PartyCntryIncorpCd is null then cast('其它' as nvarchar2(50)) else PartyCntryIncorpDescr_cn end) PartyCntryIncorpDescr_cn
,(case when PartyCntryIncorpCd is null then cast('OTH' as nvarchar2(50)) else PartyCntryIncorpDescr_en end) PartyCntryIncorpDescr_en
from v_bondview_full
left join table(BondClassList('NR,AAA,AAA-,AA+,AA,AA-,A+,A,A-,A-1,A-2,BBB+,BBB,BBB-,BB+,BB,BB-,B+,B,B-,CCC+,CCC,CCC-,CC,C,D')) bondRatingList ON latest_rating_cd= bondRatingList.bondclass 
left join table(BondClassList('NR,AAA,AAA-,AA+,AA,AA-,A+,A,A-,A-1,A-2,BBB+,BBB,BBB-,BB+,BB,BB-,B+,B,B-,CCC+,CCC,CCC-,CC,C,D')) issuerRatingList ON party_rating_cd= issuerRatingList.bondclass 
;