using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FireflySoft.ModelDataCompare.Core
{
    /// <summary>
    /// 模型数据比较器
    /// </summary>
    public class Comparer
    {
        /// <summary>
        /// 值描述查询对象字典
        /// </summary>
        private static readonly Dictionary<string, object> valueDescriptionQueryObjects = new Dictionary<string, object>();

        /// <summary>
        /// 类型属性字典
        /// </summary>
        private static readonly Dictionary<string, PropertyInfo[]> typeProperties = new Dictionary<string, PropertyInfo[]>();

        /// <summary>
        /// 属性ValueDescriptionAttribute字典
        /// </summary>
        private static readonly Dictionary<string, ValueDescriptionAttribute[]> propertyValueDescriptionAttributes = new Dictionary<string, ValueDescriptionAttribute[]>();

        /// <summary>
        /// 比较模型数据，获取变更项
        /// </summary>
        /// <param name="oldModel">原数据</param>
        /// <param name="newModel">新数据</param>
        /// <param name="excludeProperties">要排除比较的属性数组，格式：属性名称[,属性所属类型全称]，如果只有属性名称，则不区分属性所属类型。</param>
        /// <param name="propertyReadableNames">属性描述字典，Key格式：属性名称[,属性所属类型全称]，优先带类型名称的严格匹配。</param>
        /// <returns></returns>
        public static ChangeItem Compare(object oldModel, object newModel, string[] excludeProperties, Dictionary<string, string> propertyReadableNames)
        {
            // 获取要比较的数据的类型
            Type type = GetModelType(oldModel, newModel);

            if (type == null)
            {
                return null;
            }

            // 初始化变更项
            ChangeItem changeItem = new ChangeItem();
            changeItem.TypeName = type.FullName;
            changeItem.Name = type.Name;

            // 如果是泛型或者数组，则按照集合进行处理
            if (type.IsGenericType || type.IsArray)
            {
                // 转换要比较的数据为集合形式
                List<object> newList;
                List<object> oldList;
                ConvertObjectToList(oldModel, newModel, out newList, out oldList);

                // 获取集合中的变更项目
                var changeItemList = CompareList(newList, oldList, excludeProperties, propertyReadableNames);
                if (changeItemList != null)
                {
                    changeItem.Sub = changeItemList;
                }
            }
            else  // 非集合类型的比较
            {
                var changeList = CompareObject(oldModel, newModel, excludeProperties, propertyReadableNames);
                changeItem.Sub = changeList;
            }

            return changeItem;
        }

        /// <summary>
        /// 非集合类型的比较
        /// </summary>
        /// <param name="oldModel"></param>
        /// <param name="newModel"></param>
        /// <param name="excludeProperties"></param>
        /// <param name="propertyReadableNames"></param>
        public static List<ChangeItem> CompareObject(object oldModel, object newModel, string[] excludeProperties, Dictionary<string, string> propertyReadableNames)
        {
            List<ChangeItem> changeItemList = new List<ChangeItem>();

            // 获取要比较的数据的类型
            Type type = GetModelType(oldModel, newModel);

            // TODO:缓存属性集合
            PropertyInfo[] properties = GetTypeProperties(type);
            foreach (PropertyInfo property in properties)
            {
                // 属性名称
                string propertyName = property.Name;

                // 需要排除的比较项
                if (CheckIsExcludeItem(propertyName, type.FullName, excludeProperties))
                {
                    continue;
                }

                // 获取属性对应的新旧值
                object oldValue = null;
                object newValue = null;

                if (oldModel != null)
                {
                    oldValue = property.GetValue(oldModel, null);
                }

                if (newModel != null)
                {
                    newValue = property.GetValue(newModel, null);
                }

                oldValue = oldValue ?? string.Empty;
                newValue = newValue ?? string.Empty;

                var changeItem = CompareProperty(property, oldValue, newValue, excludeProperties, propertyReadableNames);
                if (changeItem != null)
                {
                    changeItemList.Add(changeItem);
                }
            }

            return changeItemList;
        }

        /// <summary>
        /// 比较属性
        /// </summary>
        /// <param name="property"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        /// <param name="excludeProperties"></param>
        /// <param name="propertyReadableNames"></param>
        public static ChangeItem CompareProperty(PropertyInfo property, object oldValue, object newValue, string[] excludeProperties, Dictionary<string, string> propertyReadableNames)
        {
            // 简单数据类型的处理
            if (property.PropertyType.IsValueType || property.PropertyType.FullName == "System.String")
            {
                var changeItem = CompareValueTypeProperty(property, oldValue, newValue);

                if (changeItem != null)
                {
                    changeItem.ReadableName = GetPropertyReadableName(changeItem.OfTypeName, property.Name, propertyReadableNames);
                    return changeItem;
                }
            }
            else // 复杂数据类型的处理
            {
                var changeItem = Compare(oldValue, newValue, excludeProperties, propertyReadableNames);

                if (changeItem != null)
                {
                    changeItem.OfTypeName = property.DeclaringType.FullName;
                    changeItem.ReadableName = GetPropertyReadableName(property.PropertyType.FullName, property.Name, propertyReadableNames);
                    changeItem.Name = property.Name;
                    return changeItem;
                }
            }

            return null;
        }

        /// <summary>
        /// 比较列表格式的模型数据，获取变更项
        /// </summary>
        /// <param name="newList"></param>
        /// <param name="oldList"></param>
        /// <param name="excludeProperties"></param>
        /// <param name="propertyReadableNames"></param>
        public static List<ChangeItem> CompareList(List<object> newList, List<object> oldList, string[] excludeProperties, Dictionary<string, string> propertyReadableNames)
        {
            // 获取新旧集合中项目的数量
            int newListCount = newList.Count;
            int oldListCount = oldList.Count;

            // 比较新旧集合的大小，以大的为下一步进行遍历比较的基数
            int loopCount = 0;
            if (oldListCount >= newListCount)
            {
                loopCount = oldListCount;
            }
            else
            {
                loopCount = newListCount;
            }

            // 遍历比较集合中的元素
            if (loopCount > 0)
            {
                List<ChangeItem> changeItemList = new List<ChangeItem>();

                for (int i = 0; i < loopCount; i++)
                {
                    object oldObj = null;
                    object newObj = null;

                    if (oldListCount >= i + 1)
                    {
                        oldObj = oldList[i];
                    }

                    if (newList.Count >= i + 1)
                    {
                        newObj = newList[i];
                    }

                    // 比较新旧两个值
                    var changeItem = Compare(oldObj, newObj, excludeProperties, propertyReadableNames);
                    changeItem.ReadableName = "第" + i + "项";

                    changeItemList.Add(changeItem);
                }

                return changeItemList;
            }

            return null;
        }

        /// <summary>
        /// 填充变更项的描述信息
        /// </summary>
        /// <param name="changeItem"></param>
        /// <param name="propertyReadableNames"></param>
        public static void FillChangeItemDescription(ChangeItem changeItem, Dictionary<string, string> propertyReadableNames)
        {
            if (changeItem != null)
            {
                changeItem.ReadableName = GetPropertyReadableName(changeItem.OfTypeName, changeItem.Name, propertyReadableNames);

                if (changeItem.Sub != null && changeItem.Sub.Count > 0)
                {
                    foreach (var item in changeItem.Sub)
                    {
                        FillChangeItemDescription(item, propertyReadableNames);
                    }
                }
            }
        }

        /// <summary>
        /// 获取变更简介
        /// </summary>
        /// <param name="changeItem"></param>
        /// <returns></returns>
        public static string GetChangeItemSummary(ChangeItem changeItem, int maxLength = 180)
        {
            string summary = string.Empty;

            if (changeItem != null && changeItem.Sub != null && changeItem.Sub.Count > 0)
            {
                int i = 0;
                foreach (var item in changeItem.Sub)
                {
                    if (item.Name != item.ReadableName)
                    {
                        if (i >= 5)
                        {
                            break;
                        }

                        summary += "[" + item.ReadableName + "]由\"" + item.OldValue + "\"变更为\"" + item.NewValue + "\"，";

                        i++;
                    }
                }
            }

            if (summary.Length > maxLength)
            {
                summary = summary.Substring(0, maxLength) + "...";
            }

            return summary;
        }

        /// <summary>
        /// 比较值类型属性
        /// </summary>
        /// <param name="property"></param>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        /// <param name="propertyDescriptions"></param>
        /// <returns></returns>
        private static ChangeItem CompareValueTypeProperty(PropertyInfo property, object oldValue, object newValue)
        {
            // 如果新旧值不同，则添加到变更集合
            if (!oldValue.Equals(newValue))
            {
                var changeItem = new ChangeItem();
                changeItem.OfTypeName = property.DeclaringType.FullName; // 声明该属性的类的全名称
                changeItem.TypeName = property.PropertyType.FullName; // 该属性的类的全名称
                changeItem.Name = property.Name; // 属性的名称

                // TODO:不同数据类型需要执行不同的ToString()
                changeItem.OldValue = oldValue != null ? oldValue.ToString() : string.Empty;
                changeItem.NewValue = newValue != null ? newValue.ToString() : string.Empty;

                // 如果属性设置了ValueDescriptionAttribute，则需要进一步获取Value值
                var valueDescAttrs = GetPropertyValueDescriptionAttribute(property);

                if (valueDescAttrs != null && valueDescAttrs.Length > 0)
                {
                    if (!string.IsNullOrEmpty(changeItem.NewValue))
                    {
                        changeItem.NewValue = GetValueDescription(valueDescAttrs, changeItem.NewValue, property.PropertyType.FullName);
                    }

                    if (!string.IsNullOrEmpty(changeItem.OldValue))
                    {
                        changeItem.OldValue = GetValueDescription(valueDescAttrs, changeItem.OldValue, property.PropertyType.FullName);
                    }
                }

                return changeItem;
            }

            return null;
        }

        /// <summary>
        /// 转换要比较的数据对象为集合
        /// </summary>
        /// <param name="oldModel"></param>
        /// <param name="newModel"></param>
        /// <param name="newList"></param>
        /// <param name="oldList"></param>
        private static void ConvertObjectToList(object oldModel, object newModel, out List<object> newList, out List<object> oldList)
        {
            newList = new List<object>();
            oldList = new List<object>();

            if (newModel != null)
            {
                var newModelList = newModel as System.Collections.ICollection;
                foreach (var newObj in newModelList)
                {
                    newList.Add(newObj);
                }
            }

            if (oldModel != null)
            {
                var oldModelList = oldModel as System.Collections.ICollection;
                foreach (var oldObj in oldModelList)
                {
                    oldList.Add(oldObj);
                }
            }
        }

        /// <summary>
        /// 获取要比较的实体的数据类型
        /// </summary>
        /// <param name="oldModel"></param>
        /// <param name="newModel"></param>
        /// <returns></returns>
        private static Type GetModelType(object oldModel, object newModel)
        {
            Type type;
            if (oldModel != null)
            {
                type = oldModel.GetType();
            }
            else
            {
                type = newModel.GetType();
            }
            return type;
        }

        /// <summary>
        /// 获取类型的属性集合
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static PropertyInfo[] GetTypeProperties(Type type)
        {
            if (typeProperties.ContainsKey(type.FullName))
            {
                return typeProperties[type.FullName];
            }
            else
            {
                var properties = type.GetProperties();
                typeProperties.Add(type.FullName, properties);

                return properties;
            }
        }

        /// <summary>
        /// 获取属性对应的ValueDescriptionAttribute数组
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        private static ValueDescriptionAttribute[] GetPropertyValueDescriptionAttribute(PropertyInfo property)
        {
            var key = property.DeclaringType.FullName + "," + property.Name;

            if (propertyValueDescriptionAttributes.ContainsKey(key))
            {
                return propertyValueDescriptionAttributes[key];
            }
            else
            {
                Type valueDescription = typeof(ValueDescriptionAttribute);
                var valueDescAttrs = (ValueDescriptionAttribute[])property.GetCustomAttributes(valueDescription, false);
                propertyValueDescriptionAttributes.Add(key, valueDescAttrs);

                return valueDescAttrs;
            }
        }

        /// <summary>
        /// 通过ValueDescriptionAttribute获取值对应的可读描述信息
        /// </summary>
        /// <param name="valueDescAttrs"></param>
        /// <param name="value"></param>
        /// <param name="propertyTypeName"></param>
        /// <returns></returns>
        private static string GetValueDescription(ValueDescriptionAttribute[] valueDescAttrs, string value, string propertyTypeName)
        {
            if (valueDescAttrs[0].EnumType != null)
            {
                return EnumUtility.GetEnumDescription(int.Parse(value), valueDescAttrs[0].EnumType);
            }
            else
            {
                if (!string.IsNullOrEmpty(valueDescAttrs[0].QueryMethod))
                {
                    string[] queryMethods = valueDescAttrs[0].QueryMethod.Split(',');

                    // 反射获取描述信息
                    object queryObject = null;
                    if (valueDescriptionQueryObjects.ContainsKey(queryMethods[1]))
                    {
                        queryObject = valueDescriptionQueryObjects[queryMethods[1]];
                    }
                    else
                    {
                        queryObject = Activator.CreateInstance(queryMethods[0], queryMethods[1]).Unwrap();
                        valueDescriptionQueryObjects.Add(queryMethods[1], queryObject);
                    }

                    Type queryType = queryObject.GetType();

                    object model = null;
                    if (propertyTypeName.Contains("System.Int32"))
                    {
                        MethodInfo queryMethod = queryType.GetMethod(queryMethods[2], new Type[] { typeof(Int32) });
                        var paras = new object[] { int.Parse(value) };
                        model = queryMethod.Invoke(queryObject, paras);
                    }
                    else
                    {
                        MethodInfo queryMethod = queryType.GetMethod(queryMethods[2], new Type[] { typeof(String) });
                        var paras = new object[] { value };
                        model = queryMethod.Invoke(queryObject, paras);
                    }

                    if (model != null)
                    {
                        var modelType = model.GetType();
                        PropertyInfo[] properties = modelType.GetProperties();

                        foreach (var property in properties)
                        {
                            if (property.Name == queryMethods[3])
                            {
                                var propertyValue = property.GetValue(model, null);

                                if (propertyValue != null)
                                {
                                    return propertyValue.ToString();
                                }
                            }
                        }
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 获取属性的可读名称
        /// </summary>
        /// <param name="classTypeName"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static string GetPropertyReadableName(string classTypeName, string propertyName, Dictionary<string, string> propertyDescriptions)
        {
            if (propertyDescriptions != null)
            {
                string description = string.Empty;

                // 优先严格匹配
                if (propertyDescriptions.TryGetValue(propertyName + "," + classTypeName, out description))
                {
                    return description;
                }

                if (propertyDescriptions.TryGetValue(propertyName, out description))
                {
                    return description;
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// 检查属性是否排除比较的项目
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        /// <param name="typeFullName">声明该属性的类的全名称</param>
        /// <param name="excludeProperties">需要排除的属性名称集合</param>
        /// <returns></returns>
        private static bool CheckIsExcludeItem(string propertyName, string typeFullName, string[] excludeProperties)
        {
            if (excludeProperties != null)
            {
                foreach (var exclude in excludeProperties)
                {
                    if (exclude.IndexOf(',') > 0)
                    {
                        string[] excludeArr = exclude.Split(',');
                        if (excludeArr.Length > 1)
                        {
                            if (excludeArr[0] == propertyName && excludeArr[1] == typeFullName)
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (exclude == propertyName)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
