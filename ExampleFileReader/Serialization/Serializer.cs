using ExampleFileReader.InstanceData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleFileReader.Serialization
{
    public class Serializer
    {
        public void Serialize(Instance instance, string path)
        {
            System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(Instance));

            var directory = new FileInfo(path).DirectoryName;
            Directory.CreateDirectory(directory);
            FileStream file = File.Create(path);

            writer.Serialize(file, instance);
            file.Close();
        }
    }
}
