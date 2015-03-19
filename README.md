# ModelDataCompare
ModelDataCompare provides a set of methods for comparison between two instances of the same types of data changes.

一、简单类型的数据比较：
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
