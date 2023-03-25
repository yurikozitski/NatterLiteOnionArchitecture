using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Http;


namespace NatterLite_OA.Core.ServiceInterfaces
{
    public interface IImageValidator
    {
        public bool IsImageValid(IFormFile picture);
    }
}
