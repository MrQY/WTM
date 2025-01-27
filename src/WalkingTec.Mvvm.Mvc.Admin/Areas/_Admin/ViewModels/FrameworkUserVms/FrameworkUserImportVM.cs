using System.Collections.Generic;
using WalkingTec.Mvvm.Core;

namespace WalkingTec.Mvvm.Mvc.Admin.ViewModels.FrameworkUserVms
{
    public class FrameworkUserTemplateVM : BaseTemplateVM
    {
        public ExcelPropety c1 = ExcelPropety.CreateProperty<FrameworkUserBase>(x => x.ITCode);
        public ExcelPropety c2 = ExcelPropety.CreateProperty<FrameworkUserBase>(x => x.Password);
        public ExcelPropety c3 = ExcelPropety.CreateProperty<FrameworkUserBase>(x => x.Name);
        public ExcelPropety c5 = ExcelPropety.CreateProperty<FrameworkUserBase>(x => x.Sex);
        public ExcelPropety c6 = ExcelPropety.CreateProperty<FrameworkUserBase>(x => x.CellPhone);

        protected override void InitVM()
        {
        }

    }

    public class FrameworkUserImportVM : BaseImportVM<FrameworkUserTemplateVM, FrameworkUserBase>
    {
        public override bool BatchSaveData()
        {
            SetEntityList();
            foreach (var item in EntityList)
            {
                item.IsValid = true;
            }
            return base.BatchSaveData();
        }
    }

}
