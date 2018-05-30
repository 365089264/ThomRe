using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VAV.Model.Data;

namespace VAV.Web.ViewModels.Home
{
    public class TreeViewModel
    {
        public MenuNode Root { get; private set; }
        /// <summary>
        /// Gets or sets the open node code.
        /// </summary>
        /// <value>The open node code.</value>
        public string OpenNodeCode { get; private set; }

        /// <summary>
        /// Gets or sets the default report ID.
        /// </summary>
        /// <value>The default report ID.</value>
        public int SelectedReportID { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeViewModel"/> class.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <param name="id">The id.</param>
        public TreeViewModel(MenuNode root,int id =-1)
        {
            Root = root;
            var code = BuildFolderCode(root);
            if (!string.IsNullOrWhiteSpace(code))
            {
                OpenNodeCode = code.Substring(0, code.Length - 1);
            }
            if(id == -1 && root.Children.Any())
            {
                SelectedReportID = root.Children.First().Id;
            }
            else
            {
                SelectedReportID = id;
            }
        }

        /// <summary>
        /// Builds the folder code.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns></returns>
        private static string BuildFolderCode(MenuNode node)
        {
            var subFolder = string.Empty;
            if (node.Children != null && node.Children.Any())
            {
                foreach (var menuNode in node.Children)
                {
                    subFolder += BuildFolderCode(menuNode);
                }
                subFolder += "'" + node.Id + "',";
            }
            return subFolder;
        }
    }
}