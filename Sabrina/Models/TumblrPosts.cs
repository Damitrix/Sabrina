﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sabrina.Models
{
    public partial class TumblrPosts
    {
        [Key]
        [Column("TumblrID")]
        public long TumblrId { get; set; }
        public long IsLoli { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? LastPosted { get; set; }
    }
}