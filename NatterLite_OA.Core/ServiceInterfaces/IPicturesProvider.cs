using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NatterLite_OA.Core.ServiceInterfaces
{
    public interface IPicturesProvider
    {
        public byte[] GetDefaultPicture(string imageLocation);
    }
}
