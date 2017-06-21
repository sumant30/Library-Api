﻿using System;
using System . Collections . Generic;
using System . Linq;
using System . Threading . Tasks;

namespace Library . API . Helpers
{
    public class AuthorResourceParameters
    {
        const int maxPageSize = 10;

        public int PageNumber { get; set; } = 1;

        private int _pageSize { get; set; } = 10;

        public int PageSize
        {
            get
            {
                return _pageSize;
            }
            set
            {
                _pageSize = ( value > maxPageSize ) ? maxPageSize : value;
            }
        }

        public string Genre { get; set; }

        public string SearchQuery { get; set; }

        public string OrderBy { get; set; } = "Name";

        public string Fields { get; set; }
    }
}
