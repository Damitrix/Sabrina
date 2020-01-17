using Sabrina.Entities;

namespace Sabrina.Dungeon.Rooms
{
	using Newtonsoft.Json;
	using Sabrina.Models;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using static DungeonTextExtension;
	using static Sabrina.Dungeon.DungeonLogic.Dungeon;

	public class Room
	{
		[JsonIgnore]
		public int LayerID;

		public Guid RoomID;

		public RoomType Type;

		[JsonIgnore]
		private DungeonDifficulty? _difficulty;

		/// <summary>
		/// Constructor for Serialization only.
		/// </summary>
		public Room()
		{
		}

		protected Room(RoomType type)
		{
			RoomID = Guid.NewGuid();
			Type = type;
		}

		public virtual Guid[] AdjacentRooms { get; set; }
		[JsonIgnore] public int WaitAfterMessage { get; private set; } = 0;

		public static Room GenerateRoom(params RoomType[] type)
		{
			Room room;
			if (type.Length == 1)
			{
				room = new Room(type[0]);
			}
			else
			{
				room = new Room(type[Helpers.RandomGenerator.RandomInt(0, type.Length - 1)]);
			}

			return room as Room;
		}

		public string GetText(TextType type)
		{
			using DiscordContext context = new DiscordContext();

			var filteredTexts = context.DungeonText.Where(dT => dT.RoomType == (int)Type && dT.TextType == (int)type);
			var text = filteredTexts.Skip(Helpers.RandomGenerator.RandomInt(0, filteredTexts.Count())).FirstOrDefault();

			if (text == null)
			{
				return null;
			}

			var endText = text.Text;
			if (text.Text.Contains("{0}"))
			{
				var variables = context.DungeonVariable.Where(dv => dv.TextId == text.Id).OrderBy(dv => dv.Position);
				List<string> resolvedVars = new List<string>();

				foreach (var variable in variables)
				{
					resolvedVars.Add(ResolveVariable(variable));
				}

				endText = String.Format(text.Text, resolvedVars.ToArray());
			}

			WaitAfterMessage += 4000;

			return endText;
		}

		public void SetDifficulty(DungeonDifficulty difficulty)
		{
			_difficulty = difficulty;
		}

		private string ResolveNameVariable(DungeonVariable variable)
		{
			using DiscordContext context = new DiscordContext();
			string text = "";

			switch ((DungeonVariableExtension.VariableType)variable.Type)
			{
				case DungeonVariableExtension.VariableType.Name_Mob:
					text = context.DungeonMob.Skip(Helpers.RandomGenerator.RandomInt(0, context.DungeonMob.Count())).First().Name;
					break;
			}

			return text;
		}

		private string ResolveTaskVariable(DungeonVariable variable)
		{
			switch ((DungeonVariableExtension.VariableType)variable.Type)
			{
				case DungeonVariableExtension.VariableType.RandomStrokes:
					var baseStrokes = (int)_difficulty * 30;
					var randomSubtraction = Helpers.RandomGenerator.RandomInt(-(baseStrokes / 4), baseStrokes / 4);
					var endStrokes = baseStrokes - randomSubtraction;

					WaitAfterMessage += baseStrokes / 5 * 1000;

					return endStrokes.ToString();

				case DungeonVariableExtension.VariableType.RandomEdges:
					var add = Helpers.RandomGenerator.RandomInt(0, 2);
					var endEdges = Convert.ToInt16((int)_difficulty * 0.7f + add);

					WaitAfterMessage += endEdges * 30000;

					return endEdges.ToString();

				case DungeonVariableExtension.VariableType.RandomFlicks:
					var baseFlicks = (int)_difficulty * 10;
					var randomFlickSubtraction = Helpers.RandomGenerator.RandomInt(-(baseFlicks / 4), baseFlicks / 4);
					var endFlicks = baseFlicks - randomFlickSubtraction;

					WaitAfterMessage += endFlicks * 1000;

					return endFlicks.ToString();

				case DungeonVariableExtension.VariableType.RandomSlaps:
					var baseSlaps = (int)_difficulty * 5;
					var randomSlapSubtraction = Helpers.RandomGenerator.RandomInt(-(baseSlaps / 4), baseSlaps / 4);
					var endSlaps = baseSlaps - randomSlapSubtraction;

					WaitAfterMessage += endSlaps * 1000;

					return endSlaps.ToString();
			}

			return null;
		}

		private string ResolveTextVariable(DungeonVariable variable)
		{
			TextType textType = 0;

			// Note: Could also be realised through Reflection. Consider for future changes.
			switch ((DungeonVariableExtension.VariableType)variable.Type)
			{
				case DungeonVariableExtension.VariableType.Text_Enter:
					textType = TextType.RoomEnter;
					break;

				case DungeonVariableExtension.VariableType.Text_Greeting:
					textType = TextType.Greeting;
					break;

				case DungeonVariableExtension.VariableType.Text_Main:
					textType = TextType.Main;
					break;

				case DungeonVariableExtension.VariableType.Text_Dismissal:
					textType = TextType.Dismissal;
					break;

				case DungeonVariableExtension.VariableType.Text_Success:
					textType = TextType.Success;
					break;

				case DungeonVariableExtension.VariableType.Text_Failure:
					textType = TextType.Failure;
					break;
			}

			var text = GetText(textType);

			WaitAfterMessage += text.Length * 10;

			return text;
		}

		private string ResolveVariable(DungeonVariable variable)
		{
			var text = "";

			if (variable.Type > (int)DungeonVariableExtension.VariableType.None && variable.Type >= (int)DungeonVariableExtension.VariableType.Text_Enter && variable.Type < (int)DungeonVariableExtension.VariableType.RandomStrokes)
			{
				text = ResolveTextVariable(variable);
			}
			else if (variable.Type >= (int)DungeonVariableExtension.VariableType.RandomStrokes && variable.Type < (int)DungeonVariableExtension.VariableType.Content)
			{
				text = ResolveTaskVariable(variable);
			}
			else if (variable.Type >= (int)DungeonVariableExtension.VariableType.Content && variable.Type < (int)DungeonVariableExtension.VariableType.Name_Mob)
			{
				text = ResolveTaskVariable(variable);
			}
			else if (variable.Type >= (int)DungeonVariableExtension.VariableType.Name_Mob)
			{
				text = ResolveNameVariable(variable);
			}

			return text;
		}
	}
}