﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sabrina.Models
{
    [Table("Sankaku_TagWhiteList")]
    public partial class SankakuTagWhiteList
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [Column("TagID")]
        public int TagId { get; set; }
        [Column("ChannelID")]
        public long ChannelId { get; set; }
        public int Weight { get; set; }
    }
}