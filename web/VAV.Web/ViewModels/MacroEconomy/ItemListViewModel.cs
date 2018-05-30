using System.Collections.Generic;
using System.Web.Mvc;
using VAV.DAL.Services;
using VAV.Web.ViewModels.Report;
using VAV.Model.Data;


namespace VAV.Web.ViewModels.MacroEconomy
{
    public class ItemListViewModel : BaseReportViewModel
    {
        public int MenuNodeId { get; private set; }
        public List<MenuNode> ItemListLeft { get; private set; }
        public List<MenuNode> ItemListRight { get; private set; }

        public ItemListViewModel(int id, string userId)
            : base(id, userId)
        {
        }

        public override void Initialization()
        {
            var menuService = (MenuService)DependencyResolver.Current.GetService(typeof(MenuService));
            MenuNodeId = menuService.GetMenuNodeByReportId(ID).Id;
            ItemListLeft =  menuService.GetMenuNodeByReportIdAndStyle(ID, "left");
            ItemListRight = menuService.GetMenuNodeByReportIdAndStyle(ID, "right");
        }
    }
}