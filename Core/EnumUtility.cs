using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FireflySoft.ModelDataCompare.Core
{
    /// <summary>
    /// 该类提供用于枚举类型处理的相关静态方法
    /// </summary>
    public class EnumUtility
    {
        /// <summary>
        /// 初始化EnumUtility类的一个实例
        /// </summary>
        private EnumUtility()
        {
        }

        /// <summary>
        /// 获得某个枚举类型的绑定列表
        /// </summary>
        /// <param name="enumType">枚举的类型，例如：typeof(Sex)</param>
        /// <returns>
        /// 返回一个DataTable
        /// DataTable 有两列：
        /// "Text"    : System.String;
        /// "Value"   : System.String
        /// </returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <example>
        /// public enum Sex
        /// {
        ///     [Description("男")]
        ///     Male = 1,
        ///     [Description("女")]
        ///     Female = 2
        /// }
        /// 
        /// DataTable dt = VeryCodes.Data.EnumUtility.GetEnumList(typeof(Sex));
        /// 
        /// string result = string.Empty;
        /// foreach (DataRow dr in dt.Rows)
        /// {
        ///     result += dr["Text"] + "," + dr["Value"] + "<br/>";
        /// }
        /// </example>
        public static DataTable GetEnumList(Type enumType)
        {
            if (!enumType.IsEnum)
            {
                // 不是枚举的要报错
                throw new InvalidOperationException("参数不是枚举类型。");
            }

            // 建立DataTable的列信息
            DataTable dt = new DataTable();

            try
            {
                dt.Columns.Add("Text", typeof(string));
                dt.Columns.Add("Value", typeof(string));

                // 获得特性Description的类型信息
                Type typeDescription = typeof(DescriptionAttribute);

                // 获得枚举的字段信息（因为枚举的值实际上是一个static的字段的值）
                System.Reflection.FieldInfo[] fields = enumType.GetFields();

                // 检索所有字段

                foreach (FieldInfo field in fields)
                {
                    // 过滤掉一个不是枚举值的，记录的是枚举的源类型
                    if (field.FieldType.IsEnum == true)
                    {
                        DataRow dr = dt.NewRow();

                        // 通过字段的名字得到枚举的值
                        // 枚举的值如果是long的话，ToChar会有问题，但这个不在本文的讨论范围之内
                        dr["Value"] = Convert.ToString((int)enumType.InvokeMember(field.Name, BindingFlags.GetField, null, null, null));

                        // 获得这个字段的所有自定义特性，这里只查找Description特性
                        object[] arr = field.GetCustomAttributes(typeDescription, true);
                        if (arr.Length > 0)
                        {
                            // 因为Description这个自定义特性是不允许重复的，所以我们只取第一个就可以了！
                            DescriptionAttribute aa = (DescriptionAttribute)arr[0];
                            // 获得特性的描述值，也就是‘男’‘女’等中文描述
                            dr["Text"] = aa.Description;
                        }
                        else
                        {
                            // 如果没有特性描述（-_-!忘记写了吧~）那么就显示英文的字段名
                            dr["Text"] = field.Name;
                        }
                        dt.Rows.Add(dr);
                    }
                }
            }
            catch
            {
            }

            return dt;
        }

        /// <summary>
        /// 获取指定枚举值的描述文本
        /// </summary>
        /// <param name="obj">枚举值</param>
        /// <param name="enumType">枚举类型</param>
        /// <returns>字符串，即Description属性的值</returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.TypeLoadException"></exception>
        /// <example>
        /// public enum Sex
        /// {
        ///     [Description("男")]
        ///     Male = 1,
        ///     [Description("女")]
        ///     Female = 2
        /// }
        /// 
        /// string result = string.Empty;
        /// 
        /// try
        /// {
        ///     result = VeryCodes.Data.EnumUtility.GetEnumDescription(Sex.Female, typeof(Sex));
        /// }
        /// catch (InvalidOperationException ex1)
        /// {
        ///     result = ex1.Message;
        /// }
        /// catch (TypeLoadException ex2)
        /// {
        ///     result = ex2.Message;
        /// }
        /// </example>
        public static string GetEnumDescription(object obj, Type enumType)
        {
            // 获取字段信息
            System.Reflection.FieldInfo[] ms = enumType.GetFields();

            Type t = enumType;
            foreach (System.Reflection.FieldInfo f in ms)
            {
                // 判断名称是否相等
                if (f.Name != obj.ToString())
                {
                    continue;
                }

                // 反射出自定义属性
                foreach (Attribute attr in f.GetCustomAttributes(true))
                {
                    // 类型转换找到一个Description，用Description作为成员名称
                    System.ComponentModel.DescriptionAttribute dscript = attr as System.ComponentModel.DescriptionAttribute;
                    if (dscript != null)
                    {
                        return dscript.Description;
                    }
                }
            }

            // 如果没有检测到合适的注释，则用默认名称
            return obj.ToString();
        }

        public static string GetEnumDescription(int value, Type enumType)
        {
            // 获取字段信息
            System.Reflection.FieldInfo[] ms = enumType.GetFields();

            Type t = enumType;
            foreach (System.Reflection.FieldInfo f in ms)
            {
                if (f.FieldType.IsEnum == true)
                {
                    var tValue = (int)enumType.InvokeMember(f.Name, BindingFlags.GetField, null, null, null);

                    // 判断名称是否相等
                    if (tValue != value)
                    {
                        continue;
                    }

                    // 反射出自定义属性
                    foreach (Attribute attr in f.GetCustomAttributes(true))
                    {
                        // 类型转换找到一个Description，用Description作为成员名称
                        System.ComponentModel.DescriptionAttribute dscript = attr as System.ComponentModel.DescriptionAttribute;
                        if (dscript != null)
                        {
                            return dscript.Description;
                        }
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取指定枚举值的描述文本
        /// </summary>
        /// <param name="obj">枚举值</param>
        /// <returns>字符串，即Description属性的值</returns>
        /// <exception cref="System.InvalidOperationException"></exception>
        /// <exception cref="System.TypeLoadException"></exception>
        /// <example>
        /// public enum Sex
        /// {
        ///     [Description("男")]
        ///     Male = 1,
        ///     [Description("女")]
        ///     Female = 2
        /// }
        /// 
        /// string result = string.Empty;
        /// 
        /// try
        /// {
        ///     result = VeryCodes.Data.EnumUtility.GetEnumDescription(Sex.Female);
        /// }
        /// catch (InvalidOperationException ex1)
        /// {
        ///     result = ex1.Message;
        /// }
        /// catch (TypeLoadException ex2)
        /// {
        ///     result = ex2.Message;
        /// }
        /// </example>
        public static string GetEnumDescription(object obj)
        {
            return GetEnumDescription(obj, obj.GetType());
        }
    }
}
