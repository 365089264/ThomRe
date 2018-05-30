create or replace PROCEDURE GetBondListByComCode
(
	P_ORG_UNI_CODE varchar2,
  P_Culture varchar2,
  P_CUR out sys_refcursor
)
as
sqlcommond varchar2(4000);
tablename varchar2(50);
begin
  if P_Culture='zh-CN' then 
    tablename:='v_bond_cn';
  else
    tablename:='v_bond_en';
  end if;
  sqlcommond:='select AssetId,  BondName,  Code,  ShortName,  IssueDate,  IssueAmount,  BondClassDescr,  OptionDescr,  CouponClassDescr,  PayFrequency,  InterestRate,  Term,  AnnoucementDate,  OrigDatedDate,  MaturityDate,  ListingDate,  PayDate,  FloatIndexDescr,  FloatOffset,  RatingCd,  RatingSrcDescr,  PartyRatingCd,  PartyRatingSrcDescr
  from '||tablename||'
  join (select bond_code,actu_end_date as mdate from (select * from bond_rate_info where isvalid=1) a,
	   (select bond_id,bond_code, row_number() over(partition by bond_id order by 
			case sec_mar_par when 3 then 1 when 1 then 2 when 2 then 3 when 4 then 4 else 5 end) mm  
		from BOND_BASIC_INFO,BOND_ISSER_INFO 
		WHERE BOND_BASIC_INFO.BOND_UNI_CODE=BOND_ISSER_INFO.BOND_UNI_CODE
		AND ORG_UNI_CODE = '||P_ORG_UNI_CODE||' 
		AND BOND_BASIC_INFO.isvalid = 1 
		AND BOND_ISSER_INFO.isvalid = 1 
		) b where mm = 1 AND a.bond_id = b.bond_id AND bond_code is not null
	)  bd on Code = BOND_CODE and MaturityDate = mdate
	where IssueDate is not null order by IssueDate desc';
  dbms_output.put_line(sqlcommond);                 
  OPEN P_CUR FOR  sqlcommond
 ;
end;