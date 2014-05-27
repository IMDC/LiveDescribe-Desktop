using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using LiveDescribe.Model;
using Newtonsoft.Json;

namespace LiveDescribe.Utilities
{
    public static class FileReader
    {
        #region Logger
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        public static Project ReadProjectFile(string path)
        {
            log.Info("Reading project file from " + path);
            Project p;
            using (var r = new StreamReader(path))
            {
                //TODO: Error handling when missing a property
                p = JsonConvert.DeserializeObject<Project>(r.ReadToEnd());
            }
            log.Info("Project file successfully read from " + path);
            return p;
        }

        public static Header ReadWaveFormHeader(Project project)
        {
            log.Info("Reading waveform header from " + project.WaveFormHeaderFile);
            Header header;
            using (var file = File.Open(project.WaveFormHeaderFile, FileMode.Open, FileAccess.Read))
            {
                var bin = new BinaryFormatter();
                header = (Header)bin.Deserialize(file);
            }
            log.Info("Waveform header successfully read from " + project.WaveFormHeaderFile);
            return header;
        }

        public static List<short> ReadWaveFormFile(Project project)
        {
            List<short> waveFormData;
            log.Info("Reading waveform data from " + project.WaveFormFile);
            using (var file = File.Open(project.WaveFormFile, FileMode.Open, FileAccess.Read))
            {
                var bin = new BinaryFormatter();
                waveFormData = (List<short>) bin.Deserialize(file);
            }
            log.Info("Waveform data successfully read from " + project.WaveFormFile);
            return waveFormData;
        }

        public static List<Description> ReadDescriptionsFile(Project project)
        {
            List<Description> descriptions;
            log.Info("Reading descriptions from " + project.DescriptionsFile);
            using (var r = new StreamReader(project.DescriptionsFile))
            {
                descriptions = JsonConvert.DeserializeObject<List<Description>>(r.ReadToEnd());
            }
            log.Info("Descriptions successfully read from " + project.DescriptionsFile);
            return descriptions;
        }

        public static List<Space> ReadSpacesFile(Project project)
        {
            List<Space> spaces;
            log.Info("Reading spaces from " + project.SpacesFile);
            using (var r = new StreamReader(project.SpacesFile))
            {
                spaces = JsonConvert.DeserializeObject<List<Space>>(r.ReadToEnd());
            }
            log.Info("Spaces successfully read from " + project.SpacesFile);
            return spaces;
        }
    }
}
