using System.Collections.Generic;
using System.Linq;
using VAV.Model.Data;
using VAV.Entities;

namespace VAV.DAL.Menu
{
    public class MenuRepository : BaseRepository
    {
        /// <summary>
        /// Get the list of menu node
        /// </summary>
        /// <returns>List<MenuNode/></returns>
        public IEnumerable<MenuNode> GetMenuNode()
        {
            IEnumerable<MenuNode> nodes;
            using (var CMADB = new CMAEntities())
            {
                nodes = ((from r in CMADB.MENUs where r.ISVISIBLE.Value == 1 select r)
                        .AsEnumerable()
                        .Select(n => new MenuNode(
                            n.ENGLISH_NAME,
                            n.CHINESE_NAME,
                            (int) n.ID,
                            (int) n.PARENT_ID,
                            (int) (n.GROUP_TYPE ?? -1),
                            (int) (n.REPORT_ID ?? -1),
                            (int) (n.NODEORDER ?? 0),
                            (int) (n.ACTIONTYPE ?? 0),
                            (int) n.ISTREEVISIBLE,
                            n.RIC
                            ))).OrderBy(re => re.ParentId).ThenBy(n => n.NodeOrder).ThenBy(re => re.Id).ToList();
            }

            return nodes;

        }

        public MenuNode GetMenuById(int id)
        {
            using (var CMADB = new CMAEntities())
            {
                return CMADB.MENUs.Where(m => m.ID == id).ToList().Select(n => new MenuNode(n.ENGLISH_NAME, n.CHINESE_NAME,
                            (int)n.ID,
                            (int)n.PARENT_ID,
                            (int)(n.GROUP_TYPE ?? -1),
                            (int)(n.REPORT_ID ?? -1),
                            (int)(n.NODEORDER ?? 0),
                            (int)(n.ACTIONTYPE ?? 0),
                            (int)n.ISTREEVISIBLE,
                            n.RIC)).FirstOrDefault();
            }
        }


        /// <summary>
        /// Get the css style of the children of node with id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEnumerable<VAV.Model.Data.MenuNodeLayout> GetMenuNodeLayout(int reportId)
        {
            using (var CMADB = new CMAEntities())
            {
                return (from n in CMADB.MENUNODELAYOUTs
                        where n.REPORT_ID == reportId
                        select new VAV.Model.Data.MenuNodeLayout { ReportId = (int) n.REPORT_ID, NodeId = (int) n.NODE_ID, Style = n.DISPLAY_STYLE }).ToList();
            }

        }
    }
}
