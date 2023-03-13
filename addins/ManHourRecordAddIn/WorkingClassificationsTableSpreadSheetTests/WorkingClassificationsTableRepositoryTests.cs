using ClosedXML.Excel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wada.ManHourRecordService;

namespace WorkingClassificationsTableSpreadSheet.Tests
{
    [TestClass()]
    public class WorkingClassificationsTableRepositoryTests
    {
        [TestMethod()]
        public void 正常系_項目分類表が取得できること()
        {
            // given
            var stream = new MemoryStream();
            using (var xlBook = new XLWorkbook())
            {
                var sheet = xlBook.AddWorksheet();
                sheet.Cell("A1").Value = "A1";
                sheet.Cell("B1").Value = "B1";
                sheet.Cell("C1").Value = "C1";
                sheet.Cell("D1").Value = "D1";
                sheet.Cell("E1").Value = "E1";
                sheet.Cell("E10").Value = "E10";
                xlBook.SaveAs(stream);
            }

            // when
            IWorkingClassificationsTableRepository repository = new WorkingClassificationsTableRepository();
            var actual = repository.FetchAll(stream);

            // then
            Assert.IsNotNull(actual);
            Assert.AreEqual(10, actual.ToArray().Length);
            Assert.AreEqual(5, actual.ToArray()[0].ToArray().Length);
            Assert.AreEqual("A1", actual.ToArray()[0].ToArray()[0]);
            Assert.AreEqual("B1", actual.ToArray()[0].ToArray()[1]);
            Assert.AreEqual("C1", actual.ToArray()[0].ToArray()[2]);
            Assert.AreEqual("D1", actual.ToArray()[0].ToArray()[3]);
            Assert.AreEqual("E1", actual.ToArray()[0].ToArray()[4]);
            Assert.IsNull(actual.ToArray()[9].ToArray()[0]);
            Assert.IsNull(actual.ToArray()[9].ToArray()[1]);
            Assert.IsNull(actual.ToArray()[9].ToArray()[2]);
            Assert.IsNull(actual.ToArray()[9].ToArray()[3]);
            Assert.AreEqual("E10", actual.ToArray()[9].ToArray()[4]);
        }
    }
}