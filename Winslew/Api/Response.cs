using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Winslew.Api
{
    public class Response
    {
        public bool Success { get; set; }
        public string Status { get; set; }
        public string Error { get; set; }
        public string Content { get; set; }
        public int LimitUserLimit
        {
            get;
            set;
        }
        public int LimitUserRemanining { get; set; }
        public int LimitUserReset { get; set; }
        public int LimitKeyLimit { get; set; }
        public int LimitKeyRemaining { get; set; }
        public int LimitKeyReset { get; set; }

        public string TextTitle { get; set; }
        public bool TextLoginFound { get; set; }
        public string TextNext { get; set; }
        public string TextContentType { get; set; }

        public WebHeaderCollection FullHeaders { get; set; }

        public void UpdateStatistics()
        {
            AppController.Current.updateApiStatusBar(this.LimitUserRemanining, this.LimitUserLimit, this.LimitUserReset, this.LimitKeyRemaining, this.LimitKeyLimit, this.LimitKeyReset);
        }
    }
}
