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
 
        public string FullVersion { get; set; }
        public DateTime FullUpdated { get; set; }
        public string FullUpdatedHumanReadable
        {
            get
            {
                if (FullUpdated != null)
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
        public DateTime MoreUpdated { get; set; }
        public string MoreUpdatedHumanReadable
        {
            get
            {
                if (MoreUpdated != null)
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
                if (LessUpdated != null)
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
        public string UpdatedHumanReadable {
            get
            {
                return string.Format("Full: {0}, More: {1}, Less: ", FullUpdatedHumanReadable, MoreUpdatedHumanReadable, LessUpdatedHumanReadable);
            }
        }
        public string FavIconPath { get; set; }

        private string _moreVersion = "";
        public bool IsComplete {
            get {
                if(LessUpdated != null && MoreUpdated != null && FullUpdated != null) {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


}
