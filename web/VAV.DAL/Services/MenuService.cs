using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using VAV.DAL.Menu;
using VAV.Model.Data;

namespace VAV.DAL.Services
{
    /// <summary>
    /// Menu Service
    /// </summary>
    public class MenuService : BaseService
    {
        /// <summary>
        /// Inject MenuRepository for MenuService
        /// </summary>
        [Dependency]
        public MenuRepository MenuRepository { get; set; }

        /// <summary>
        /// Root MenuNode
        /// </summary>
        private MenuNode _root;

        private int _defaultReportId = -100;

        /// <summary>
        /// Gets the report Menu
        /// </summary>
        /// <returns></returns>
        public MenuNode GetMenu()
        {
            if(_root != null) return _root;
            var menuNodes = MenuRepository.GetMenuNode().ToList();

            IEnumerable<int> menuGroups = menuNodes.Where(re=>re.ParentId==0).OrderBy(re=>re.NodeOrder).Select(n => n.GroupId).Distinct().ToList();
            var nodeDictionary = new Dictionary<int, MenuNode>();
            _root = new MenuNode(0);

            foreach (int group in menuGroups)
            {
                IEnumerable<MenuNode> nodes = menuNodes.Where(n => n.GroupId == group).Select(n => n);

                foreach (MenuNode node in nodes)
                {
                    if (!nodeDictionary.ContainsKey(node.Id))
                    {
                        nodeDictionary.Add(node.Id, node);
                        nodeDictionary[node.Id].Children = new List<MenuNode>();
                    }

                    if (node.ParentId == 0) //root node
                        _root.Children.Add(node);
                }

                foreach (MenuNode node in nodes)
                {
                    if (nodeDictionary.ContainsKey(node.ParentId))
                    {
                        nodeDictionary[node.ParentId].Children.Add(node);
                        node.Parent = nodeDictionary[node.ParentId];
                    }
                }
            }
            return _root;
        }

        /// <summary>
        /// Gets the menu node by id.
        /// </summary>
        /// <param name="nodeId">The node id.</param>
        /// <returns></returns>
        public MenuNode GetMenuNodeByNodeId(int nodeId)
        {
            var root = GetMenu();
            return GetDescendentNode(root, nodeId, true);
        }

        /// <summary>
        /// Gets the menu node by report id.
        /// </summary>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public MenuNode GetMenuNodeByReportId(int reportId)
        {
            var root = GetMenu();
            return GetDescendentNode(root, reportId, false);
        }

        /// <summary>
        /// Gets the menu node by report id.
        /// </summary>
        /// <param name="reportId"></param>
        /// <returns></returns>
        public List<MenuNode> GetMenuNodeByReportIdAndStyle(int reportId, string style)
        {
            var nodes = GetDescendentNode(GetMenu(), reportId, false);
            var nodeLayout = MenuRepository.GetMenuNodeLayout(reportId);

            return (from n in nodes.Children
                            join l in nodeLayout on n.Id equals l.NodeId
                            where l.Style == style
                            select n).ToList();
        }

        /// <summary>
        /// Get the default report
        /// </summary>
        /// <returns></returns>
        public MenuNode GetDefaultReport()
        {
            if (_defaultReportId  == -100)
            {
                _defaultReportId = 0;
            }
            return MenuRepository.GetMenuById(_defaultReportId);
        }

        /// <summary>
        /// Gets the descendent node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="id">The id.</param>
        /// <param name="isNodeId">if set to <c>true</c> [is node id].</param>
        /// <returns></returns>
        private static MenuNode GetDescendentNode(MenuNode node, int id, bool isNodeId)
        {
            if (node == null) return null;
            int idToCheck = isNodeId ? node.Id : node.ReportId;
            if (idToCheck == id)
            {
                return node;
            }

            foreach (var menuNode in node.Children)
            {
                var childMenu = GetDescendentNode(menuNode, id, isNodeId);
                if(childMenu != null)
                {
                    return childMenu;
                }
            }
            return null;
        }
    }
}
