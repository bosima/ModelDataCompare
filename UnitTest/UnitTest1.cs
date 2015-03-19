using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FireflySoft.ModelDataCompare.Core;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;

namespace FireflySoft.ModelDataCompare.UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        /// <summary>
        /// 简单类型
        /// </summary>
        [TestMethod]
        public void GetSimpleModelChange()
        {
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

            var descriptionDictionary = new Dictionary<string, string>();
            descriptionDictionary.Add("Name", "姓名");
            descriptionDictionary.Add("Age", "年龄");
            descriptionDictionary.Add("Gender", "性别");
            descriptionDictionary.Add("City", "城市");
            descriptionDictionary.Add("Province", "省");

            var changeItem = Comparer.Compare(oldModel, newModel, new string[] { "ID" }, descriptionDictionary);
            changeItem.ReadableName = "简单类型";

            Assert.AreEqual(5, changeItem.Sub.Count);
            Assert.AreEqual("先生", changeItem.Sub.Where(i => i.Name == "Gender").First().OldValue);
            Assert.AreEqual("女士", changeItem.Sub.Where(i => i.Name == "Gender").First().NewValue);
            Assert.AreEqual("张三", changeItem.Sub.Where(i => i.Name == "Name").First().OldValue);
            Assert.AreEqual("李四", changeItem.Sub.Where(i => i.Name == "Name").First().NewValue);
            Assert.IsNull(changeItem.Sub.Where(i => i.Name == "ID").FirstOrDefault());
            Assert.AreEqual("姓名", changeItem.Sub.Where(i => i.Name == "Name").FirstOrDefault().ReadableName);
        }

        /// <summary>
        /// 嵌套类型
        /// </summary>
        [TestMethod]
        public void GetNestedModelChange()
        {
            var oldModel = new NestedModel()
            {
                User = new SimpleModel()
                {
                    Name = "张三",
                    Age = 22,
                    Gender = 1,
                    City = 1,
                    Province = 1,
                    ID = Guid.NewGuid()
                },
                DeptName = "研发部",
                Year = 2015,
                Address = "北京"

            };

            var newModel = new NestedModel()
            {
                User = new SimpleModel()
                {
                    Name = "李四",
                    Age = 26,
                    Gender = 1,
                    City = 2,
                    Province = 2,
                    ID = Guid.NewGuid()
                },
                DeptName = "市场部",
                Year = 2014,
                Address = "北京"
            };

            var changeItem = Comparer.Compare(oldModel, newModel, null, null);
            changeItem.ReadableName = "嵌套类型";

            Assert.AreEqual(3, changeItem.Sub.Count);
            Assert.AreEqual(5, changeItem.Sub.Where(i => i.Name == "User").First().Sub.Count);
            Assert.AreEqual("26", changeItem.Sub.Where(i => i.Name == "User").First().Sub.Where(i => i.Name == "Age").First().NewValue);
        }

        /// <summary>
        /// 多级嵌套类型
        /// </summary>
        [TestMethod]
        public void GetMulitNestedModelChange()
        {
            var oldModel = new MulitNestedModel()
            {
                SubNested = new NestedModel()
                {
                    User = new SimpleModel()
                    {
                        Name = "张三",
                        Age = 22,
                        Gender = 1,
                        City = 1,
                        Province = 1,
                        ID = Guid.NewGuid()
                    },
                    DeptName = "研发部",
                    Year = 2015,
                    Address = "北京"
                },
                NestedName = "NestedName1"
            };

            var newModel = new MulitNestedModel()
            {
                SubNested = new NestedModel()
                {
                    User = new SimpleModel()
                    {
                        Name = "李四",
                        Age = 26,
                        Gender = 1,
                        City = 2,
                        Province = 2,
                        ID = Guid.NewGuid()
                    },
                    DeptName = "市场部",
                    Year = 2014,
                    Address = "北京"
                },
                NestedName = "NestedName2"
            };

            var changeItem = Comparer.Compare(oldModel, newModel, null, null);
            changeItem.ReadableName = "多级嵌套类型";

            Assert.AreEqual(2, changeItem.Sub.Count);
            Assert.AreEqual(3, changeItem.Sub.Where(i => i.Name == "SubNested").First().Sub.Count);
            Assert.AreEqual(5, changeItem.Sub.First(i => i.Name == "SubNested").Sub.First(i => i.Name == "User").Sub.Count);
            Assert.AreEqual("市场部", changeItem.Sub.First(i => i.Name == "SubNested").Sub.First(i => i.Name == "DeptName").NewValue);
        }

        /// <summary>
        /// 元素数量相等的数组
        /// </summary>
        [TestMethod]
        public void GetItemNumberEqualArrayChange()
        {
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
            changeItem.ReadableName = "元素数量相等的数组";

            Assert.AreEqual(1, changeItem.Sub[0].Sub.Count);
            Assert.AreEqual(2, changeItem.Sub[1].Sub.Count);
        }

        /// <summary>
        /// 元素数量不相等的数组
        /// </summary>
        [TestMethod]
        public void GetItemNumberNotEqualArrayChange()
        {
            SimpleModel[] oldArray = new SimpleModel[]{
                new SimpleModel()
                {
                    Name = "AA",
                    Age = 24,
                    Gender = 1,
                    City = 1,
                    Province = 1
                },

               new SimpleModel()
                {
                    Name = "BB",
                    Age = 25,
                    Gender = 1,
                    City = 2,
                }
            };

            SimpleModel[] newArray = new SimpleModel[]{
                new SimpleModel()
                {
                    Name = "AA",
                    Age = 23,
                    Gender = 1,
                    City = 1,
                    Province = 1
                },

                new SimpleModel()
                {
                    Name = "BB",
                    Age = 24,
                    Gender = 1,
                    City = 3,
                },
                 new SimpleModel()
                {
                    Name = "CC",
                    Age = 21,
                    Gender = 2,
                    City = 3,
                }
            };

            var changeItem = Comparer.Compare(oldArray, newArray, null, null);
            changeItem.ReadableName = "元素数量不相等的数组";

            Assert.AreEqual(3, changeItem.Sub.Count);
            Assert.AreEqual("CC", changeItem.Sub.First(i => i.ReadableName == "第2项").Sub.First(i => i.Name == "Name").NewValue);
            Assert.AreEqual(string.Empty, changeItem.Sub.First(i => i.ReadableName == "第2项").Sub.First(i => i.Name == "Name").OldValue);
        }
    }

    /// <summary>
    /// 简单类型
    /// </summary>
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

    /// <summary>
    /// 多级嵌套类型
    /// </summary>
    public class MulitNestedModel
    {
        public NestedModel SubNested
        {
            get;
            set;
        }

        public string NestedName
        {
            get;
            set;
        }
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
}
