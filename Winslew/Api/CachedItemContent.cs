using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Winslew.Api
{
    public class CachedItemContent
    {
        public string Id { get; set; }
        public string ExternalUrl { get; set; }

        public string FullVersion { get; set; }
        public DateTime FullUpdated { get; set; }
        public string FullUpdatedHumanReadable
        {
            get
            {
                if (Updated != null)
                {
                    return string.Format("{0} {1}", FullUpdated.ToShortDateString(), FullUpdated.ToShortTimeString());
                }
                else
                {
                    return "---";
                }
            }
        }

        public string MoreVersion {
            get 
            {
                return _moreVersion;
            }
            set
            {
                _moreVersion = value;
            }
        }
        public string LessVersion { get; set; }
        public DateTime Updated { get; set; }
        public string UpdatedHumanReadable {
            get
            {
                if (Updated != null)
                {
                    return string.Format("{0} {1}", Updated.ToShortDateString(), Updated.ToShortTimeString());
                }
                else
                {
                    return "---";
                }
            }
        }
        public string FavIconPath { get; set; }

        private string _moreVersion = "";
    }
}
