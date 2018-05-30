using System.Linq;
using VAV.Model.Data;

namespace VAV.Web.ViewModels.Home
{
    /// <summary>
    /// Index View Model
    /// </summary>
    public class IndexViewModel
    {
        /// <summary>
        /// Gets or sets the menu tree.
        /// </summary>
        /// <value>The menu tree.</value>
        public MenuNode MenuTree { get; private set; }

        /// <summary>
        /// Gets or sets the current tree view model.
        /// </summary>
        /// <value>The current tree view model.</value>
        public TreeViewModel CurrentTreeViewModel { get; private set; }

        /// <summary>
        /// Gets or sets the first report.
        /// </summary>
        /// <value>The first report.</value>
        public MenuNode DefaultReport { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndexViewModel"/> class.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <param name="defaultReport">The default report.</param>
        public IndexViewModel (MenuNode tree, MenuNode defaultReport)
        {
            MenuTree = tree;
            DefaultReport = defaultReport;
            CurrentTreeViewModel = new TreeViewModel(MenuTree.Children.First(), defaultReport.Id); 
        }

        
    }
}