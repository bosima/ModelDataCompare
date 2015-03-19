using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FireflySoft.ModelDataCompare.Core
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ValueDescriptionAttribute : Attribute
    {
        /// <summary>
        /// 枚举类型
        /// </summary>
        public Type EnumType { get; set; }

        /// <summary>
        /// 获取文本描述的方法
        /// </summary>
        public string QueryMethod { get; set; }

        /// <summary>
        /// 文本描述
        /// </summary>
        public string Describtion { get; set; }
    }
}
