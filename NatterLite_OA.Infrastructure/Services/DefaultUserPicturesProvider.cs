using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NatterLite_OA.Core.ServiceInterfaces;

namespace NatterLite_OA.Infrastructure.Services
{
    public class DefaultUserPicturesProvider : IPicturesProvider
    {
        public byte[] GetDefaultPicture(string imageLocation)
        {
            byte[] imageData = null;
            FileInfo fileInfo = new FileInfo(imageLocation);
            long imageFileLength = fileInfo.Length;
            FileStream fs = new FileStream(imageLocation, FileMode.Open, FileAccess.Read);
            using (BinaryReader br = new BinaryReader(fs))
            {
                imageData = br.ReadBytes((int)imageFileLength);
            }
            return imageData;
        }
    }
}
