﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sabrina.Models
{
    [Table("Scenario_LocationModifierLink")]
    public partial class ScenarioLocationModifierLink
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [Column("LocationID")]
        public int LocationId { get; set; }
        [Column("LocationModifierID")]
        public int LocationModifierId { get; set; }

        [ForeignKey(nameof(LocationId))]
        [InverseProperty(nameof(ScenarioLocation.ScenarioLocationModifierLink))]
        public virtual ScenarioLocation Location { get; set; }
        [ForeignKey(nameof(LocationModifierId))]
        [InverseProperty(nameof(ScenarioLocationModifier.ScenarioLocationModifierLink))]
        public virtual ScenarioLocationModifier LocationModifier { get; set; }
    }
}