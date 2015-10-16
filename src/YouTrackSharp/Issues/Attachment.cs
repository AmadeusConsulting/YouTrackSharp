using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YouTrackSharp.Issues
{
    public class Attachment
    {
        public string Id { get; set; }
        public string Url { get; set; }
        public string Name { get; set; }
        public string AuthorLogin { get; set; }
        public string Group { get; set; }
        public DateTime Created { get; set; }
    }
}
