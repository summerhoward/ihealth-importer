using System;
using iHealthImport;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class JsonTest
    {
        [TestMethod]
        public void JsonFileTest()
        {
            new HealthRepository().DoImport("C:\\Users\\matttorres\\Desktop\\iHealthImport\\Tests\\20161127-20161203.json");
        }
    }
}
