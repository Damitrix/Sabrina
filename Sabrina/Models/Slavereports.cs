﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sabrina.Models
{
    public partial class Slavereports
    {
        [Key]
        [Column("SlaveReportID")]
        public int SlaveReportId { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime TimeOfReport { get; set; }
        [Column("UserID")]
        public long UserId { get; set; }
        public int Edges { get; set; }
        [Required]
        [StringLength(20)]
        public string SessionOutcome { get; set; }
        public long TimeSpan { get; set; }

        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(Users.Slavereports))]
        public virtual Users User { get; set; }
    }
}