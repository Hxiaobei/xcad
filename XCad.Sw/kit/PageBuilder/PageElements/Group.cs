//*********************************************************************
//xCAD
//Copyright(C) 2024 Xarial Pty Limited
//Product URL: https://www.xcad.net
//License: https://xcad.xarial.com/license/
//*********************************************************************

using XCad.kit.PageBuilder.Base;
using XCad.UI.PropertyPage.Base;

namespace XCad.kit.PageBuilder.PageElements {
    public abstract class Group<TVal> : Control<TVal>, IGroup {
#pragma warning disable CS0067

        protected override event ControlValueChangedDelegate<TVal> ValueChanged;

#pragma warning restore CS0067

        protected Group(int id, object tag, IMetadata[] metadata) : base(id, tag, metadata) {
        }

        protected override TVal GetSpecificValue() => default(TVal);

        protected override void SetSpecificValue(TVal value) {
        }
    }

    public abstract class Group : Group<object> {
        protected Group(int id, object tag, IMetadata[] metadata) : base(id, tag, metadata) {
        }

        public override void Focus() {
        }
    }
}