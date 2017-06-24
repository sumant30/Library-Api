using System;
using System . Collections . Generic;
using System . Linq;
using System . Threading . Tasks;

namespace Library . API . Models
{
    public class LinkDto
    {
        public string Href { get; set; }

        public string Rel { get; set; }

        public string Method { get; set; }

        public LinkDto ( string Href , string Rel , string Method )
        {
            this . Href = Href;
            this . Rel = Rel;
            this . Method = Method;
        }
    }
}
