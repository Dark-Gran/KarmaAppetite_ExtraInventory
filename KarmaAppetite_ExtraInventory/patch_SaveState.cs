using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using RWCustom;

public class patch_SaveState
{
    public static void Patch()
    {
        On.SaveState.SaveToString += SaveState_SaveToString;
        On.SaveState.LoadGame += SaveState_LoadGame;
        On.SaveState.SessionEnded += SaveState_SessionEnded;
    }

    private static void SaveState_SessionEnded(On.SaveState.orig_SessionEnded orig, SaveState self, RainWorldGame game, bool survived, bool newMalnourished)
    {
		KarmaAppetite_ExtraInventory.SaveEnabled = survived;
		orig.Invoke(self, game, survived, newMalnourished);
		KarmaAppetite_ExtraInventory.SaveEnabled = true;
		if (!survived)
        {
			KarmaAppetite_ExtraInventory.Inventories.Clear();
        }
	}

    private static string SaveState_SaveToString(On.SaveState.orig_SaveToString orig, SaveState self)
    {
		Debug.Log("SAVE!");
		string text = string.Empty;
		string text2 = text;
		text = string.Concat(new object[]
		{
			text2,
			"SAV STATE NUMBER<svB>",
			self.saveStateNumber,
			"<svA>"
		});
		text2 = text;
		text = string.Concat(new object[]
		{
			text2,
			"SEED<svB>",
			self.seed,
			"<svA>"
		});
		text2 = text;
		text = string.Concat(new object[]
		{
			text2,
			"VERSION<svB>",
			self.gameVersion,
			"<svA>"
		});
		text2 = text;
		text = string.Concat(new object[]
		{
			text2,
			"INITVERSION<svB>",
			self.initiatedInGameVersion,
			"<svA>"
		});
		text2 = text;
		text = string.Concat(new object[]
		{
			text2,
			"WORLDVERSION<svB>",
			self.worldVersion,
			"<svA>"
		});
		text = text + "DENPOS<svB>" + self.denPosition + "<svA>";
		text2 = text;
		text = string.Concat(new object[]
		{
			text2,
			"CYCLENUM<svB>",
			self.cycleNumber,
			"<svA>"
		});
		text2 = text;
		text = string.Concat(new object[]
		{
			text2,
			"FOOD<svB>",
			self.food,
			"<svA>"
		});
		text2 = text;
		text = string.Concat(new object[]
		{
			text2,
			"NEXTID<svB>",
			self.nextIssuedID,
			"<svA>"
		});
		if (self.theGlow)
		{
			text += "HASTHEGLOW<svA>";
		}
		if (self.guideOverseerDead)
		{
			text += "GUIDEOVERSEERDEAD<svA>";
		}
		if (self.respawnCreatures.Count > 0)
		{
			text += "RESPAWNS<svB>";
			for (int i = 0; i < self.respawnCreatures.Count; i++)
			{
				text = text + self.respawnCreatures[i] + ((i >= self.respawnCreatures.Count - 1) ? string.Empty : ".");
			}
			text += "<svA>";
		}
		if (self.waitRespawnCreatures.Count > 0)
		{
			text += "WAITRESPAWNS<svB>";
			for (int j = 0; j < self.waitRespawnCreatures.Count; j++)
			{
				text = text + self.waitRespawnCreatures[j] + ((j >= self.waitRespawnCreatures.Count - 1) ? string.Empty : ".");
			}
			text += "<svA>";
		}
		if (self.creatureCommunitiesString != null)
		{
			text = text + "COMMUNITIES<svB>" + self.creatureCommunitiesString + "<svA>";
		}
		for (int k = 0; k < self.regionStates.Length; k++)
		{
			if (self.regionStates[k] != null)
			{
				text = text + "REGIONSTATE<svB>" + self.regionStates[k].SaveToString() + "<svA>";
			}
			else if (self.regionLoadStrings[k] != null)
			{
				text = text + "REGIONSTATE<svB>" + self.regionLoadStrings[k] + "<svA>";
			}
		}
		text = text + "DEATHPERSISTENTSAVEDATA<svB>" + self.deathPersistentSaveData.SaveToString(false, false) + "<svA>";
		string text3 = self.miscWorldSaveData.ToString();
		if (text3 != string.Empty)
		{
			text = text + "MISCWORLDSAVEDATA<svB>" + text3 + "<svA>";
		}
		if (self.swallowedItems != null)
		{
			text += "SWALLOWEDITEMS<svB>";
			for (int l = 0; l < self.swallowedItems.Length; l++)
			{
				text = text + self.swallowedItems[l] + ((l >= self.swallowedItems.Length - 1) ? string.Empty : "<svB>");
			}
			text += "<svA>";
		}
		if (self.dreamsState != null)
		{
			text = text + "DREAMSSTATE<svB>" + self.dreamsState.ToString() + "<svA>";
		}
		text2 = text;
		text = string.Concat(new object[]
		{
			text2,
			"TOTFOOD<svB>",
			self.totFood,
			"<svA>"
		});
		text2 = text;
		text = string.Concat(new object[]
		{
			text2,
			"TOTTIME<svB>",
			self.totTime,
			"<svA>"
		});
		text2 = text;
		text = string.Concat(new object[]
		{
			text2,
			"CURRVERCYCLES<svB>",
			self.cyclesInCurrentWorldVersion,
			"<svA>"
		});
		if (self.kills.Count > 0)
		{
			text += "KILLS<svB>";
			for (int m = 0; m < self.kills.Count; m++)
			{
				text2 = text;
				text = string.Concat(new object[]
				{
					text2,
					self.kills[m].Key,
					"<svD>",
					self.kills[m].Value,
					(m >= self.kills.Count - 1) ? string.Empty : "<svC>"
				});
			}
			text += "<svA>";
		}
		if (self.redExtraCycles)
		{
			text += "REDEXTRACYCLES<svA>";
		}
		if (KarmaAppetite_ExtraInventory.SaveEnabled && KarmaAppetite_ExtraInventory.Inventories.Count > 0)
        {

			text += "EXTRAINVENTORY<svB>";
			int m = 0;

			foreach (var kvp in KarmaAppetite_ExtraInventory.Inventories)
				{

				if (kvp.Value.Count > 0)
				{	

					text = text + kvp.Key + "<svD>";

					for (int n = 0; n < kvp.Value.Count; n++)
					{
						text2 = text;
						text = string.Concat(new object[]
						{
						text2,
						kvp.Value[n].ToString(),
						(n >= kvp.Value.Count - 1) ? string.Empty : "<svD>"
						});
					}

					text += (m >= KarmaAppetite_ExtraInventory.Inventories.Count - 1) ? string.Empty : "svC";
				}
				m++;
			}
			text += "<svA>";
		}
		return text;
	}

