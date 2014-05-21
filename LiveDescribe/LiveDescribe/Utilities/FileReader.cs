using System;
using System.Collections.Generic;
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
        public static Project ReadProjectFile(string path)
        {
            Project p;
            using (var r = new StreamReader(path))
            {
                //TODO: Error handling when missing a property
                p = JsonConvert.DeserializeObject<Project>(r.ReadToEnd());
            }
            return p;
        }

        public static List<short> ReadWaveFormFile(Project project)
        {
            var waveFormData = new List<short>();

            using (var file = File.Open(project.WaveFormFile, FileMode.Open, FileAccess.Read))
            {
                var bin = new BinaryFormatter();
                waveFormData = (List<short>) bin.Deserialize(file);
            }

            return waveFormData;
        }
    }
}
