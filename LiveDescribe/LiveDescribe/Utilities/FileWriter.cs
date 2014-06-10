using LiveDescribe.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace LiveDescribe.Utilities
{
    public static class FileWriter
    {
        #region Logger
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        /// <summary>
        /// Writes a Project object to a file. The path is determined by the project's ProjectFile.AbsolutePath.
        /// </summary>
        /// <param name="project"></param>
        public static void WriteProjectFile(Project project)
        {
            Log.Info("Saving project file to " + project.Files.Project);
            var serializer = new JsonSerializer { Formatting = Formatting.Indented };
            using (var sw = new StreamWriter(project.Files.Project, false))
            {
                serializer.Serialize(sw, project, typeof(Project));
            }
            Log.Info("Project file saved successfully");
        }

        /// <summary>
        /// Writes the header for the WAV file that the waveform data was sampled from.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="header"></param>
        public static void WriteWaveFormHeader(Project project, Header header)
        {
            Log.Info("Saving waveform header file to " + project.Files.WaveFormHeader);
            using (var file = File.Open(project.Files.WaveFormHeader, FileMode.Create, FileAccess.Write))
            {
                var bin = new BinaryFormatter();
                bin.Serialize(file, header);
            }
            Log.Info("Waveform header file saved successfully");
        }

        /// <summary>
        /// Writes audio data to the file path specified by project.Files.WaveForm.AbsolutePath.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="waveFormData"></param>
        public static void WriteWaveFormFile(Project project, List<short> waveFormData)
        {
            Log.Info("Saving waveform data file to " + project.Files.WaveForm);
            using (var file = File.Open(project.Files.WaveForm, FileMode.Create, FileAccess.Write))
            {
                var bin = new BinaryFormatter();
                bin.Serialize(file, waveFormData);
            }
            Log.Info("Waveform data file saved successfully");
        }

        public static void WriteDescriptionsFile(Project project, ObservableCollection<Description> descriptions)
        {
            Log.Info("Saving descriptions file to " + project.Files.Descriptions);
            var json = new JsonSerializer { Formatting = Formatting.Indented };
            using (var sw = new StreamWriter(project.Files.Descriptions))
            {
                json.Serialize(sw, descriptions.ToList());
            }
            Log.Info("Descriptions file saved successfully");
        }

        public static void WriteSpacesFile(Project project, ObservableCollection<Space> spaces)
        {
            Log.Info("Saving spaces file to " + project.Files.Spaces);
            var json = new JsonSerializer { Formatting = Formatting.Indented };
            using (var sw = new StreamWriter(project.Files.Spaces))
            {
                json.Serialize(sw, spaces.ToList());
            }
            Log.Info("Spaces file saved successfully");
        }
    }
}
