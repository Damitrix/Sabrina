using System;
using System.Collections.Generic;

namespace Sabrina.Models
{
    public partial class PornhubVideos
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Creator { get; set; }
        public DateTime Date { get; set; }
        public string ImageUrl { get; set; }
        public string Title { get; set; }
    }
}
