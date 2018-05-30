using System.Collections.Generic;
using System.Linq;
using VAV.Model.Data.CnE.GDT;

namespace VAV.Web.ViewModels.GDT
{
    public class GdtOutputViewModel : GdtBaseViewModel
    {
        public List<GDTQueryCondition> FilterList { get; set; }

        public override void Initialization()
        {
            var currentNode = GdtService[ReportId].FirstOrDefault(t => t.ItemID == ItemId);
            FilterList = GdtService.GetQueryConditions(ItemId, 0, currentNode.TableName1, currentNode.TableFilter1);
        }
    }
}
