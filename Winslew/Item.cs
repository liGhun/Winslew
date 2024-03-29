﻿using System;
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
        public string DescripiveText
        {
            get
            {
                string text = "Title: " + this.title + "\r\n";
                text += "Url: " + this.url + "\r\n";
                text += "Tags: " + this.tags + "\r\n";
                text += "Added: " + this.timeUpdatedHumanReadable + "\r\n";
                text += "Updated: " + this.timeAddedHumanReadable + "\r\n";
                if (this.contentCache != null)
                {
                    text += "Cache updated: " + contentCache.UpdatedHumanReadable;
                }
                else
                {
                    text += "Cache updated: ---";
                }

                return text;
            }
        }

        public bool TagsHaveBeenUpdated { get { return _tagsHaveBeenUpdated; } }

        private bool _tagsHaveBeenUpdated = false;

        public void removeTag(string ToBeRemovedTag)
        {
            char[] delimiters = new char[] { ',' };
            string[] allTags = this.tags.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            if(allTags.Contains(ToBeRemovedTag.ToLower())) {
                List<string> tagsList = new List<string>(allTags);
                _tagsHaveBeenUpdated = true;
                tagsList.Remove(ToBeRemovedTag.ToLower());
                tags = string.Join(",",tagsList.ToArray());
            }
        }

        public Item()
        {
            contentCache = new Api.CachedItemContent();
        }
    }
}
