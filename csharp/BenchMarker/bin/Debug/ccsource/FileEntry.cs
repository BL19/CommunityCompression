using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunityComprimation
{
    public class FileEntry
    {

        public string name;
        public string content;
        public bool compressed;
        public CompressionMethod method;

        public void Compress() {
            //TODO: Compression method
        }

        public void DeCompress() {
            //TODO: DeCompression method
        }

        public void LoadFromFile(string name) {
            
        }

    }
}
