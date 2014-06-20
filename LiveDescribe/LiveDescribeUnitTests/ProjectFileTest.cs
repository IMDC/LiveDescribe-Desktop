using LiveDescribe.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()] public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run [ClassCleanup()] public
        // static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test [TestInitialize()] public void
        // MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run [TestCleanup()] public void
        // MyTestCleanup() { }
        //
        #endregion
        #endregion

        /// <summary>
        /// Tests the creation of project files from given relative file paths. These paths are
        /// expected to be contained in their base folder.
        /// </summary>
        [TestMethod]
        public void ProjectFile_InternalRelativeFileCreationTest()
        {
            //Arrange
            const string pathToProjectFolder = "D:\\Test\\Wildlife";
            const string relativePath1 = "Wildlife.wmv";
            const string relativePath2 = "projectCache\\waveform.bin";
            const string expectedAbsolutePath1 = "D:\\Test\\Wildlife\\Wildlife.wmv";
            const string expectedAbsolutePath2 = "D:\\Test\\Wildlife\\projectCache\\waveform.bin";

            ProjectFile pf1;
            ProjectFile pf1a;
            ProjectFile pf2;
            ProjectFile pf2a;

            //Act
            pf1 = ProjectFile.FromRelativePath(relativePath1, pathToProjectFolder);
            pf1a = new ProjectFile { RelativePath = relativePath1 };
            pf1a.MakeAbsoluteWith(pathToProjectFolder);

            pf2 = ProjectFile.FromRelativePath(relativePath2, pathToProjectFolder);
            pf2a = new ProjectFile { RelativePath = relativePath2 };
            pf2a.MakeAbsoluteWith(pathToProjectFolder);

            //Assert
            Assert.AreEqual(expectedAbsolutePath1, pf1.AbsolutePath);
            Assert.AreEqual(expectedAbsolutePath1, pf1a.AbsolutePath);

            Assert.AreEqual(expectedAbsolutePath2, pf2.AbsolutePath);
            Assert.AreEqual(expectedAbsolutePath2, pf2a.AbsolutePath);
        }

        /// <summary>
        /// Tests the creation of project files from given relative file paths. These paths are
        /// expected to be contained outside of the given base folder.
        /// </summary>
        [TestMethod]
        public void ProjectFile_ExternalRelativeFileCreationTest()
        {
            //Arrange
            const string pathToProjectFolder = "D:\\Test\\RelTest2";
            const string relativePath1 = "../../Valentin/Videos/Wildlife.wmv";
            const string expectedPath1 = "D:\\Valentin\\Videos\\Wildlife.wmv";

            ProjectFile pf1;
            ProjectFile pf1a;

            //Act
            pf1 = ProjectFile.FromRelativePath(relativePath1, pathToProjectFolder);
            pf1a = new ProjectFile { RelativePath = relativePath1 };
            pf1a.MakeAbsoluteWith(pathToProjectFolder);

            //Assert
            Assert.AreEqual(expectedPath1, pf1.AbsolutePath);
            Assert.AreEqual(expectedPath1, pf1a);
        }

        /// <summary>
        /// Tests creation of project files from Absolute Paths expected to be contained inside
        /// their given base folder.
        /// </summary>
        [TestMethod]
        public void ProjectFile_InternalAbsoluteFileCreationTest()
        {
            //Arrange
            const string pathToProjectFolder = "D:\\Test\\Wildlife";
            const string absolutePath1 = "D:\\Test\\Wildlife\\Wildlife.wmv";
            const string absolutePath2 = "D:\\Test\\Wildlife\\projectCache\\waveform.bin";
            const string expectedRelativePath1 = "Wildlife.wmv";
            const string expectedRelativePath2 = "projectCache/waveform.bin";

            ProjectFile pf1;
            ProjectFile pf1a;
            ProjectFile pf2;
            ProjectFile pf2a;

            //Act
            pf1 = ProjectFile.FromAbsolutePath(absolutePath1, pathToProjectFolder);
            pf1a = new ProjectFile { AbsolutePath = absolutePath1 };
            pf1a.MakeRelativeTo(pathToProjectFolder);

            pf2 = ProjectFile.FromAbsolutePath(absolutePath2, pathToProjectFolder);
            pf2a = new ProjectFile { AbsolutePath = absolutePath2 };
            pf2a.MakeRelativeTo(pathToProjectFolder);

            //Assert
            Assert.AreEqual(expectedRelativePath1, pf1.RelativePath);
            Assert.AreEqual(expectedRelativePath1, pf1a.RelativePath);
            Assert.AreEqual(expectedRelativePath2, pf2.RelativePath);
            Assert.AreEqual(expectedRelativePath2, pf2a.RelativePath);
        }

        /// <summary>
        /// Tests creation of project files from Absolute paths expected to be contained outside
        /// their given base folder.
        /// </summary>
        [TestMethod]
        public void ProjectFile_ExternalAbsoluteFileCreationTest()
        {
            //Arrange
            const string absolutePath1 = "D:\\Valentin\\Videos\\Wildlife.wmv";
            const string basePath = "D:\\Test\\RelTest2";
            const string expectedRelativePath1 = "../../Valentin/Videos/Wildlife.wmv";

            ProjectFile pf1;
            ProjectFile pf1a;

            //Act
            pf1 = ProjectFile.FromAbsolutePath(absolutePath1, basePath);
            pf1a = new ProjectFile { AbsolutePath = absolutePath1 };
            pf1a.MakeRelativeTo(basePath);

            //Assert
            Assert.AreEqual(expectedRelativePath1, pf1.RelativePath);
            Assert.AreEqual(expectedRelativePath1, pf1a.RelativePath);
        }

        /// <summary>
        /// Tests the output of the toString method, as well as other methods of converting a
        /// ProjectFile to a string.
        /// </summary>
        [TestMethod]
        public void ProjectFile_StringTest()
        {
            //Arrange
            const string absolutePath1 = "D:\\Valentin\\Videos\\Wildlife.wmv";
            const string relativePath1 = "../../Valentin/Videos/Wildlife.wmv";
            const string expectedString = absolutePath1;

            ProjectFile pf1;

            //Act
            pf1 = new ProjectFile
            {
                AbsolutePath = absolutePath1,
                RelativePath = relativePath1
            };

            //Assert
            Assert.AreEqual(expectedString, pf1);
            Assert.AreEqual(expectedString, pf1.AbsolutePath);
            Assert.AreEqual(expectedString, pf1.ToString());
        }
    }
}