	private static void SaveState_LoadGame(On.SaveState.orig_LoadGame orig, SaveState self, string str, RainWorldGame game)
    {
		self.loaded = true;
		self.redExtraCycles = false;
		self.initiatedInGameVersion = 0;
		if (str == string.Empty)
		{
			Debug.Log("NOTHING TO LOAD - START CLEAR");
			self.denPosition = ((self.saveStateNumber != 2) ? "SU_C04" : "LF_H01");
		}
		else
		{
			string[] array = Regex.Split(str, "<svA>");
			List<string[]> list = new List<string[]>();
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = Regex.Split(array[i], "<svB>");
				if (array2.Length > 0 && array2[0].Length > 0)
				{
					list.Add(array2);
				}
			}
			int j = 0;
			while (j < list.Count)
			{
				string text = list[j][0];
				if (text == null)
				{
					goto IL_66F;
				}
				
				Dictionary<string, int> dictionary = new Dictionary<string, int>(24);
				dictionary.Add("SAV STATE NUMBER", 0);
				dictionary.Add("DENPOS", 1);
				dictionary.Add("CYCLENUM", 2);
				dictionary.Add("FOOD", 3);
				dictionary.Add("NEXTID", 4);
				dictionary.Add("HASTHEGLOW", 5);
				dictionary.Add("GUIDEOVERSEERDEAD", 6);
				dictionary.Add("RESPAWNS", 7);
				dictionary.Add("WAITRESPAWNS", 8);
				dictionary.Add("REGIONSTATE", 9);
				dictionary.Add("COMMUNITIES", 10);
				dictionary.Add("MISCWORLDSAVEDATA", 11);
				dictionary.Add("DEATHPERSISTENTSAVEDATA", 12);
				dictionary.Add("SWALLOWEDITEMS", 13);
				dictionary.Add("VERSION", 14);
				dictionary.Add("INITVERSION", 15);
				dictionary.Add("WORLDVERSION", 16);
				dictionary.Add("SEED", 17);
				dictionary.Add("DREAMSSTATE", 18);
				dictionary.Add("TOTFOOD", 19);
				dictionary.Add("TOTTIME", 20);
				dictionary.Add("CURRVERCYCLES", 21);
				dictionary.Add("KILLS", 22);
				dictionary.Add("REDEXTRACYCLES", 23);
				dictionary.Add("EXTRAINVENTORY", 24);
				
				int num;
				if (!dictionary.TryGetValue(text, out num))
				{
					goto IL_66F;
				}
				switch (num)
				{
					case 0:
						break;
					case 1:
						self.denPosition = list[j][1];
						break;
					case 2:
						self.cycleNumber = int.Parse(list[j][1]);
						break;
					case 3:
						self.food = int.Parse(list[j][1]);
						break;
					case 4:
						if (game != null)
						{
							game.nextIssuedId = int.Parse(list[j][1]);
						}
						break;
					case 5:
						self.theGlow = true;
						break;
					case 6:
						self.guideOverseerDead = true;
						break;
					case 7:
						{
							string[] array3 = list[j][1].Split(new char[]
							{
						'.'
							});
							for (int k = 0; k < array3.Length; k++)
							{
								if (k < array3.Length)
								{
									self.respawnCreatures.Add(int.Parse(array3[k]));
								}
							}
							break;
						}
					case 8:
						{
							string[] array4 = list[j][1].Split(new char[]
							{
						'.'
							});
							for (int l = 0; l < array4.Length; l++)
							{
								if (l < array4.Length)
								{
									self.waitRespawnCreatures.Add(int.Parse(array4[l]));
								}
							}
							break;
						}
					case 9:
						{
							string[] array5 = Regex.Split(list[j][1], "<rgA>");
							for (int m = 0; m < array5.Length; m++)
							{
								if (Regex.Split(array5[m], "<rgB>")[0] == "REGIONNAME")
								{
									for (int n = 0; n < self.progression.regionNames.Length; n++)
									{
										if (self.progression.regionNames[n] == Regex.Split(array5[m], "<rgB>")[1])
										{
											self.regionLoadStrings[n] = list[j][1];
											break;
										}
									}
									break;
								}
							}
							break;
						}
					case 10:
						self.creatureCommunitiesString = list[j][1];
						break;
					case 11:
						self.miscWorldSaveData.FromString(list[j][1]);
						break;
					case 12:
						self.deathPersistentSaveData.FromString(list[j][1]);
						if (self.saveStateNumber == 1)
						{
							self.deathPersistentSaveData.howWellIsPlayerDoing = -1f;
						}
						else if (self.saveStateNumber == 2)
						{
							self.deathPersistentSaveData.howWellIsPlayerDoing = 1f;
						}
						break;
					case 13:
						self.swallowedItems = new string[list[j].Length - 1];
						for (int num2 = 1; num2 < list[j].Length; num2++)
						{
							self.swallowedItems[num2 - 1] = list[j][num2];
						}
						break;
					case 14:
						self.gameVersion = int.Parse(list[j][1]);
						break;
					case 15:
						self.initiatedInGameVersion = int.Parse(list[j][1]);
						break;
					case 16:
						self.worldVersion = int.Parse(list[j][1]);
						break;
					case 17:
						self.seed = int.Parse(list[j][1]);
						break;
					case 18:
						self.dreamsState.FromString(list[j][1]);
						break;
					case 19:
						self.totFood = int.Parse(list[j][1]);
						break;
					case 20:
						self.totTime = int.Parse(list[j][1]);
						break;
					case 21:
						self.cyclesInCurrentWorldVersion = int.Parse(list[j][1]);
						break;
					case 22:
						{
							self.kills.Clear();
							string[] array6 = Regex.Split(list[j][1], "<svC>");
							for (int num3 = 0; num3 < array6.Length; num3++)
							{
								self.kills.Add(new KeyValuePair<IconSymbol.IconSymbolData, int>(IconSymbol.IconSymbolData.IconSymbolDataFromString(Regex.Split(array6[num3], "<svD>")[0]), int.Parse(Regex.Split(array6[num3], "<svD>")[1])));
							}
							break;
						}
					case 23:
						self.redExtraCycles = true;
						break;
					case 24:
						{
							KarmaAppetite_ExtraInventory.InventorySave = list[j][1];
							KarmaAppetite_ExtraInventory.Inventories.Clear();
							break;
						}
					default:
						goto IL_66F;
				}
			IL_68A:
				j++;
				continue;
			IL_66F:
				Debug.Log("Unknown save state load string");
				Debug.Log(list[j][0]);
				goto IL_68A;
			}
		}
		if (game != null)
		{
			if (game.setupValues.cheatKarma > 0)
			{
				self.deathPersistentSaveData.karma = game.setupValues.cheatKarma - 1;
				self.deathPersistentSaveData.karmaCap = Math.Max(self.deathPersistentSaveData.karmaCap, self.deathPersistentSaveData.karma);
			}
			if (game.setupValues.theMark)
			{
				self.deathPersistentSaveData.theMark = true;
			}
			if (self.worldVersion != game.rainWorld.worldVersion)
			{
				game.manager.rainWorld.progression.miscProgressionData.redUnlocked = true;
				BackwardsCompability.UpdateWorldVersion(self, game.rainWorld.worldVersion, game.rainWorld.progression);
				self.cyclesInCurrentWorldVersion = 0;
			}
		}
		else
		{
			Debug.Log("LOADING SAV WITH NULL GAME");
		}
		if (self.deathPersistentSaveData.redsDeath && self.cycleNumber < RedsIllness.RedsCycles(self.redExtraCycles))
		{
			self.deathPersistentSaveData.redsDeath = false;
		}
	}

}
