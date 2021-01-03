using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class patch_StoryGameSession
{
    public static void Patch()
    {
        On.StoryGameSession.AddPlayer += StoryGameSession_AddPlayer;
        
    }    

    private static void StoryGameSession_AddPlayer(On.StoryGameSession.orig_AddPlayer orig, StoryGameSession self, AbstractCreature player)
    {
        orig.Invoke(self, player);

        foreach (AbstractCreature ac in self.Players)
        {
            if (ac == player && !KarmaAppetite_ExtraInventory.Inventories.ContainsKey((ac.state as PlayerState).playerNumber))
            {
                if (KarmaAppetite_ExtraInventory.InventorySave != null && KarmaAppetite_ExtraInventory.InventorySave != string.Empty)
                {
                    List<AbstractPhysicalObject> content = new List<AbstractPhysicalObject>();

                    string[] arrayPlayers = Regex.Split(KarmaAppetite_ExtraInventory.InventorySave, "<svC>");

                    for (int m = 0; m < arrayPlayers.Length; m++)
                    {

                        string[] arrayContent = Regex.Split(arrayPlayers[m], "<svD>");

                        if (int.Parse(arrayContent[0]) == (player.state as PlayerState).playerNumber)
                        {

                            for (int n = 1; n < arrayContent.Length; n++)
                            {
                                AbstractPhysicalObject apo = null;

                                if (arrayContent[n].Contains("<oA>"))
                                {
                                    apo = SaveState.AbstractPhysicalObjectFromString(self.game.world, arrayContent[n]);
                                }
                                else if (arrayContent[n].Contains("<cA>"))
                                {
                                    apo = SaveState.AbstractCreatureFromString(self.game.world, arrayContent[n], false);
                                }
                                if (apo != null)
                                {
                                    apo.pos = ac.pos;
                                }
                                content.Add(apo);
                            }

                        }

                    }
                    
                    KarmaAppetite_ExtraInventory.Inventories.Add((ac.state as PlayerState).playerNumber, content);
                }
                else
                {
                    KarmaAppetite_ExtraInventory.Inventories.Add((ac.state as PlayerState).playerNumber, new List<AbstractPhysicalObject>());
                }
            }
        }
    }

}
