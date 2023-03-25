using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NatterLite_OA.Core.RepositoryInterfaces;

namespace NatterLite_OA.Core.ServiceInterfaces
{
    public interface IDataInitializer
    {
        Task InitializeAsync(IUserRepository userRepository,
            IPicturesProvider picturesProvider);
    }
}
