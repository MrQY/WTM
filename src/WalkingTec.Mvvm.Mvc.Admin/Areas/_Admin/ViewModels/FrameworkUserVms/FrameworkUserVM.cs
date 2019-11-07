using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WalkingTec.Mvvm.Core;
using WalkingTec.Mvvm.Core.Extensions;

namespace WalkingTec.Mvvm.Mvc.Admin.ViewModels.FrameworkUserVms
{
    public class FrameworkUserVM : BaseCRUDVM<FrameworkUserBase>
    {
        [JsonIgnore]
        public List<ComboSelectListItem> AllRoles { get; set; }
        [Display(Name = "Role")]
        public List<Guid> SelectedRolesIDs { get; set; }

        public FrameworkUserVM()
        {
            SetInclude(x => x.UserRoles);
        }

        /// <summary>
        /// 验证重复字段
        /// </summary>
        /// <returns></returns>
        public override DuplicatedInfo<FrameworkUserBase> SetDuplicatedCheck()
        {
            var rv = CreateFieldsInfo(SimpleField(x => x.ITCode));
            return rv;
        }

        protected override void InitVM()
        {
            if (ControllerName.Contains("WalkingTec.Mvvm.Mvc.Admin.Controllers"))
            {
                SelectedRolesIDs = Entity.UserRoles.Select(x => x.RoleId).ToList();
                AllRoles = DC.Set<FrameworkRole>().GetSelectListItems(LoginUserInfo.DataPrivileges, null, y => y.RoleName);
            }

        }

        protected override void ReInitVM()
        {
            if (ControllerName.Contains("WalkingTec.Mvvm.Mvc.Admin.Controllers"))
            {
                AllRoles = DC.Set<FrameworkRole>().GetSelectListItems(LoginUserInfo.DataPrivileges, null, y => y.RoleName);
            }
        }

        public override async Task DoAddAsync()
        {
            if (ControllerName.Contains("WalkingTec.Mvvm.Mvc.Admin.Controllers"))
            {
                Entity.UserRoles = new List<FrameworkUserRole>();
                if (SelectedRolesIDs != null)
                {
                    foreach (var roleid in SelectedRolesIDs)
                    {
                        Entity.UserRoles.Add(new FrameworkUserRole { RoleId = roleid });
                    }
                }
            }
            Entity.IsValid = true;
            Entity.Password = Utils.GetMD5String(Entity.Password);
            await base.DoAddAsync();
        }

        public override async Task DoEditAsync(bool updateAllFields = false)
        {
            if (ControllerName.Contains("WalkingTec.Mvvm.Mvc.Admin.Controllers"))
            {
                Entity.UserRoles = new List<FrameworkUserRole>();
                if (SelectedRolesIDs != null)
                {
                    SelectedRolesIDs.ForEach(x => Entity.UserRoles.Add(new FrameworkUserRole { ID = Guid.NewGuid(), UserId = Entity.ID, RoleId = x }));
                }
            }
            await base.DoEditAsync(updateAllFields);
            await LoginUserInfo.RemoveUserCache(Entity.ID.ToString());
        }

        public override async Task DoDeleteAsync()
        {
            await base.DoDeleteAsync();
        }

        public void ChangePassword()
        {
            Entity.Password = Utils.GetMD5String(Entity.Password);
            DC.UpdateProperty(Entity, x => x.Password);
            DC.SaveChanges();
        }
    }
}
