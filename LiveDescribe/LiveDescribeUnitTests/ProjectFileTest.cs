using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LiveDescribe.Model;

namespace LiveDescribeUnitTests
{
    /// <summary>
    /// Tests for the Project File Class are contained here.
    /// </summary>
    [TestClass]
    public class ProjectFileTest
    {
        #region Auto-Generated Stuff
        public ProjectFileTest()
        { }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion
        #endregion

        [TestMethod]
        public void InternalRelativeFileCreationTest()
        {
            //Arrange
            string pathToProjectFolder = "D:\\Test\\Wildlife";
            string relativePath1 = "Wildlife.wmv";
            string relativePath2 = "projectCache\\waveform.bin";
            string expectedAbsolutePath1 = "D:\\Test\\Wildlife\\Wildlife.wmv";
            string expectedAbsolutePath2 = "D:\\Test\\Wildlife\\projectCache\\waveform.bin";

            ProjectFile pf1;
            ProjectFile pf2;

            //Act
            pf1 = new ProjectFile { RelativePath = relativePath1 };
            pf1.MakeAbsoluteWith(pathToProjectFolder);

            pf2 = new ProjectFile { RelativePath = relativePath2 };
            pf2.MakeAbsoluteWith(pathToProjectFolder);

            //Assert
            Assert.AreEqual(pf1.AbsolutePath, expectedAbsolutePath1);
            Assert.AreEqual(pf2.AbsolutePath, expectedAbsolutePath2);
        }

        [TestMethod]
        public void ExternalRelativeFileCreationTest()
        {
            //Arrange
            string relativePath = "../../Valentin/Videos/Wildlife.wmv";
            string pathToProjectFolder = "D:\\Test\\RelTest2";

            string expectedPath = "D:\\Valentin\\Videos\\Wildlife.wmv";

            ProjectFile f;

            //Act
            f = new ProjectFile
            {
                RelativePath = relativePath,
            };

            f.MakeAbsoluteWith(pathToProjectFolder);

            //Assert
            Assert.AreEqual(f.AbsolutePath, expectedPath);
        }
    }
}
