﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sabrina.Models
{
    [Table("Scenario_SavePlayer")]
    public partial class ScenarioSavePlayer
    {
        public ScenarioSavePlayer()
        {
            ScenarioSavePlayerNameLink = new HashSet<ScenarioSavePlayerNameLink>();
        }

        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [Column("UserID")]
        public long UserId { get; set; }
        [Column("RaceID")]
        public int RaceId { get; set; }
        [Column("RaceModifierID")]
        public int RaceModifierId { get; set; }
        [Column("SaveID")]
        public int SaveId { get; set; }

        [ForeignKey(nameof(RaceId))]
        [InverseProperty(nameof(ScenarioRace.ScenarioSavePlayer))]
        public virtual ScenarioRace Race { get; set; }
        [ForeignKey(nameof(RaceModifierId))]
        [InverseProperty(nameof(ScenarioRaceModifier.ScenarioSavePlayer))]
        public virtual ScenarioRaceModifier RaceModifier { get; set; }
        [ForeignKey(nameof(SaveId))]
        [InverseProperty(nameof(ScenarioSave.ScenarioSavePlayer))]
        public virtual ScenarioSave Save { get; set; }
        [ForeignKey(nameof(UserId))]
        [InverseProperty(nameof(Users.ScenarioSavePlayer))]
        public virtual Users User { get; set; }
        [InverseProperty("SavePlayer")]
        public virtual ICollection<ScenarioSavePlayerNameLink> ScenarioSavePlayerNameLink { get; set; }
    }
}