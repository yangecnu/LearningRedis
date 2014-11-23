using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlogSystem
{
    public interface IHasBlogRepository
    {
        IBlogRepository Repository { set; }
    }

}
