﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sabrina.Models
{
    [Table("Scenario_LocationModifier")]
    public partial class ScenarioLocationModifier
    {
        public ScenarioLocationModifier()
        {
            ScenarioLocationModifierLink = new HashSet<ScenarioLocationModifierLink>();
        }

        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        [InverseProperty("LocationModifier")]
        public virtual ICollection<ScenarioLocationModifierLink> ScenarioLocationModifierLink { get; set; }
    }
}