﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sabrina.Models
{
    public partial class Finisher
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [Required]
        public string Link { get; set; }
        [Column("CreatorID")]
        public int CreatorId { get; set; }
        public int Type { get; set; }

        [ForeignKey(nameof(CreatorId))]
        [InverseProperty("Finisher")]
        public virtual Creator Creator { get; set; }
    }
}