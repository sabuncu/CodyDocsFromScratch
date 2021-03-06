﻿using CodyDocsFromScratch.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodyDocsFromScratch.Services
{
    public static class DocumentationFileSerializer
    {
        public static FileDocumentation Deserialize(string filepath)
        {
            if (!File.Exists(filepath))
            {
                return new FileDocumentation();
            }
            string fileContents = File.ReadAllText(filepath);
            var deserialized = JsonConvert.DeserializeObject<FileDocumentation>(fileContents);
            return deserialized;
        }

        public static void Serialize(string filepath, FileDocumentation data)
        {
            string serialized = JsonConvert.SerializeObject(data);
            File.WriteAllText(filepath, serialized);
        }
    }
}
