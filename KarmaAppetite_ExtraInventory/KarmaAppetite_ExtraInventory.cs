using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Partiality.Modloader;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

public class KarmaAppetite_ExtraInventory : PartialityMod
{

    public KarmaAppetite_ExtraInventory()
    {
        instance = this;
        this.ModID = "KarmaAppetite_ExtraInventory";
        this.Version = "0.1";
        this.author = "DarkGran";
    }

    public static KarmaAppetite_ExtraInventory instance;

    public override void OnEnable()
    {
        base.OnEnable();
        patch_Player.Patch();
        patch_PlayerGraphics.Patch();
        patch_PlayerProgression.Patch();
        patch_SaveState.Patch();
        patch_SlugcatHand.Patch();
        patch_StoryGameSession.Patch();
        Inventories = new Dictionary<int, List<AbstractPhysicalObject>>();
    }

    //INVENTORY

    public static Dictionary<int, List<AbstractPhysicalObject>> Inventories;
    public static bool SaveEnabled = true;
    public static string InventorySave;

    public static int MaxSize = 1;
    public static bool HasSomethingInInventory(Player player)
    {
        if (Inventories.ContainsKey(player.playerState.playerNumber) && Inventories[player.playerState.playerNumber] != null)
        {
            foreach (AbstractPhysicalObject obj in Inventories[player.playerState.playerNumber])
            {
                if (obj != null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static bool HasSpace(Player player)
    {
        return Inventories.ContainsKey(player.playerState.playerNumber) && Inventories[player.playerState.playerNumber] != null && Inventories[player.playerState.playerNumber].Count < MaxSize;
    }

    public static void PutInInventory(Player player)
    {
        if (player.grasps[0] == null)
        {
            return;
        }

        List<AbstractPhysicalObject> content = new List<AbstractPhysicalObject>();
        foreach (AbstractPhysicalObject apo in Inventories[player.playerState.playerNumber])
        {
            content.Add(apo);
        }
        content.Add(player.grasps[0].grabbed.abstractPhysicalObject);
        Inventories.Remove(player.playerState.playerNumber);
        Inventories.Add(player.playerState.playerNumber, content);

        player.ReleaseGrasp(0);
        Inventories[player.playerState.playerNumber][Inventories[player.playerState.playerNumber].Count - 1].realizedObject.RemoveFromRoom();
        Inventories[player.playerState.playerNumber][Inventories[player.playerState.playerNumber].Count - 1].Abstractize(player.abstractCreature.pos);
        Inventories[player.playerState.playerNumber][Inventories[player.playerState.playerNumber].Count - 1].Room.RemoveEntity(Inventories[player.playerState.playerNumber][Inventories[player.playerState.playerNumber].Count - 1]);
        BodyChunk mainBodyChunk = player.mainBodyChunk;
        mainBodyChunk.vel.y = mainBodyChunk.vel.y + 2f;
        player.room.PlaySound(SoundID.Slugcat_Swallow_Item, player.mainBodyChunk);
    }

    public static void PullFromInventory(Player player)
    {
        if (Inventories[player.playerState.playerNumber].Count == 0)
        {
            return;
        }
        player.room.abstractRoom.AddEntity(Inventories[player.playerState.playerNumber][Inventories[player.playerState.playerNumber].Count - 1]);
        Inventories[player.playerState.playerNumber][Inventories[player.playerState.playerNumber].Count - 1].pos = player.abstractCreature.pos;
        Inventories[player.playerState.playerNumber][Inventories[player.playerState.playerNumber].Count - 1].RealizeInRoom();
        Vector2 vector = player.bodyChunks[0].pos;
        Vector2 a = Custom.DirVec(player.bodyChunks[1].pos, player.bodyChunks[0].pos);
        bool flag = false;
        if (Mathf.Abs(player.bodyChunks[0].pos.y - player.bodyChunks[1].pos.y) > Mathf.Abs(player.bodyChunks[0].pos.x - player.bodyChunks[1].pos.x) && player.bodyChunks[0].pos.y > player.bodyChunks[1].pos.y)
        {
            vector += Custom.DirVec(player.bodyChunks[1].pos, player.bodyChunks[0].pos) * 5f;
            a *= -1f;
            a.x += 0.4f * (float)player.flipDirection;
            a.Normalize();
            flag = true;
        }
        Inventories[player.playerState.playerNumber][Inventories[player.playerState.playerNumber].Count - 1].realizedObject.firstChunk.HardSetPosition(vector);
        Inventories[player.playerState.playerNumber][Inventories[player.playerState.playerNumber].Count - 1].realizedObject.firstChunk.vel = Vector2.ClampMagnitude((a * 2f + Custom.RNV() * Random.value) / Inventories[player.playerState.playerNumber][Inventories[player.playerState.playerNumber].Count - 1].realizedObject.firstChunk.mass, 6f);
        player.bodyChunks[0].pos -= a * 2f;
        player.bodyChunks[0].vel -= a * 2f;
        if (player.graphicsModule != null)
        {
            (player.graphicsModule as PlayerGraphics).head.vel += Custom.RNV() * Random.value * 3f;
        }
        for (int i = 0; i < 3; i++)
        {
            player.room.AddObject(new WaterDrip(vector + Custom.RNV() * Random.value * 1.5f, Custom.RNV() * 3f * Random.value + a * Mathf.Lerp(2f, 6f, Random.value), false));
        }
        player.room.PlaySound(SoundID.Slugcat_Regurgitate_Item, player.mainBodyChunk);
        if (Inventories[player.playerState.playerNumber][Inventories[player.playerState.playerNumber].Count - 1].realizedObject is Hazer && player.graphicsModule != null)
        {
            (Inventories[player.playerState.playerNumber][Inventories[player.playerState.playerNumber].Count - 1].realizedObject as Hazer).SpitOutByPlayer(PlayerGraphics.SlugcatColor(player.playerState.slugcatCharacter));
        }
        if (flag && player.FreeHand() > -1)
        {
            player.SlugcatGrab(Inventories[player.playerState.playerNumber][Inventories[player.playerState.playerNumber].Count - 1].realizedObject, player.FreeHand());
        }

        List<AbstractPhysicalObject> content = new List<AbstractPhysicalObject>();
        foreach (AbstractPhysicalObject apo in Inventories[player.playerState.playerNumber])
        {
            content.Add(apo);
        }
        content.RemoveAt(content.Count - 1);
        Inventories.Remove(player.playerState.playerNumber);
        Inventories.Add(player.playerState.playerNumber, content);

    }


}