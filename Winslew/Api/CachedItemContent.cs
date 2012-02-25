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
                if (FullVersion != null)
                {
                    return string.Format("{0} {1}", FullUpdated.ToShortDateString(), FullUpdated.ToShortTimeString());
                }
                else
                {
                    return "---";
                }
            }
        }


        public string MoreVersion {get; set;}
        public DateTime MoreUpdated { get; set; }
        public string MoreUpdatedHumanReadable
        {
            get
            {
                if (MoreVersion != null)
                {
                    return string.Format("{0} {1}", MoreUpdated.ToShortDateString(), MoreUpdated.ToShortTimeString());
                }
                else
                {
                    return "---";
                }
            }
        }

        public string LessVersion { get; set; }
        public DateTime LessUpdated { get; set; }
        public string LessUpdatedHumanReadable
        {
            get
            {
                if (LessVersion != null)
                {
                    return string.Format("{0} {1}", LessUpdated.ToShortDateString(), LessUpdated.ToShortTimeString());
                }
                else
                {
                    return "---";
                }
            }
        }


        public DateTime Updated { get; set; }
        public string UpdatedHumanReadable
        {
            get
            {
                return string.Format("Full: {0}, More: {1}, Less: {2}", FullUpdatedHumanReadable, MoreUpdatedHumanReadable, LessUpdatedHumanReadable);
            }
        }
        public string FavIconPath { get; set; }

        
        public bool IsComplete
        {
            get
            {
                if (LessVersion != null && MoreVersion != null && FullVersion != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public CachedItemContent()
        {
            
        }

    }
}
