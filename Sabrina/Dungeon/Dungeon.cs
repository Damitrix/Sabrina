using Sabrina.Dungeon.Rooms;
using Sabrina.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using static Sabrina.Dungeon.Rooms.Room;

namespace Sabrina.Dungeon
{
	public class DungeonLogic
	{
		public class Dungeon
		{
			public List<Room> Rooms;
			private readonly int startChance = 1000;
			private int chanceToDivert;

			public Dungeon(int level, DungeonLength length, DungeonDifficulty difficulty)
			{
				this.chanceToDivert = this.startChance;
				bool isFinished = false;

				this.Rooms = new List<Room>();
				int cLayer = 0;

				while (!isFinished)
				{
					var layerToAdd = GenerateLayer(cLayer, length, difficulty);
					this.Rooms.AddRange(layerToAdd);
					cLayer++;

					if (layerToAdd.Count == 1 && cLayer > 2)
					{
						isFinished = true;
					}
				}

				if (level == 0)
				{
					//Do something later
				}
			}

			public enum DungeonDifficulty
			{
				ChildsPlay = 1,
				Easy = 2,
				Moderate = 3,
				Medium = 4,
				Hard = 5,
				VeryHard = 6,
				Catastrophical = 7
			}

			public enum DungeonLength
			{
				Tiny = 10,
				Small = 30,
				Medium = 60,
				Long = 73,
				Xxl = 80,
				Endless = 100
			}

			private List<Room> GenerateLayer(int layerID, DungeonLength length, DungeonDifficulty difficulty)
			{
				List<Room> generatedLayer = new List<Room>();
				int prevRoomIndex = 0;
				int skipRooms = 0;

				if (layerID - 1 == -1)
				{
					var room = this.GenerateNormalRoom(difficulty, length);
					room.LayerID = 0;
					generatedLayer.Add(room);
					return generatedLayer;
				}

				int chanceDifference = this.startChance - this.chanceToDivert;

				int minNumberForDouble = startChance / 3 * 2;
				int minNumberForSingle = startChance / 2;

				int chance = Helpers.RandomGenerator.RandomInt(0, this.startChance);

				this.chanceToDivert -= Convert.ToInt32((int)DungeonLength.Endless - (int)length);

				foreach (var prevRoom in this.Rooms.Where(r => r.LayerID == layerID - 1))
				{
					if (skipRooms > 0)
					{
						prevRoomIndex++;
						skipRooms--;
						continue;
					}

					var chanceToRandom = Helpers.RandomGenerator.RandomInt(0, 5);
					var tempChance = 0;

					if (chanceToRandom == 0)
					{
						tempChance = Helpers.RandomGenerator.RandomInt(-(((int)DungeonLength.Endless - (int)length) * 2), ((int)DungeonLength.Endless - (int)length) * 2);
					}

					if (this.chanceToDivert + tempChance > minNumberForDouble)
					{
						//Generate Two Rooms
						var room1 = this.GenerateNormalRoom(difficulty, length);
						room1.AdjacentRooms = new Guid[] { prevRoom.RoomID };
						room1.LayerID = layerID;
						generatedLayer.Add(room1);
						var room2 = this.GenerateNormalRoom(difficulty, length);
						room2.AdjacentRooms = new Guid[] { prevRoom.RoomID };
						room2.LayerID = layerID;
						generatedLayer.Add(room2);
					}
					else if (this.chanceToDivert + tempChance > minNumberForSingle)
					{
						//Generate a Single Room
						var room = this.GenerateNormalRoom(difficulty, length);
						room.LayerID = layerID;
						room.AdjacentRooms = new Guid[] { prevRoom.RoomID };
						generatedLayer.Add(room);
					}
					else
					{
						//Converge with another Room
						Random rnd = new Random();
						int convergeNum = rnd.Next(1, 3);

						var adjacentRooms = new List<Room>
						{
							prevRoom
						};

						//If enough Rooms are left, converge with either 2 or 3 rooms
						if (this.Rooms.Count(r => r.LayerID == layerID - 1) > prevRoomIndex + 1)
						{
							adjacentRooms.Add(this.Rooms.Where(r => r.LayerID == layerID - 1).ElementAt(prevRoomIndex + 1));
							skipRooms++;

							if (convergeNum == 2 && this.Rooms.Count(r => r.LayerID == layerID - 1) > prevRoomIndex + 2)
							{
								adjacentRooms.Add(this.Rooms.Where(r => r.LayerID == layerID - 1).ElementAt(prevRoomIndex + 2));
								skipRooms++;
							}
						}

						var room = this.GenerateNormalRoom(difficulty, length);
						room.LayerID = layerID;
						room.AdjacentRooms = adjacentRooms.Select(r => r.RoomID).ToArray();
						generatedLayer.Add(room);
					}

					prevRoomIndex++;
				}

				if (!generatedLayer.Any())
				{
					throw new NullReferenceException("Could not add a Room to the Current Layer.");
				}
				return generatedLayer;
			}

			private Room GenerateNormalRoom(DungeonDifficulty difficulty, DungeonLength length)
			{
				int maxLootRoomChance = (int)DungeonDifficulty.Catastrophical * 10 * (int)length;
				int lootRoomChance = (int)difficulty;

				Room generatedRoom;
				if (Helpers.RandomGenerator.RandomInt(0, maxLootRoomChance) > lootRoomChance)
				{
					generatedRoom = GenerateRoom(DungeonTextExtension.RoomType.Loot);
				}
				else
				{
					generatedRoom = GenerateRoom(DungeonTextExtension.RoomType.LesserMob, DungeonTextExtension.RoomType.Boss);
				}

				if (generatedRoom == null)
				{
					throw new NullReferenceException("A suiting Room could not be generated");
				}
				return generatedRoom;
			}

			//private Room GenerateStartRoom()
			//{
			//	return GenerateRoom(DungeonTextExtension.RoomType.Start);
			//}
		}

		public class Item
		{
			public int Id;
			public string Name;
		}
	}
}