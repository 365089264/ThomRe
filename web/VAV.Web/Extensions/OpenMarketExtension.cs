using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VAV.DAL.Common;
using VAV.Model.Data.OpenMarket;

namespace VAV.Web.Extensions
{
     public static class OpenMarketExtension
    {
        #region Inovked when view needs to category or when user export to excel
        /// <summary>
        /// Get different categories
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="categoryType"></param>
        /// <param name="flag">used to sign which category type is used</param>
        /// <returns></returns>
        public static List<OpenMarketRepo> GetCategorys(List<OpenMarketRepo> repo, string categoryType, ref int flag)
        {
            List<string> category_List = new List<string>();
            switch (categoryType)
            {
                case ConstValues.category_Term:
                    category_List = repo.OrderBy(t => t.Term).Select(t => t.OperationTerm).Distinct().ToList();
                    repo.GroupBy(t => t.OperationTerm);
                    flag = 1;
                    break;
                case ConstValues.category_Variety:
                    category_List = repo.Select(t => t.Category).Distinct().ToList();
                    repo.GroupBy(t => t.Category);
                    flag = -1;
                    break;
                default:
                    flag = 0;
                    break;
            }
            if (category_List.Count == 0)
            {
                return repo.OrderByDescending(r => r.Date).ToList();
            }
            else
            {
                OpenMarketRepo totalRow = new OpenMarketRepo { IsSumItem = true, Category = Resources.Global.Total, Amount = 0, Volume = 0 };
                List<OpenMarketRepo> selectedGroupRepo = new List<OpenMarketRepo>();
                foreach (string item in category_List)
                {
                    OpenMarketRepo sumRow = new OpenMarketRepo { IsSumItem = true, Amount = 0, Volume = 0 };
                    List<OpenMarketRepo> temp = ItemGroupOfCurrentCategory(item, repo, flag, ref sumRow);
                    totalRow.Amount += sumRow.Amount;
                    totalRow.Volume += sumRow.Volume;
                    selectedGroupRepo.Add(sumRow);
                    foreach (OpenMarketRepo t in temp)
                    {
                        selectedGroupRepo.Add(t);
                    }
                }
                selectedGroupRepo.Add(totalRow);
                return selectedGroupRepo;
            }
        }

        /// <summary>
        /// Select items from the original repo that belongs to the current category
        /// </summary>
        /// <param name="category">Category name</param>
        /// <param name="repo">the original repo</param>
        /// <param name="sumRow">used as a sum row of the current category</param>
        /// <returns></returns>
        public static List<OpenMarketRepo> ItemGroupOfCurrentCategory(string category, List<OpenMarketRepo> repo, int flag, ref OpenMarketRepo sumRow)
        {
            List<OpenMarketRepo> selectedGroupRepo = new List<OpenMarketRepo>();
            if (flag == 1)
            {
                selectedGroupRepo = repo.OrderByDescending(r => r.Date).Where(r => r.OperationTerm == category).ToList();
                sumRow.Category = selectedGroupRepo.FirstOrDefault().OperationTerm;
            }
            else if (flag == -1)
            {
                selectedGroupRepo = repo.OrderByDescending(r => r.Date).Where(r => r.Category == category).ToList();
                sumRow.Category = selectedGroupRepo.FirstOrDefault().Category;
            }
            double? sum_Amount = selectedGroupRepo.Sum(t => t.Amount);
            double? sum_Volume = selectedGroupRepo.Sum(t => t.Volume);
            sumRow.IsSumItem = true;
            sumRow.Amount = sum_Amount;
            sumRow.Volume = sum_Volume;
            return selectedGroupRepo;
        }
        #endregion
    }
}