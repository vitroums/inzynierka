using DropNet.Models;

namespace Client.Cloud
{
    public class CloudFile
    {
        public string Name { get; private set; }
        public string Extension { get; private set; }
        public long Size { get; private set; }

        public CloudFile(MetaData file)
        {
            Name = file.Name;
            Extension = file.Extension;
            Size = file.Bytes;
        }

        public static implicit operator CloudFile(MetaData file)
        {
            return new CloudFile(file);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
