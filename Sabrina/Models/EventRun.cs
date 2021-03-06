﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sabrina.Models
{
    public partial class EventRun
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime Time { get; set; }
        [Column("EventID")]
        public int EventId { get; set; }
        [Column("ChannelID")]
        public long ChannelId { get; set; }

        [ForeignKey(nameof(EventId))]
        [InverseProperty("EventRun")]
        public virtual Event Event { get; set; }
    }
}