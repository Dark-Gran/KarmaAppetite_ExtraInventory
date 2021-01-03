using System;
using RWCustom;
using UnityEngine;

public class patch_Player
{
	public static void Patch()
	{
		On.Player.GrabUpdate += Player_GrabUpdate;
	}

	private static void Player_GrabUpdate(On.Player.orig_GrabUpdate orig, Player self, bool eu)
	{
		if (self.spearOnBack != null)
		{
			self.spearOnBack.Update(eu);
		}
		bool flag = self.input[0].x == 0 && self.input[0].y == 0 && !self.input[0].jmp && !self.input[0].thrw && self.mainBodyChunk.submersion < 0.5f;
		bool flag2 = false;
		bool flag3 = false;
		if (self.input[0].pckp && !self.input[1].pckp && self.switchHandsProcess == 0f)
		{
			bool flag4 = self.grasps[0] != null || self.grasps[1] != null;
			if (self.grasps[0] != null && (self.Grabability(self.grasps[0].grabbed) == Player.ObjectGrabability.TwoHands || self.Grabability(self.grasps[0].grabbed) == Player.ObjectGrabability.Drag))
			{
				flag4 = false;
			}
			if (flag4)
			{
				if (self.switchHandsCounter == 0)
				{
					self.switchHandsCounter = 15;
				}
				else
				{
					self.room.PlaySound(SoundID.Slugcat_Switch_Hands_Init, self.mainBodyChunk);
					self.switchHandsProcess = 0.01f;
					self.wantToPickUp = 0;
					self.noPickUpOnRelease = 20;
				}
			}
			else
			{
				self.switchHandsProcess = 0f;
			}
		}
		if (self.switchHandsProcess > 0f)
		{
			float num = self.switchHandsProcess;
			self.switchHandsProcess += 0.083333336f;
			if (num < 0.5f && self.switchHandsProcess >= 0.5f)
			{
				self.room.PlaySound(SoundID.Slugcat_Switch_Hands_Complete, self.mainBodyChunk);
				self.SwitchGrasps(0, 1);
			}
			if (self.switchHandsProcess >= 1f)
			{
				self.switchHandsProcess = 0f;
			}
		}
		int num2 = -1;
		if (flag)
		{
			int num3 = -1;
			int num4 = -1;
			int num5 = 0;
			while (num3 < 0 && num5 < 2)
			{
				if (self.grasps[num5] != null && self.grasps[num5].grabbed is IPlayerEdible && (self.grasps[num5].grabbed as IPlayerEdible).Edible)
				{
					num3 = num5;
				}
				num5++;
			}
			if ((num3 == -1 || (self.FoodInStomach >= self.MaxFoodInStomach && !(self.grasps[num3].grabbed is KarmaFlower) && !(self.grasps[num3].grabbed is Mushroom))) && (self.objectInStomach == null || KarmaAppetite_ExtraInventory.HasSpace(self) || self.CanPutSpearToBack))
			{
				int num6 = 0;
				while (num4 < 0 && num2 < 0 && num6 < 2)
				{
					if (self.grasps[num6] != null)
					{
						if (self.CanPutSpearToBack && self.grasps[num6].grabbed is Spear)
						{
							num2 = num6;
						}
						else if (self.CanBeSwallowed(self.grasps[num6].grabbed))
						{
							num4 = num6;
						}
					}
					num6++;
				}
			}
			if (num3 > -1 && self.noPickUpOnRelease < 1)
			{
				if (!self.input[0].pckp)
				{
					int num7 = 1;
					while (num7 < 10 && self.input[num7].pckp)
					{
						num7++;
					}
					if (num7 > 1 && num7 < 10)
					{
						self.PickupPressed();
					}
				}
			}
			else if (self.input[0].pckp && !self.input[1].pckp)
			{
				self.PickupPressed();
			}
			if (self.input[0].pckp)
			{
				if (num2 > -1 || self.CanRetrieveSpearFromBack)
				{
					self.spearOnBack.increment = true;
				}
				else if (num4 > -1 || self.objectInStomach != null || KarmaAppetite_ExtraInventory.HasSomethingInInventory(self))
				{
					flag3 = true;
				}
			}
			if (num3 > -1 && self.wantToPickUp < 1 && (self.input[0].pckp || self.eatCounter <= 15) && self.Consious && Custom.DistLess(self.mainBodyChunk.pos, self.mainBodyChunk.lastPos, 3.6f))
			{
				if (self.graphicsModule != null)
				{
					(self.graphicsModule as PlayerGraphics).LookAtObject(self.grasps[num3].grabbed);
				}
				flag2 = true;
				if (self.FoodInStomach < self.MaxFoodInStomach || self.grasps[num3].grabbed is KarmaFlower || self.grasps[num3].grabbed is Mushroom)
				{
					flag3 = false;
					if (self.spearOnBack != null)
					{
						self.spearOnBack.increment = false;
					}
					if (self.eatCounter < 1)
					{
						self.eatCounter = 15;
						self.BiteEdibleObject(eu);
					}
				}
				else if (self.eatCounter < 20 && self.room.game.cameras[0].hud != null)
				{
					self.room.game.cameras[0].hud.foodMeter.RefuseFood();
				}
			}
		}
		else if (self.input[0].pckp && !self.input[1].pckp)
		{
			self.PickupPressed();
		}
		else
		{
			if (self.CanPutSpearToBack)
			{
				for (int i = 0; i < 2; i++)
				{
					if (self.grasps[i] != null && self.grasps[i].grabbed is Spear)
					{
						num2 = i;
						break;
					}
				}
			}
			if (self.input[0].pckp && (num2 > -1 || self.CanRetrieveSpearFromBack))
			{
				self.spearOnBack.increment = true;
			}
		}
		if (self.input[0].pckp && self.grasps[0] != null && self.grasps[0].grabbed is Creature && self.CanEatMeat(self.grasps[0].grabbed as Creature) && (self.grasps[0].grabbed as Creature).Template.meatPoints > 0)
		{
			self.eatMeat++;
			self.EatMeatUpdate();
			if (self.spearOnBack != null)
			{
				self.spearOnBack.increment = false;
				self.spearOnBack.interactionLocked = true;
			}
			if (self.eatMeat % 80 == 0 && ((self.grasps[0].grabbed as Creature).State.meatLeft <= 0 || self.FoodInStomach >= self.MaxFoodInStomach))
			{
				self.eatMeat = 0;
				self.wantToPickUp = 0;
				self.TossObject(0, eu);
				self.ReleaseGrasp(0);
				self.standing = true;
			}
			return;
		}
		if (!self.input[0].pckp && self.grasps[0] != null && self.eatMeat > 60)
		{
			self.eatMeat = 0;
			self.wantToPickUp = 0;
			self.TossObject(0, eu);
			self.ReleaseGrasp(0);
			self.standing = true;
			return;
		}
		self.eatMeat = Custom.IntClamp(self.eatMeat - 1, 0, 50);
		if (flag2 && self.eatCounter > 0)
		{
			self.eatCounter--;
		}
		else if (!flag2 && self.eatCounter < 40)
		{
			self.eatCounter++;
		}
		if (flag3)
		{
			self.swallowAndRegurgitateCounter++;

			if (self.grasps[0] == null) //Try Spit
			{
				if ((KarmaAppetite_ExtraInventory.HasSomethingInInventory(self) || self.objectInStomach != null) && self.swallowAndRegurgitateCounter > 110) 
				{
					
					if (KarmaAppetite_ExtraInventory.HasSomethingInInventory(self)) {
						KarmaAppetite_ExtraInventory.PullFromInventory(self); 
					}
					else
                    {
						self.Regurgitate();
					}

					if (self.spearOnBack != null)
					{
						self.spearOnBack.interactionLocked = true;
					}
					self.swallowAndRegurgitateCounter = 0;
				}
            }
			else //Try Swallow
			{
				if ((self.objectInStomach == null || KarmaAppetite_ExtraInventory.HasSpace(self)) && self.swallowAndRegurgitateCounter > 90)
				{
					if (self.CanBeSwallowed(self.grasps[0].grabbed))
					{
						self.bodyChunks[0].pos += Custom.DirVec(self.grasps[0].grabbed.firstChunk.pos, self.bodyChunks[0].pos) * 2f;

						if (self.objectInStomach == null)
						{
							self.SwallowObject(0);
						}
						else
                        {
							KarmaAppetite_ExtraInventory.PutInInventory(self);
                        }

						if (self.spearOnBack != null)
						{
							self.spearOnBack.interactionLocked = true;
						}
						self.swallowAndRegurgitateCounter = 0;
						(self.graphicsModule as PlayerGraphics).swallowing = 20;

					}
				}
            } 
		}
		else
		{
			self.swallowAndRegurgitateCounter = 0;
		}
		for (int k = 0; k < self.grasps.Length; k++)
		{
			if (self.grasps[k] != null && self.grasps[k].grabbed.slatedForDeletetion)
			{
				self.ReleaseGrasp(k);
			}
		}
		if (self.grasps[0] != null && self.Grabability(self.grasps[0].grabbed) == Player.ObjectGrabability.TwoHands)
		{
			self.pickUpCandidate = null;
		}
		else
		{
			PhysicalObject physicalObject = (self.dontGrabStuff >= 1) ? null : self.PickupCandidate(20f);
			if (self.pickUpCandidate != physicalObject && physicalObject != null && physicalObject is PlayerCarryableItem)
			{
				(physicalObject as PlayerCarryableItem).Blink();
			}
			self.pickUpCandidate = physicalObject;
		}
		if (self.switchHandsCounter > 0)
		{
			self.switchHandsCounter--;
		}
		if (self.wantToPickUp > 0)
		{
			self.wantToPickUp--;
		}
		if (self.wantToThrow > 0)
		{
			self.wantToThrow--;
		}
		if (self.noPickUpOnRelease > 0)
		{
			self.noPickUpOnRelease--;
		}
		if (self.input[0].thrw && !self.input[1].thrw)
		{
			self.wantToThrow = 5;
		}
		if (self.wantToThrow > 0)
		{
			for (int l = 0; l < 2; l++)
			{
				if (self.grasps[l] != null && self.IsObjectThrowable(self.grasps[l].grabbed))
				{
					self.ThrowObject(l, eu);
					self.wantToThrow = 0;
					break;
				}
			}
		}
		if (self.wantToPickUp > 0)
		{
			bool flag5 = true;
			if (self.animation == Player.AnimationIndex.DeepSwim)
			{
				if (self.grasps[0] == null && self.grasps[1] == null)
				{
					flag5 = false;
				}
				else
				{
					for (int m = 0; m < 10; m++)
					{
						if (self.input[m].y > -1 || self.input[m].x != 0)
						{
							flag5 = false;
							break;
						}
					}
				}
			}
			else
			{
				for (int n = 0; n < 5; n++)
				{
					if (self.input[n].y > -1)
					{
						flag5 = false;
						break;
					}
				}
			}
			if (self.grasps[0] != null && self.HeavyCarry(self.grasps[0].grabbed))
			{
				flag5 = true;
			}
			if (flag5)
			{
				int num8 = -1;
				for (int num9 = 0; num9 < 2; num9++)
				{
					if (self.grasps[num9] != null)
					{
						num8 = num9;
						break;
					}
				}
				if (num8 > -1)
				{
					self.wantToPickUp = 0;
					self.ReleaseObject(num8, eu);
				}
				else if (self.spearOnBack != null && self.spearOnBack.spear != null && self.mainBodyChunk.ContactPoint.y < 0)
				{
					self.room.socialEventRecognizer.CreaturePutItemOnGround(self.spearOnBack.spear, self);
					self.spearOnBack.DropSpear();
				}
			}
			else if (self.pickUpCandidate != null)
			{
				if (self.pickUpCandidate is Spear && self.CanPutSpearToBack && ((self.grasps[0] != null && self.Grabability(self.grasps[0].grabbed) >= Player.ObjectGrabability.BigOneHand) || (self.grasps[1] != null && self.Grabability(self.grasps[1].grabbed) >= Player.ObjectGrabability.BigOneHand) || (self.grasps[0] != null && self.grasps[1] != null)))
				{
					Debug.Log("spear straight to back");
					self.room.PlaySound(SoundID.Slugcat_Switch_Hands_Init, self.mainBodyChunk);
					self.spearOnBack.SpearToBack(self.pickUpCandidate as Spear);
				}
				else
				{
					int num10 = 0;
					for (int num11 = 0; num11 < 2; num11++)
					{
						if (self.grasps[num11] == null)
						{
							num10++;
						}
					}
					if (self.Grabability(self.pickUpCandidate) == Player.ObjectGrabability.TwoHands && num10 < 4)
					{
						for (int num12 = 0; num12 < 2; num12++)
						{
							if (self.grasps[num12] != null)
							{
								self.ReleaseGrasp(num12);
							}
						}
					}
					else if (num10 == 0)
					{
						for (int num13 = 0; num13 < 2; num13++)
						{
							if (self.grasps[num13] != null && self.grasps[num13].grabbed is Fly)
							{
								self.ReleaseGrasp(num13);
								break;
							}
						}
					}
					for (int num14 = 0; num14 < 2; num14++)
					{
						if (self.grasps[num14] == null)
						{
							if (self.pickUpCandidate is Creature)
							{
								self.room.PlaySound(SoundID.Slugcat_Pick_Up_Creature, self.pickUpCandidate.firstChunk, false, 1f, 1f);
							}
							else if (self.pickUpCandidate is PlayerCarryableItem)
							{
								for (int num15 = 0; num15 < self.pickUpCandidate.grabbedBy.Count; num15++)
								{
									self.pickUpCandidate.grabbedBy[num15].grabber.GrabbedObjectSnatched(self.pickUpCandidate.grabbedBy[num15].grabbed, self);
									self.pickUpCandidate.grabbedBy[num15].grabber.ReleaseGrasp(self.pickUpCandidate.grabbedBy[num15].graspUsed);
								}
								(self.pickUpCandidate as PlayerCarryableItem).PickedUp(self);
							}
							else
							{
								self.room.PlaySound(SoundID.Slugcat_Pick_Up_Misc_Inanimate, self.pickUpCandidate.firstChunk, false, 1f, 1f);
							}
							self.SlugcatGrab(self.pickUpCandidate, num14);
							if (self.pickUpCandidate.graphicsModule != null && self.Grabability(self.pickUpCandidate) < (Player.ObjectGrabability)5)
							{
								self.pickUpCandidate.graphicsModule.BringSpritesToFront();
							}
							break;
						}
					}
				}
				self.wantToPickUp = 0;
			}
		}
	}
}