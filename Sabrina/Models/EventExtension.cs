using System;

namespace Sabrina.Models
{
	partial class Event
	{
		[System.ComponentModel.DataAnnotations.Schema.NotMapped]
		public TimeSpan TriggerTimeSpan
		{
			get => TimeSpan.FromTicks(TriggerTime);
			set => TriggerTime = value.Ticks;
		}
	}
}