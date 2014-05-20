﻿using System;
using System.Collections.Generic;
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
            var serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            using (var sw = new StreamWriter(project.ProjectFile, false))
            {
                serializer.Serialize(sw, project, typeof (Project));
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
                bin.Serialize(file,waveFormData);
            }
        }
    }
}
