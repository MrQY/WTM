using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;
using WalkingTec.Mvvm.Core;
using WalkingTec.Mvvm.Core.Extensions;
using WalkingTec.Mvvm.Demo.Models;
using WalkingTec.Mvvm.Mvc.Admin.ViewModels.FrameworkRoleVMs;

namespace WalkingTec.Mvvm.Demo.ViewModels.MyUserVMs
{
    public class MyUserVM : BaseCRUDVM<MyUser>
    {
        public List<ComboSelectListItem> AllUserRoless { get; set; }
        [Display(Name = "角色")]
        public List<Guid> SelectedUserRolesIDs { get; set; }

        [JsonIgnore]
        public FrameworkRoleListVM RoleListVM { get; set; }

        public MyUserVM()
        {
            SetInclude(x => x.UserRoles);
            RoleListVM = new FrameworkRoleListVM();
        }

        protected override void InitVM()
        {
            AllUserRoless = DC.Set<FrameworkRole>().GetSelectListItems(LoginUserInfo.DataPrivileges, null, y => y.RoleName);
            SelectedUserRolesIDs = Entity.UserRoles.Select(x => x.RoleId).ToList();
        }

        public override void DoAdd()
        {
            if (SelectedUserRolesIDs != null)
            {
                foreach (var id in SelectedUserRolesIDs)
                {
                    Entity.UserRoles.Add(new FrameworkUserRole { RoleId = id });
                }
            }

            Entity.IsValid = true;
            Entity.Password = Utils.GetMD5String(Entity.Password);

            base.DoAdd();
        }

        public override void DoEdit(bool updateAllFields = false)
        {
            if (SelectedUserRolesIDs == null || SelectedUserRolesIDs.Count == 0)
            {
                FC.Add("Entity.SelectedUserRolesIDs.DONOTUSECLEAR", "true");
            }
            else
            {
                Entity.UserRoles = new List<FrameworkUserRole>();
                SelectedUserRolesIDs.ForEach(x => Entity.UserRoles.Add(new FrameworkUserRole { ID = Guid.NewGuid(), RoleId = x }));
            }

            base.DoEdit(updateAllFields);
        }

        public override void DoDelete()
        {
            base.DoDelete();
        }
    }
}
