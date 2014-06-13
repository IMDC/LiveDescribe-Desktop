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

        /// <summary>
        /// Tests the creation of project files from given relative file paths. These paths are
        /// expected to be contained in their base folder.
        /// </summary>
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
            pf1 = ProjectFile.FromRelativePath(relativePath1, pathToProjectFolder);

            pf2 = ProjectFile.FromRelativePath(relativePath2, pathToProjectFolder);

            //Assert
            Assert.AreEqual(pf1.AbsolutePath, expectedAbsolutePath1);
            Assert.AreEqual(pf2.AbsolutePath, expectedAbsolutePath2);
        }

        /// <summary>
        /// Tests the creation of project files from given relative file paths. These paths are
        /// expected to be contained outside of the given base folder.
        /// </summary>
        [TestMethod]
        public void ExternalRelativeFileCreationTest()
        {
            //Arrange
            string pathToProjectFolder = "D:\\Test\\RelTest2";
            string relativePath1 = "../../Valentin/Videos/Wildlife.wmv";
            string expectedPath1 = "D:\\Valentin\\Videos\\Wildlife.wmv";

            ProjectFile pf1;

            //Act
            pf1 = ProjectFile.FromRelativePath(relativePath1, pathToProjectFolder);

            //Assert
            Assert.AreEqual(pf1.AbsolutePath, expectedPath1);
        }

        /// <summary>
        /// Tests creation of project files from Absolute Paths expected to be contained inside
        /// their given base folder.
        /// </summary>
        [TestMethod]
        public void InternalAbsoluteFileCreationTest()
        {
            //Arrange
            string pathToProjectFolder = "D:\\Test\\Wildlife";
            string absolutePath1 = "D:\\Test\\Wildlife\\Wildlife.wmv";
            string absolutePath2 = "D:\\Test\\Wildlife\\projectCache\\waveform.bin";
            string expectedRelativePath1 = "Wildlife.wmv";
            string expectedRelativePath2 = "projectCache/waveform.bin";

            ProjectFile pf1;
            ProjectFile pf2;

            //Act
            pf1 = ProjectFile.FromAbsolutePath(absolutePath1, pathToProjectFolder);
            pf2 = ProjectFile.FromAbsolutePath(absolutePath2, pathToProjectFolder);

            //Assert
            Assert.AreEqual(pf1.RelativePath, expectedRelativePath1);
            Assert.AreEqual(pf2.RelativePath, expectedRelativePath2);
        }

        /// <summary>
        /// Tests creation of project files from Absolute paths expected to be contained outside
        /// their given base folder.
        /// </summary>
        [TestMethod]
        public void ExternalAbsoluteFileCreationTest()
        {
            //Arrange
            string absolutePath1 = "D:\\Valentin\\Videos\\Wildlife.wmv";
            string basePath = "D:\\Test\\RelTest2";
            string expectedRelativePath1 = "../../Valentin/Videos/Wildlife.wmv";

            ProjectFile pf1;

            //Act
            pf1 = ProjectFile.FromAbsolutePath(absolutePath1, basePath);

            //Assert
            Assert.AreEqual(pf1.RelativePath, expectedRelativePath1);
        }
    }
}
