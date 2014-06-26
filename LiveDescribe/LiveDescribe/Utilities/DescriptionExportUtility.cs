using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveDescribe.Model;
namespace LiveDescribe.Utilities
{
    class DescriptionExportUtility
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Instance Variables
        private string _videoFile;
        private List<Description> _descriptionList;
        #endregion

        #region Constructors
        public DescriptionExportUtility(string videoFile, List<Description> descriptionList)
        {
            _videoFile = videoFile;
            _descriptionList = descriptionList;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public void exportVideoWithDescriptions()
        {
            foreach (var description in _descriptionList)
            {
                Console.WriteLine(description.AudioFile);
            }
        }
    }
}
