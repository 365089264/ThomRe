using System.Collections.Generic;
using VAV.DAL.CnE;
using VAV.Model.Data.CnE.GDT;
using Microsoft.Practices.Unity;
using System.Threading;

namespace VAV.DAL.Services
{
    public class GDTService
    {
        /// <summary>
        /// Inject NewGdtRespository for NewGdtRespository
        /// </summary>
        [Dependency]
        public NewGdtRespository NewGdtRespos { get; set; }

        private static Dictionary<int, List<TabNode>> nodeDictionary = null;
        private static Dictionary<int, GDTQueryWithDirection> queryDictionary = null;
        public List<TabNode> this[int key]
        {
            get
            {
                return GetTabNodesDictionary()[key];
            }

        }
        public Dictionary<int, List<TabNode>> GetTabNodesDictionary()
        {

            if (nodeDictionary == null)
            {
                nodeDictionary = NewGdtRespos.GetGDTTabNodes();

            }
            return nodeDictionary;


        }
        public Dictionary<int, GDTQueryWithDirection> GetQueryDictionary()
        {
            if (queryDictionary == null)
            {
                queryDictionary = NewGdtRespos.GetQueryConditions();

            }
            return queryDictionary;
        }
        public List<GDTQueryCondition> GetQueryConditions(int itemID, int direction, string tableName, string where)
        {
            Dictionary<int, GDTQueryWithDirection> dic = GetQueryDictionary();
            if (direction == 0 && dic.ContainsKey(itemID))
            {
                foreach (var i in dic[itemID].Left)
                {
                    if (i.ItemID == itemID)
                    {
                        i.ListItem = NewGdtRespos.GetQueryConditionContent(i.RelationColumn, tableName, where);
                    }
                    i.ListItem.Insert(0, new System.Web.Mvc.SelectListItem() { Text = Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? "所有" : "All", Value = "" });
                }
                return dic[itemID].Left;
            }
            else if (direction == 1 && dic.ContainsKey(itemID))
            {
                foreach (var i in dic[itemID].Right)
                {

                    if (i.ItemID == itemID)
                    {
                        i.ListItem = NewGdtRespos.GetQueryConditionContent(i.RelationColumn, tableName, where);
                    }

                    i.ListItem.Insert(0, new System.Web.Mvc.SelectListItem() { Text = Thread.CurrentThread.CurrentUICulture.Name == "zh-CN" ? "所有" : "All", Value = "" });
                }
                return dic[itemID].Right;
            }
            else
            {
                return new List<GDTQueryCondition>();
            }
        }
        public List<GDTQueryCondition> GetQueryConditionsDefault(int itemID, int direction, string tableName, string where)
        {
            Dictionary<int, GDTQueryWithDirection> dic = GetQueryDictionary();
            if (direction == 0 && dic.ContainsKey(itemID))
            {
                foreach (var i in dic[itemID].Left)
                {
                    if (i.ItemID == itemID)
                    {

                        if (i.ItemID == itemID)
                        {
                            i.ListItem = NewGdtRespos.GetQueryConditionContent(i.RelationColumn, tableName, where);
                        }
                    }
                }
                return dic[itemID].Left;
            }
            else if (direction == 1 && dic.ContainsKey(itemID))
            {
                foreach (var i in dic[itemID].Right)
                {

                    if (i.ItemID == itemID)
                    {
                        i.ListItem = NewGdtRespos.GetQueryConditionContent(i.RelationColumn, tableName, where);
                    }

                }
                return dic[itemID].Right;
            }
            else
            {
                return new List<GDTQueryCondition>();
            }
        }
    }
}
