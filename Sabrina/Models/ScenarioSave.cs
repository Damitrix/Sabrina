﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sabrina.Models
{
    [Table("Scenario_Save")]
    public partial class ScenarioSave
    {
        public ScenarioSave()
        {
            ScenarioSavePlayer = new HashSet<ScenarioSavePlayer>();
        }

        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime CreationDate { get; set; }
        [Required]
        public string Name { get; set; }
        [Column("LocationID")]
        public int LocationId { get; set; }
        [Column("LocationModifierID")]
        public int LocationModifierId { get; set; }

        [InverseProperty("Save")]
        public virtual ICollection<ScenarioSavePlayer> ScenarioSavePlayer { get; set; }
    }
}