﻿using System;
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
                p = JsonConvert.DeserializeObject<Project>(r.ReadToEnd());
            }
            return p;
        }

        public static List<float> ReadWaveFormFile(Project project)
        {
            var waveFormData = new List<float>();

            using (var file = File.Open(project.WaveFormFile.AbsolutePath, FileMode.Open, FileAccess.Read))
            {
                var bin = new BinaryFormatter();
                waveFormData = (List<float>) bin.Deserialize(file);
            }

            return waveFormData;
        }
    }
}