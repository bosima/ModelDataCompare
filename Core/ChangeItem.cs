using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FireflySoft.ModelDataCompare.Core
{
    /// <summary>
    /// 复杂数据类型变更项类：如泛型集合、具有多个属性的类型等。
    /// </summary>
    public class ChangeItem
    {
        private IList<ChangeItem> _sub;

        /// <summary>
        /// 变更项所属类型的名称
        /// </summary>
        public string OfTypeName { get; set; }

        /// <summary>
        /// 变更项类型的名称
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// 变更项名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 变更项描述
        /// </summary>
        public string ReadableName { get; set; }

        /// <summary>
        /// 修改前
        /// </summary>
        public string OldValue { get; set; }

        /// <summary>
        /// 修改后
        /// </summary>
        public string NewValue { get; set; }

        /// <summary>
        /// 如果是一个复杂类型，则可能具有下级变更项
        /// </summary>
        public IList<ChangeItem> Sub
        {
            get
            {
                if (_sub == null)
                {
                    _sub = new List<ChangeItem>();
                }

                return _sub;
            }

            set
            {
                _sub = value;
            }
        }
    }
}
