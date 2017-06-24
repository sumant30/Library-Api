using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.API.Models
{
    public class LinkCollectionResourceWrapperDto<T>:LinkedResourceBaseDto where T :LinkedResourceBaseDto
    {
        public IEnumerable<T> Value { get; set; }

        public LinkCollectionResourceWrapperDto ( IEnumerable<T> Value )
        {
            this . Value = Value;
        }
    }
}
