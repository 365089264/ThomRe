using System.Collections.Generic;

namespace VAV.Model.Data
{
    /// <summary>
    /// Menu Node
    /// </summary>
    public class MenuNode : BaseModel
    {
        /// <summary>
        /// Gets or sets the display name.
        /// </summary>
        /// <value>The display name.</value>
        public string DisplayName
        {
            get
            {
                return Culture == "zh-CN" ? _chineseName : _englishName;
            }
        }

        public string TraceName { get { return _englishName; } }

        /// <summary>
        /// Private _englishName
        /// </summary>
        private string _englishName;

        /// <summary>
        /// Private _chineseName
        /// </summary>
        private string _chineseName;


        /// <summary>
        /// Gets or sets the report id this node refer to
        /// </summary>
        public int ReportId { get; private set; }

        /// <summary>
        /// Gets or sets the node order.
        /// </summary>
        /// <value>The node order.</value>
        public int NodeOrder { get; private set; }

        /// <summary>
        /// Gets or sets the type of the action.
        /// </summary>
        /// <value>The type of the action.</value>
        public int ActionType { get; private set; }

        /// <summary>
        /// Gets or sets the Id.
        /// </summary>
        /// <value>The ID.</value>
        public int Id { get; private set; }

        /// <summary>
        /// Gets or sets the parent Id.
        /// </summary>
        /// <value></value>
        public int ParentId { get; private set; }

        /// <summary>
        /// Gets or sets the group id.
        /// </summary>
        public int GroupId { get; private set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public MenuNode Parent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is tree visible.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is tree visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsTreeVisible { get; private set; }

        /// <summary>
        /// Get the ric associated with the menu node.
        /// </summary>
        public string Ric { get; private set; }

        /// <summary>
        /// Gets or sets the children.
        /// </summary>
        /// <value>The children.</value>
        public List<MenuNode> Children { get; set; }


        public MenuNode(int id)
        {
            this.Id = id;
            this.Children = new List<MenuNode>();
        }

        public MenuNode(string eName, string cName, int id, int pId, int gId, int reportId, int nodeOrder, int actiontype, int istreeVisible, string ric)
        {
            this._englishName = eName;
            this._chineseName = cName;
            this.Id = id;
            this.ParentId = pId;
            this.GroupId = gId;
            this.ReportId = reportId;
            this.NodeOrder = nodeOrder;
            this.ActionType = actiontype;
            this.IsTreeVisible = istreeVisible == 1;
            this.Ric = ric;
        }
    }
}
