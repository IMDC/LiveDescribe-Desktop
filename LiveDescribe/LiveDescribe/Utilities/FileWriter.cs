using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using LiveDescribe.Model;
using Newtonsoft.Json;

namespace LiveDescribe.Utilities
{
    public static class FileWriter
    {
        /// <summary>
        /// Writes a Project object to a file. The path is determined by the project's 
        /// ProjectFile.AbsolutePath.
        /// </summary>
        /// <param name="project"></param>
        public static void WriteProjectFile(Project project)
        {
            var serializer = new JsonSerializer { Formatting = Formatting.Indented };
            using (var sw = new StreamWriter(project.ProjectFile, false))
            {
                serializer.Serialize(sw, project, typeof(Project));
            }
        }

        /// <summary>
        /// Writes the header for the WAV file that the waveform data was sampled from.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="header"></param>
        public static void WriteWaveFormHeader(Project project, Header header)
        {
            using (var file = File.Open(project.WaveFormHeaderFile, FileMode.Create, FileAccess.Write))
            {
                var bin = new BinaryFormatter();
                bin.Serialize(file, header);
            }
        }

        /// <summary>
        /// Writes audio data to the file path specified by project.WaveFormFile.AbsolutePath.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="waveFormData"></param>
        public static void WriteWaveFormFile(Project project, List<short> waveFormData)
        {
            using (var file = File.Open(project.WaveFormFile, FileMode.Create, FileAccess.Write))
            {
                var bin = new BinaryFormatter();
                bin.Serialize(file, waveFormData);
            }
        }

        public static void WriteDescriptionsFile(Project project, ObservableCollection<Description> descriptions)
        {
            var json = new JsonSerializer { Formatting = Formatting.Indented };
            using (var sw = new StreamWriter(project.DescriptionsFile))
            {
                json.Serialize(sw, descriptions.ToList());
            }
        }

        public static void WriteSpacesFile(Project project, ObservableCollection<Space> spaces)
        {
            var json = new JsonSerializer() { Formatting = Formatting.Indented };
            using (var sw = new StreamWriter(project.SpacesFile))
            {
                json.Serialize(sw, spaces.ToList());
            }
        }
    }
}
