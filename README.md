# ModelDataCompare
ModelDataCompare provides a set of methods for comparison between two instances of the same types of data changes.

ModelDataCompare 提供了一组方法用于比较同一个类型的两个实例之间的数据差异。
常用于计算某种业务数据的变更，比如“用户信息”修改后计算修改了哪些用户的属性、属性修改前后的新旧值。

数据变更的结果通过ChangeItem定义：

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
        /// 变更项业务人员可读名称
        /// </summary>
        public string ReadableName { get; set; }

        /// <summary>
        /// 修改前值
        /// </summary>
        public string OldValue { get; set; }

        /// <summary>
        /// 修改后值
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

ModelDataCompare 不仅仅是简单的数据比对，还提供了若干增强支持：

1、嵌套类型比较：

如下定义的模型类，其中的User属性也是一个复杂数据类型，ModelDataCompare 可以对User中的每一个属性进行比较。

    /// <summary>
    /// 嵌套类型
    /// </summary>
    public class NestedModel
    {
        public SimpleModel User
        {
            get;
            set;
        }

        public string DeptName
        {
            get;
            set;
        }

        public int? Year
        {
            get;
            set;
        }

        public string Address
        {
            get;
            set;
        }
    }
    
2、集合类型比较：

ModelDataCompare 可以比较集合中对应索引位置的同一个类型的实例，还支持集合元素数量不相等的比较。

            SimpleModel[] oldArray = new SimpleModel[]{
                new SimpleModel()
                {
                    Name = "张三",
                    Age = 24,
                    Gender = 1,
                    City = 1,
                    Province = 1
                },

               new SimpleModel()
                {
                    Name = "李四",
                    Age = 25,
                    Gender = 1,
                    City = 2,
                }
            };

            SimpleModel[] newArray = new SimpleModel[]{
                new SimpleModel()
                {
                    Name = "张三",
                    Age = 23,
                    Gender = 1,
                    City = 1,
                    Province = 1
                },

               new SimpleModel()
                {
                    Name = "李四",
                    Age = 24,
                    Gender = 1,
                    City = 3,
                }
            };

            var changeItem = Comparer.Compare(oldArray, newArray, null, null);

3、数据属性友好名称

程序中的属性一般使用英文名称，对于业务系统的使用用户来说可读性较差，ModelDataCompare 支持为属性提供一个友好的可读名称。

            var oldModel = new SimpleModel()
            {
                Name = "张三",
                Age = 24,
                Gender = 1,
                City = 1,
                Province = 1,
                ID = Guid.NewGuid()
            };

            var newModel = new SimpleModel()
            {
                Name = "李四",
                Age = 25,
                Gender = 0,
                City = 2,
                ID = Guid.NewGuid()
            };

            var propertyReadableNameDictionary = new Dictionary<string, string>();
            propertyReadableNameDictionary.Add("Name", "姓名");
            propertyReadableNameDictionary.Add("Age", "年龄");
            propertyReadableNameDictionary.Add("Gender", "性别");
            propertyReadableNameDictionary.Add("City", "城市");
            propertyReadableNameDictionary.Add("Province", "省");

            var changeItem = Comparer.Compare(oldModel, newModel, new string[] { "ID" }, propertyReadableNameDictionary);
            changeItem.ReadableName = "简单类型";

            Assert.AreEqual("姓名", changeItem.Sub.Where(i => i.Name == "Name").FirstOrDefault().ReadableName);

4、数据属性值友好描述

属性对应的值可能只是一个系统内部使用的编号，对于业务系统的使用用户来说看不懂，ModelDataCompare 支持通过特性的方式转换相应的内部值为一个用户可读的值。

目前特性支持两种方式：枚举、方法。

    public class SimpleModel
    {
        public Guid ID { get; set; }

        public string Name { get; set; }

        public int Age { get; set; }

        [ValueDescription(EnumType = typeof(EnumGender))]
        public int? Gender { get; set; }

        [ValueDescription(QueryMethod = "FireflySoft.ModelDataCompare.UnitTest,FireflySoft.ModelDataCompare.UnitTest.CityBLL,GetModel,CityName")]
        public int? City { get; set; }

        public int? Province { get; set; }
    }
    
        public enum EnumGender
    {
        [System.ComponentModel.Description("先生")]
        Male = 1,
        [System.ComponentModel.Description("女士")]
        Female = 0
    }

    public class CityBLL
    {
        public CityModel GetModel(int cityID)
        {
            var model = new CityModel();
            model.CityID = 1;
            model.CityName = "CityName";
            return model;
        }
    }

    public class CityModel
    {
        public int CityID { get; set; }

        public string CityName { get; set; }
    }
    
ModelDataCompare 内部采用了反射的方式用于获取类型的属性、获取属性的特性，以及获取相关方法和属性的值，这对性能会造成一定的影响。为了解决这个问题，ModelDataCompare 内部对相关反射获取到的数据进行了缓存，可以提升一些性能。
