﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sabrina.Models
{
    [Table("WaifuJOI_Album")]
    public partial class WaifuJoiAlbum
    {
        public long ChannelId { get; set; }
        [Required]
        [StringLength(24)]
        public string ContentId { get; set; }
        [Key]
        [Column("ID")]
        public int Id { get; set; }
    }
}