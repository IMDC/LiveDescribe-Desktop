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
            log.Info("Reading waveform header from " + project.Files.WaveFormHeader);
            Header header;
            using (var file = File.Open(project.Files.WaveFormHeader, FileMode.Open, FileAccess.Read))
            {
                var bin = new BinaryFormatter();
                header = (Header)bin.Deserialize(file);
            }
            log.Info("Waveform header successfully read from " + project.Files.WaveFormHeader);
            return header;
        }

        public static List<short> ReadWaveFormFile(Project project)
        {
            List<short> waveFormData;
            log.Info("Reading waveform data from " + project.Files.WaveForm);
            using (var file = File.Open(project.Files.WaveForm, FileMode.Open, FileAccess.Read))
            {
                var bin = new BinaryFormatter();
                waveFormData = (List<short>) bin.Deserialize(file);
            }
            log.Info("Waveform data successfully read from " + project.Files.WaveForm);
            return waveFormData;
        }

        public static List<Description> ReadDescriptionsFile(Project project)
        {
            List<Description> descriptions;
            log.Info("Reading descriptions from " + project.Files.Descriptions);
            using (var r = new StreamReader(project.Files.Descriptions))
            {
                descriptions = JsonConvert.DeserializeObject<List<Description>>(r.ReadToEnd());
            }
            log.Info("Descriptions successfully read from " + project.Files.Descriptions);
            return descriptions;
        }

        public static List<Space> ReadSpacesFile(Project project)
        {
            List<Space> spaces;
            log.Info("Reading spaces from " + project.Files.Spaces);
            using (var r = new StreamReader(project.Files.Spaces))
            {
                spaces = JsonConvert.DeserializeObject<List<Space>>(r.ReadToEnd());
            }
            log.Info("Spaces successfully read from " + project.Files.Spaces);
            return spaces;
        }
    }
}
