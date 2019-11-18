using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WalkingTec.Mvvm.Core;
using WalkingTec.Mvvm.Core.Extensions;
using WalkingTec.Mvvm.Mvc.Admin.ViewModels.DataPrivilegeVMs;
using WalkingTec.Mvvm.Mvc.Admin.ViewModels.FrameworkMenuVMs;

namespace WalkingTec.Mvvm.Mvc.Admin.ViewModels.FrameworkRoleVMs
{
    public class FrameworkRoleVM : BaseCRUDVM<FrameworkRole>
    {
        [Required(ErrorMessage = "{0}required")]
        [StringLength(50, ErrorMessage = "{0}stringmax{1}")]
        [Display(Name = "TableName")]
        public string TableName { get; set; }
        public List<ComboSelectListItem> TableNames { get; set; }
        public DpListVM DpList { get; set; }

        [Display(Name = "AllDp")]
        public bool? IsAll { get; set; }

        [Display(Name = "AllowedDp")]
        public List<string> SelectedItemsID { get; set; }

        public FrameworkMenuListVM ListVM { get; set; }

        public FrameworkRoleVM()
        {
            DpList = new DpListVM();
            IsAll = false;
            ListVM = new FrameworkMenuListVM();
        }
        public override DuplicatedInfo<FrameworkRole> SetDuplicatedCheck()
        {
            var rv = CreateFieldsInfo(SimpleField(x => x.RoleName));
            rv.AddGroup(SimpleField(x => x.RoleCode));
            return rv;
        }

        protected override void InitVM()
        {
            TableNames = new List<ComboSelectListItem>();
            if (ControllerName.Contains("WalkingTec.Mvvm.Mvc.Admin.Controllers"))
            {
                TableNames = ConfigInfo.DataPrivilegeSettings.ToListItems(x => x.PrivillegeName, x => x.ModelName);
            }
            //var rids = DC.Set<DataPrivilege>().Where(x => x.TableName == Entity.TableName && x.RoleId == Entity.ID).Select(x => x.RelateId).ToList();

            ListVM.CopyContext(this);
            ListVM.Searcher.RoleID = Entity.ID;
        }
    }
}
