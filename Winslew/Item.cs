using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Winslew
{
    public class Item
    {
        public string id { get; set; }
        public string title { get; set; }
        public string url { get; set; }        
        public long timeUpdated { get; set; }
        public string timeUpdatedHumanReadable { get; set; }
        public long timeAdded { get; set; }
        public string timeAddedHumanReadable { get; set; }
        public string tags { get; set; }
        public bool read { get; set; }
        public Api.CachedItemContent contentCache { get; set; }
    }
}
