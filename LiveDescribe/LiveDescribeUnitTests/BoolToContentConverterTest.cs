using LiveDescribe.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;

namespace LiveDescribeUnitTests
{
    [TestClass]
    public class BoolToContentConverterTest
    {
        private const string TrueContentString = "True Content";
        private const string FalseContentString = "False Content";
        private const string NullContentString = "Null Content";
        private readonly Type _objectType = typeof(object);
        private readonly CultureInfo _currentCulture = CultureInfo.CurrentCulture;

        private BoolToContentConverter CreateTwoStateConverter()
        {
            return new BoolToContentConverter
            {
                TrueContent = TrueContentString,
                FalseContent = FalseContentString
            };
        }

        private BoolToContentConverter CreateThreeStateConverter()
        {
            return new BoolToContentConverter
            {
                IsThreeState = true,
                TrueContent = TrueContentString,
                FalseContent = FalseContentString,
                NullContent = NullContentString
            };
        }

        [TestMethod]
        public void BoolToContentConverter_TrueValueTest()
        {
            //Arrange
            const bool value = true;
            const string expectedContent = TrueContentString;

            BoolToContentConverter c1 = CreateTwoStateConverter();
            BoolToContentConverter c2 = CreateThreeStateConverter();

            string result1;
            string result2;

            //Act
            result1 = (string)c1.Convert(value, _objectType, null, _currentCulture);
            result2 = (string)c2.Convert(value, _objectType, null, _currentCulture);

            //Assert
            Assert.AreEqual(expectedContent, result1);
            Assert.AreEqual(expectedContent, result2);
        }

        [TestMethod]
        public void BoolToContentConverter_FalseValueTest()
        {
            //Arrange
            const bool value = false;
            const string expectedContent = FalseContentString;

            BoolToContentConverter c1 = CreateTwoStateConverter();
            BoolToContentConverter c2 = CreateThreeStateConverter();

            string result1;
            string result2;

            //Act
            result1 = (string)c1.Convert(value, _objectType, null, _currentCulture);
            result2 = (string)c2.Convert(value, _objectType, null, _currentCulture);

            //Assert
            Assert.AreEqual(expectedContent, result1);
            Assert.AreEqual(expectedContent, result2);
        }

        [TestMethod]
        public void BoolToContentConverter_NullValueTest()
        {
            //Arrange
            bool? nullBoolValue = null;
            object nullValue = null;
            const double wrongValue = Math.PI;

            const string expectedContent1 = FalseContentString;
            const string expectedContent2 = NullContentString;

            BoolToContentConverter c1 = CreateTwoStateConverter();
            BoolToContentConverter c2 = CreateThreeStateConverter();

            string result1;
            string result2;
            string result3;
            string result4;
            string result5;
            string result6;

            //Act
            result1 = (string)c1.Convert(nullBoolValue, _objectType, null, _currentCulture);
            result2 = (string)c1.Convert(nullValue, _objectType, null, _currentCulture);
            result3 = (string)c1.Convert(wrongValue, _objectType, null, _currentCulture);
            result4 = (string)c2.Convert(nullBoolValue, _objectType, null, _currentCulture);
            result5 = (string)c2.Convert(nullValue, _objectType, null, _currentCulture);
            result6 = (string)c2.Convert(wrongValue, _objectType, null, _currentCulture);

            //Assert
            Assert.AreEqual(expectedContent1, result1);
            Assert.AreEqual(expectedContent1, result2);
            Assert.AreEqual(expectedContent1, result3);
            Assert.AreEqual(expectedContent2, result4);
            Assert.AreEqual(expectedContent2, result5);
            Assert.AreEqual(expectedContent2, result6);
        }
    }
}
