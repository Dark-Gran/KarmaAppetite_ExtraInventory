using RWCustom;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class patch_SlugcatHand
{
    public static void Patch()
    {
        On.SlugcatHand.Update += SlugcatHand_Update;
    }

    private static void SlugcatHand_Update(On.SlugcatHand.orig_Update orig, SlugcatHand self)
    {
		//Super.Update()
		self.lastPos = self.pos;
		if (self.retract && self.mode != Limb.Mode.Retracted)
		{
			self.mode = Limb.Mode.HuntAbsolutePosition;
			self.absoluteHuntPos = self.connection.pos;
			if (Custom.DistLess(self.absoluteHuntPos, self.pos, self.huntSpeed))
			{
				self.mode = Limb.Mode.Retracted;
			}
		}
		if (self.mode == Limb.Mode.HuntRelativePosition)
		{
			self.absoluteHuntPos = self.connection.pos + Custom.RotateAroundOrigo(self.relativeHuntPos, Custom.AimFromOneVectorToAnother(self.connection.rotationChunk.pos, self.connection.pos));
		}
		switch (self.mode)
		{
			case Limb.Mode.HuntRelativePosition:
			case Limb.Mode.HuntAbsolutePosition:
				if (Custom.DistLess(self.absoluteHuntPos, self.pos, self.huntSpeed))
				{
					self.vel = self.absoluteHuntPos - self.pos;
					self.reachedSnapPosition = true;
				}
				else
				{
					self.vel = Vector2.Lerp(self.vel, Custom.DirVec(self.pos, self.absoluteHuntPos) * self.huntSpeed, self.quickness);
					self.reachedSnapPosition = false;
				}
				break;
			case Limb.Mode.Retracted:
				self.vel = self.connection.vel;
				self.pos = self.connection.pos;
				self.reachedSnapPosition = true;
				break;
			case Limb.Mode.Dangle:
				self.reachedSnapPosition = false;
				break;
		}
		self.quickness = self.defaultQuickness;
		self.huntSpeed = self.defaultHuntSpeed;
		if (self.mode != Limb.Mode.Retracted)
		{
			self.pos += self.vel;
			if (self.mode == Limb.Mode.HuntRelativePosition)
			{
				self.pos += self.connection.vel;
			}
			self.vel *= self.airFriction;
			if (self.pushOutOfTerrain)
			{
				self.PushOutOfTerrain(self.owner.owner.room, self.connection.pos);
			}
		}

		//Update()
		self.ConnectToPoint(self.connection.pos, 20f, false, 0f, self.connection.vel, 0f, 0f);
		bool flag;
		if (self.reachingForObject)
		{
			self.mode = Limb.Mode.HuntAbsolutePosition;
			flag = false;
			self.reachingForObject = false;
		}
		else
		{
			flag = self.EngageInMovement();
		}
		if (self.limbNumber == 0 && (self.owner.owner as Player).grasps[0] != null && (self.owner.owner as Player).HeavyCarry((self.owner.owner as Player).grasps[0].grabbed))
		{
			flag = true;
		}
		if (flag)
		{
			if ((self.owner.owner as Player).grasps[0] != null && (self.owner.owner as Player).HeavyCarry((self.owner.owner as Player).grasps[0].grabbed))
			{
				self.mode = Limb.Mode.HuntAbsolutePosition;
				BodyChunk grabbedChunk = (self.owner.owner as Player).grasps[0].grabbedChunk;
				self.absoluteHuntPos = grabbedChunk.pos + Custom.PerpendicularVector((self.connection.pos - grabbedChunk.pos).normalized) * grabbedChunk.rad * 0.8f * ((self.limbNumber != 0) ? 1f : -1f);
				self.huntSpeed = 20f;
				self.quickness = 1f;
				flag = false;
			}
			else if ((self.owner.owner as Player).grasps[self.limbNumber] != null)
			{
				self.mode = Limb.Mode.HuntRelativePosition;
				self.relativeHuntPos.x = -20f + 40f * (float)self.limbNumber;
				self.relativeHuntPos.y = -12f;
				if ((self.owner.owner as Player).eatCounter < 40)
				{
					int num = -1;
					int num2 = 0;
					while (num < 0 && num2 < 2)
					{
						if ((self.owner.owner as Player).grasps[num2] != null && (self.owner.owner as Player).grasps[num2].grabbed is IPlayerEdible && ((self.owner.owner as Player).grasps[num2].grabbed as IPlayerEdible).Edible)
						{
							num = num2;
						}
						num2++;
					}
					if (num == self.limbNumber)
					{
						self.relativeHuntPos *= Custom.LerpMap((float)(self.owner.owner as Player).eatCounter, 40f, 20f, 0.9f, 0.7f);
						self.relativeHuntPos.y = self.relativeHuntPos.y + Custom.LerpMap((float)(self.owner.owner as Player).eatCounter, 40f, 20f, 2f, 4f);
						self.relativeHuntPos.x = self.relativeHuntPos.x * Custom.LerpMap((float)(self.owner.owner as Player).eatCounter, 40f, 20f, 1f, 1.2f);
					}
				}
				//Hand Towards Mouth
				if ((self.owner.owner as Player).swallowAndRegurgitateCounter > 10)
				{
					int num3 = -1;
					int num4 = 0;
					while (num3 < 0 && num4 < 2)
					{
						if ((self.owner.owner as Player).grasps[num4] != null && (self.owner.owner as Player).CanBeSwallowed((self.owner.owner as Player).grasps[num4].grabbed))
						{
							num3 = num4;
						}
						num4++;
					}
					if (num3 == self.limbNumber)
					{
						float num5 = Mathf.InverseLerp(10f, 90f, (float)(self.owner.owner as Player).swallowAndRegurgitateCounter);
						if (num5 < 0.5f)
						{
							self.relativeHuntPos *= Mathf.Lerp(0.9f, 0.7f, num5 * 2f);
							self.relativeHuntPos.y = self.relativeHuntPos.y + Mathf.Lerp(2f, 4f, num5 * 2f);
							self.relativeHuntPos.x = self.relativeHuntPos.x * Mathf.Lerp(1f, 1.2f, num5 * 2f);
						}
						else
						{
							if ((self.owner.owner as Player).grasps[0] != null && ((self.owner.owner as Player).objectInStomach == null || KarmaAppetite_ExtraInventory.HasSpace(self.owner.owner as Player)))
							{
								self.relativeHuntPos = new Vector2(0f, -4f) + Custom.RNV() * 2f * Random.value * Mathf.InverseLerp(0.5f, 1f, num5);
							}
							(self.owner as PlayerGraphics).blink = 5;
							(self.owner as PlayerGraphics).head.vel += Custom.RNV() * 2f * Random.value * Mathf.InverseLerp(0.5f, 1f, num5);
							self.owner.owner.bodyChunks[0].vel += Custom.RNV() * 0.2f * Random.value * Mathf.InverseLerp(0.5f, 1f, num5);
						}
					}
				}
				self.relativeHuntPos.x = self.relativeHuntPos.x * (1f - Mathf.Sin((self.owner.owner as Player).switchHandsProcess * 3.1415927f));
				if ((self.owner as PlayerGraphics).spearDir != 0f && (self.owner.owner as Player).bodyMode == Player.BodyModeIndex.Stand)
				{
					Vector2 to = Custom.DegToVec(180f + ((self.limbNumber != 0) ? 1f : -1f) * 8f + (float)(self.owner.owner as Player).input[0].x * 4f) * 12f;
					to.y += Mathf.Sin((float)(self.owner.owner as Player).animationFrame / 6f * 2f * 3.1415927f) * 2f;
					to.x -= Mathf.Cos((float)((self.owner.owner as Player).animationFrame + ((!(self.owner.owner as Player).leftFoot) ? 6 : 0)) / 12f * 2f * 3.1415927f) * 4f * (float)(self.owner.owner as Player).input[0].x;
					to.x += (float)(self.owner.owner as Player).input[0].x * 2f;
					self.relativeHuntPos = Vector2.Lerp(self.relativeHuntPos, to, Mathf.Abs((self.owner as PlayerGraphics).spearDir));
					if ((self.owner.owner as Player).grasps[self.limbNumber].grabbed is Weapon)
					{
						((self.owner.owner as Player).grasps[self.limbNumber].grabbed as Weapon).ChangeOverlap(((self.owner as PlayerGraphics).spearDir > -0.4f && self.limbNumber == 0) || ((self.owner as PlayerGraphics).spearDir < 0.4f && self.limbNumber == 1));
					}
				}
				flag = false;
				if ((self.owner.owner as Creature).grasps[self.limbNumber].grabbed is Fly && !((self.owner.owner as Creature).grasps[self.limbNumber].grabbed as Fly).dead)
				{
					self.huntSpeed = Random.value * 5f;
					self.quickness = Random.value * 0.3f;
					self.vel += Custom.DegToVec(Random.value * 360f) * Random.value * Random.value * ((!Custom.DistLess(self.absoluteHuntPos, self.pos, 7f)) ? 1.5f : 4f);
					self.pos += Custom.DegToVec(Random.value * 360f) * Random.value * 4f;
					(self.owner as PlayerGraphics).NudgeDrawPosition(0, Custom.DirVec((self.owner.owner as Creature).mainBodyChunk.pos, self.pos) * 3f * Random.value);
					(self.owner as PlayerGraphics).head.vel += Custom.DirVec((self.owner.owner as Creature).mainBodyChunk.pos, self.pos) * 2f * Random.value;
				}
				else if ((self.owner.owner as Creature).grasps[self.limbNumber].grabbed is VultureMask)
				{
					self.relativeHuntPos *= 1f - ((self.owner.owner as Creature).grasps[self.limbNumber].grabbed as VultureMask).donned;
				}
			}
		}
		if (flag && self.mode != Limb.Mode.Retracted)
		{
			self.retractCounter++;
			if ((float)self.retractCounter > 5f)
			{
				self.mode = Limb.Mode.HuntAbsolutePosition;
				self.pos = Vector2.Lerp(self.pos, self.owner.owner.bodyChunks[0].pos, Mathf.Clamp(((float)self.retractCounter - 5f) * 0.05f, 0f, 1f));
				if (Custom.DistLess(self.pos, self.owner.owner.bodyChunks[0].pos, 2f) && self.reachedSnapPosition)
				{
					self.mode = Limb.Mode.Retracted;
				}
				self.absoluteHuntPos = self.owner.owner.bodyChunks[0].pos;
				self.huntSpeed = 1f + (float)self.retractCounter * 0.2f;
				self.quickness = 1f;
			}
		}
		else
		{
			self.retractCounter -= 10;
			if (self.retractCounter < 0)
			{
				self.retractCounter = 0;
			}
		}
	}
}
