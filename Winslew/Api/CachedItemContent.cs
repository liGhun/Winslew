﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Winslew.Api
{
    public class CachedItemContent
    {
        public string Id { get; set; }
        public string ExternalUrl { get; set; }
        public string MoreVersion { get; set; }
        public string LessVersion { get; set; }
        public DateTime Updated { get; set; }
        public string UpdatedHumanReadable { get; set; }
        public string FavIconPath { get; set; }
    }
}
