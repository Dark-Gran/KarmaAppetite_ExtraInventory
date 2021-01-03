using RWCustom;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class patch_PlayerGraphics
{
    public static void Patch()
    {
        On.PlayerGraphics.Update += PlayerGraphics_Update;
    }

    private static void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
    {
		//Super.Update()
		self.lastCulled = self.culled;
		self.culled = self.ShouldBeCulled;
		if (!self.culled && self.lastCulled)
		{
			self.Reset();
		}

		//Update()
		self.lastMarkAlpha = self.markAlpha;
		if (!self.player.dead && self.player.room.game.session is StoryGameSession && (self.player.room.game.session as StoryGameSession).saveState.deathPersistentSaveData.theMark)
		{
			self.markAlpha = Custom.LerpAndTick(self.markAlpha, Mathf.Clamp(Mathf.InverseLerp(30f, 80f, (float)self.player.touchedNoInputCounter) - Random.value * Mathf.InverseLerp(80f, 30f, (float)self.player.touchedNoInputCounter), 0f, 1f) * self.markBaseAlpha, 0.1f, 0.033333335f);
		}
		else
		{
			self.markAlpha = 0f;
		}
		if (self.player.input[1].x != self.player.input[0].x || self.player.input[1].y != self.player.input[0].y)
		{
			self.flail = Mathf.Min(1f, self.flail + 0.33333334f);
		}
		else
		{
			self.flail = Mathf.Max(0f, self.flail - 0.0125f);
		}
		self.lastBreath = self.breath;
		if (!self.player.dead)
		{
			if (self.player.Sleeping)
			{
				self.breath += 0.0125f;
			}
			else
			{
				self.breath += 1f / Mathf.Lerp(60f, 15f, Mathf.Pow(self.player.aerobicLevel, 1.5f));
			}
		}
		if (self.lightSource != null)
		{
			self.lightSource.stayAlive = true;
			self.lightSource.setPos = new Vector2?(self.player.mainBodyChunk.pos);
			if (self.lightSource.slatedForDeletetion || self.player.room.Darkness(self.player.mainBodyChunk.pos) == 0f)
			{
				self.lightSource = null;
			}
		}
		else if (self.player.room.Darkness(self.player.mainBodyChunk.pos) > 0f && self.player.glowing)
		{
			self.lightSource = new LightSource(self.player.mainBodyChunk.pos, false, Color.Lerp(new Color(1f, 1f, 1f), PlayerGraphics.SlugcatColor(self.player.playerState.slugcatCharacter), 0.5f), self.player);
			self.lightSource.requireUpKeep = true;
			self.lightSource.setRad = new float?(300f);
			self.lightSource.setAlpha = new float?(1f);
			self.player.room.AddObject(self.lightSource);
		}
		if (self.malnourished > 0f && !self.player.Malnourished)
		{
			self.malnourished = Mathf.Max(0f, self.malnourished - 0.005f);
		}
		if (self.player.bodyMode == Player.BodyModeIndex.Stand && self.player.input[0].x != 0)
		{
			self.spearDir = Mathf.Clamp(self.spearDir + (float)self.player.input[0].x * 0.1f, -1f, 1f);
		}
		else if (self.spearDir < 0f)
		{
			self.spearDir = Mathf.Min(self.spearDir + 0.05f, 0f);
		}
		else if (self.spearDir > 0f)
		{
			self.spearDir = Mathf.Max(self.spearDir - 0.05f, 0f);
		}
		if (self.player.room.world.rainCycle.RainApproaching < 1f && Random.value > self.player.room.world.rainCycle.RainApproaching && Random.value < 0.009803922f && (self.player.room.roomSettings.DangerType == RoomRain.DangerType.Rain || self.player.room.roomSettings.DangerType == RoomRain.DangerType.FloodAndRain))
		{
			self.objectLooker.LookAtPoint(new Vector2(self.player.room.PixelWidth * Random.value, self.player.room.PixelHeight + 100f), (1f - self.player.room.world.rainCycle.RainApproaching) * 0.6f);
		}
		float num = 0f;
		if (self.player.Consious && self.objectLooker.currentMostInteresting != null && self.objectLooker.currentMostInteresting is Creature)
		{
			CreatureTemplate.Relationship relationship = self.player.abstractCreature.creatureTemplate.CreatureRelationship((self.objectLooker.currentMostInteresting as Creature).abstractCreature.creatureTemplate);
			if (relationship.type == CreatureTemplate.Relationship.Type.Afraid && !(self.objectLooker.currentMostInteresting as Creature).dead)
			{
				float from = Mathf.Lerp(40f, 250f, relationship.intensity);
				num = Mathf.InverseLerp(from, 10f, Vector2.Distance(self.player.mainBodyChunk.pos, self.objectLooker.mostInterestingLookPoint) * ((!self.player.room.VisualContact(self.player.mainBodyChunk.pos, self.objectLooker.mostInterestingLookPoint)) ? 1.5f : 1f));
				if ((self.objectLooker.currentMostInteresting as Creature).abstractCreature.abstractAI != null && (self.objectLooker.currentMostInteresting as Creature).abstractCreature.abstractAI.RealAI != null)
				{
					num *= (self.objectLooker.currentMostInteresting as Creature).abstractCreature.abstractAI.RealAI.CurrentPlayerAggression(self.player.abstractCreature);
				}
			}
		}
		if (!self.player.Consious)
		{
			self.objectLooker.LookAtNothing();
			self.blink = 10;
		}
		if (self.DEBUGLABELS != null)
		{
			self.DEBUGLABELS[0].label.text = self.player.bodyMode.ToString() + " " + self.player.animation.ToString();
			self.DEBUGLABELS[1].label.text = string.Concat(new object[]
			{
			"XPOS: ",
			self.player.mainBodyChunk.pos.x,
			" YPOS: ",
			self.player.mainBodyChunk.pos.y
			});
			self.DEBUGLABELS[2].label.text = string.Concat(new object[]
			{
			"XPOS: ",
			self.player.bodyChunks[1].pos.x,
			" YPOS: ",
			self.player.bodyChunks[1].pos.y
			});
		}
		for (int i = 0; i < self.owner.bodyChunks.Length; i++)
		{
			self.drawPositions[i, 1] = self.drawPositions[i, 0];
		}
		self.drawPositions[0, 0] = self.owner.bodyChunks[0].pos;
		self.drawPositions[1, 0] = self.owner.bodyChunks[1].pos;
		int num2 = 0;
		bool flag = false;
		float num3 = 1f;
		switch (self.player.bodyMode)
		{
			case Player.BodyModeIndex.Default:
				if (self.player.animation == Player.AnimationIndex.AntlerClimb)
				{
					num2 = 2;
				}
				else if (self.player.animation == Player.AnimationIndex.LedgeGrab)
				{
					self.legsDirection.y = self.legsDirection.y - 1f;
					self.drawPositions[0, 0].x -= (float)self.player.flipDirection * 5f;
				}
				else
				{
					num3 = 0f;
				}
				break;
			case Player.BodyModeIndex.Crawl:
				{
					num2 = 1;
					float num4 = Mathf.Sin((float)self.player.animationFrame / 21f * 2f * 3.1415927f);
					float num5 = Mathf.Cos((float)self.player.animationFrame / 14f * 2f * 3.1415927f);
					float num6 = (self.player.superLaunchJump <= 19) ? 1f : 0f;
					self.drawPositions[0, 0].x += num5 * (float)self.player.flipDirection * 2f;
					self.drawPositions[0, 0].y -= num4 * -1.5f - 3f;
					GenericBodyPart genericBodyPart = self.head;
					genericBodyPart.vel.y = genericBodyPart.vel.y - (num4 * -0.5f - 0.5f);
					GenericBodyPart genericBodyPart2 = self.head;
					genericBodyPart2.vel.x = genericBodyPart2.vel.x + ((self.owner.bodyChunks[0].pos.x >= self.owner.bodyChunks[1].pos.x) ? 1f : -1f);
					self.drawPositions[1, 0].x += -3f * num4 * (float)self.player.flipDirection;
					self.drawPositions[1, 0].y -= num5 * 1.5f - 7f + 3f * num6;
					break;
				}
			case Player.BodyModeIndex.Stand:
				self.drawPositions[0, 0].x += (float)self.player.flipDirection * 6f * Mathf.Clamp(Mathf.Abs(self.owner.bodyChunks[1].vel.x) - 0.2f, 0f, 1f);
				self.drawPositions[0, 0].y += Mathf.Cos((float)self.player.animationFrame / 6f * 2f * 3.1415927f) * 2f;
				self.drawPositions[1, 0].x -= (float)self.player.flipDirection * (1.5f - (float)self.player.animationFrame / 6f);
				self.drawPositions[1, 0].y += 2f + Mathf.Sin((float)self.player.animationFrame / 6f * 2f * 3.1415927f) * 4f;
				flag = (Mathf.Abs(self.owner.bodyChunks[0].vel.x) > 2f && Mathf.Abs(self.owner.bodyChunks[1].vel.x) > 2f);
				num3 = 1f - Mathf.Clamp((Mathf.Abs(self.owner.bodyChunks[1].vel.x) - 1f) * 0.5f, 0f, 1f);
				break;
			case Player.BodyModeIndex.WallClimb:
				{
					num2 = 1;
					self.legsDirection.y = self.legsDirection.y - 1f;
					self.drawPositions[0, 0].y += 2f;
					self.drawPositions[0, 0].x -= (float)self.player.flipDirection * ((self.owner.bodyChunks[1].ContactPoint.y >= 0) ? 5f : 3f);
					GenericBodyPart genericBodyPart3 = self.head;
					genericBodyPart3.vel.y = genericBodyPart3.vel.y - (float)self.player.flipDirection * 5f;
					break;
				}
			case Player.BodyModeIndex.ClimbingOnBeam:
				num2 = 2;
				switch (self.player.animation)
				{
					case Player.AnimationIndex.GetUpOnBeam:
						self.disbalanceAmount = 70f;
						break;
					case Player.AnimationIndex.StandOnBeam:
						num2 = 0;
						self.drawPositions[1, 0].y += 3f;
						flag = (Mathf.Abs(self.owner.bodyChunks[0].vel.x) > 2f && Mathf.Abs(self.owner.bodyChunks[1].vel.x) > 2f);
						num3 = 1f - Mathf.Clamp((Mathf.Abs(self.owner.bodyChunks[1].vel.x) - 1f) * 0.3f, 0f, 1f);
						if (flag)
						{
							TailSegment tailSegment = self.tail[0];
							tailSegment.vel.x = tailSegment.vel.x - self.owner.bodyChunks[0].vel.x * 2f;
							TailSegment tailSegment2 = self.tail[0];
							tailSegment2.vel.y = tailSegment2.vel.y + 1.5f;
							TailSegment tailSegment3 = self.tail[1];
							tailSegment3.vel.x = tailSegment3.vel.x - self.owner.bodyChunks[0].vel.x * 0.2f;
							TailSegment tailSegment4 = self.tail[1];
							tailSegment4.vel.y = tailSegment4.vel.y + 0.5f;
						}
						break;
					case Player.AnimationIndex.ClimbOnBeam:
						self.drawPositions[0, 0].x += (float)self.player.flipDirection * 2.5f + (float)self.player.flipDirection * 0.5f * Mathf.Sin((float)self.player.animationFrame / 20f * 3.1415927f * 2f);
						self.drawPositions[1, 0].x += (float)self.player.flipDirection * 2.5f * Mathf.Cos((float)self.player.animationFrame / 20f * 3.1415927f * 2f);
						break;
					case Player.AnimationIndex.GetUpToBeamTip:
						self.disbalanceAmount = 120f;
						break;
				}
				break;
			case Player.BodyModeIndex.Swimming:
				if (self.player.animation == Player.AnimationIndex.DeepSwim || self.player.input[0].x != 0)
				{
					self.drawPositions[1, 0] += Custom.PerpendicularVector(Custom.DirVec(self.player.bodyChunks[0].pos, self.player.bodyChunks[1].pos)) * Mathf.Sin(self.player.swimCycle * 2f * 3.1415927f) * 5f;
				}
				break;
			case Player.BodyModeIndex.ZeroG:
				self.disbalanceAmount = Mathf.Max(self.disbalanceAmount, 70f * Mathf.InverseLerp(0.8f, 1f, self.flail));
				break;
		}
		switch (self.player.animation)
		{
			case Player.AnimationIndex.CorridorTurn:
				self.drawPositions[0, 0] += Custom.DegToVec(Random.value * 360f) * 3f * Random.value;
				self.drawPositions[1, 0] += Custom.DegToVec(Random.value * 360f) * 2f * Random.value;
				self.blink = 5;
				break;
			case Player.AnimationIndex.Roll:
			case Player.AnimationIndex.Flip:
				{
					float num7 = 6f;
					Vector2 a = Custom.DirVec(self.player.bodyChunks[0].pos, self.player.bodyChunks[1].pos);
					for (int j = 0; j < self.tail.Length; j++)
					{
						self.tail[j].vel += a * num7;
						num7 /= 1.7f;
					}
					break;
				}
		}
		if (self.player.bodyMode == Player.BodyModeIndex.Default && self.player.animation == Player.AnimationIndex.None && self.owner.bodyChunks[0].ContactPoint.x == 0 && self.owner.bodyChunks[0].ContactPoint.y == 0 && self.owner.bodyChunks[1].ContactPoint.x == 0 && self.owner.bodyChunks[1].ContactPoint.y == 0)
		{
			self.airborneCounter += self.owner.bodyChunks[0].vel.magnitude;
		}
		else
		{
			self.airborneCounter = 0f;
		}
		if (self.player.bodyMode == Player.BodyModeIndex.ClimbingOnBeam && (self.player.animation == Player.AnimationIndex.BeamTip || self.player.animation == Player.AnimationIndex.StandOnBeam))
		{
			if (Mathf.Abs(self.owner.bodyChunks[0].vel.x) > 2f)
			{
				self.disbalanceAmount += ((self.player.animation != Player.AnimationIndex.BeamTip) ? 3f : 17f);
			}
			else
			{
				self.disbalanceAmount -= 1f;
			}
			self.disbalanceAmount = Mathf.Clamp(self.disbalanceAmount, 0f, 120f);
			self.balanceCounter += 1f + self.disbalanceAmount / 40f * (1f + Random.value);
			if (self.balanceCounter > 300f)
			{
				self.balanceCounter -= 300f;
			}
			float num8 = Mathf.Sin(self.balanceCounter / 300f * 3.1415927f * 2f) / (Mathf.Abs(self.owner.bodyChunks[1].vel.x) + 1f);
			self.drawPositions[0, 0].x += num8 * (self.disbalanceAmount + 20f) * 0.08f;
			self.drawPositions[0, 0].y += num8 * self.disbalanceAmount * 0.02f;
			TailSegment tailSegment5 = self.tail[0];
			tailSegment5.vel.x = tailSegment5.vel.x + num8 * (self.disbalanceAmount + 20f) * 0.1f;
			TailSegment tailSegment6 = self.tail[1];
			tailSegment6.vel.x = tailSegment6.vel.x + num8 * (self.disbalanceAmount + 20f) * 0.04f;
		}
		if (self.player.bodyMode == Player.BodyModeIndex.ZeroG)
		{
			self.disbalanceAmount -= 1f;
			self.disbalanceAmount = Mathf.Clamp(self.disbalanceAmount, 0f, 120f);
			self.balanceCounter += 1f + self.disbalanceAmount / 40f * (1f + Random.value);
			if (self.balanceCounter > 300f)
			{
				self.balanceCounter -= 300f;
			}
			float d = Mathf.Sin(self.balanceCounter / 300f * 3.1415927f * 2f);
			Vector2 vector = Custom.DirVec(self.player.bodyChunks[1].pos, self.player.mainBodyChunk.pos);
			Vector2 a2 = Custom.PerpendicularVector(vector);
			self.drawPositions[0, 0] += a2 * d * (self.disbalanceAmount + 20f) * 0.08f;
			self.tail[0].vel -= a2 * d * (self.disbalanceAmount + 20f) * 0.1f + vector * self.disbalanceAmount * 0.1f;
			self.tail[1].vel -= a2 * d * (self.disbalanceAmount + 20f) * 0.04f + vector * self.disbalanceAmount * 0.04f;
		}
		if (self.player.Consious && self.player.standing && num > 0.5f)
		{
			self.drawPositions[0, 0] += Custom.DirVec(self.objectLooker.mostInterestingLookPoint, self.player.bodyChunks[0].pos) * 3.4f * Mathf.InverseLerp(0.5f, 1f, num);
			self.head.vel += Custom.DirVec(self.objectLooker.mostInterestingLookPoint, self.head.pos) * 1.4f * Mathf.InverseLerp(0.5f, 1f, num);
		}
		if (num > 0f)
		{
			self.tail[0].vel += Custom.DirVec(self.objectLooker.mostInterestingLookPoint, self.drawPositions[1, 0]) * 5f * num;
			self.tail[1].vel += Custom.DirVec(self.objectLooker.mostInterestingLookPoint, self.drawPositions[1, 0]) * 3f * num;
			self.player.aerobicLevel = Mathf.Max(self.player.aerobicLevel, Mathf.InverseLerp(0.5f, 1f, num) * 0.9f);
		}
		Vector2 vector2 = self.owner.bodyChunks[0].pos;
		if (flag)
		{
			vector2 = self.owner.bodyChunks[1].pos;
			vector2.y -= 4f;
			vector2.x += (float)self.player.flipDirection * 16f * Mathf.Clamp(Mathf.Abs(self.owner.bodyChunks[1].vel.x) - 0.2f, 0f, 1f);
		}
		Vector2 pos = self.owner.bodyChunks[1].pos;
		float num9 = 28f;
		self.tail[0].connectedPoint = new Vector2?(self.drawPositions[1, 0]);
		for (int k = 0; k < self.tail.Length; k++)
		{
			self.tail[k].Update();
			self.tail[k].vel *= Mathf.Lerp(0.75f, 0.95f, num3 * (1f - self.owner.bodyChunks[1].submersion));
			TailSegment tailSegment7 = self.tail[k];
			tailSegment7.vel.y = tailSegment7.vel.y - Mathf.Lerp(0.1f, 0.5f, num3) * (1f - self.owner.bodyChunks[1].submersion) * self.owner.room.gravity;
			num3 = (num3 * 10f + 1f) / 11f;
			if (!Custom.DistLess(self.tail[k].pos, self.owner.bodyChunks[1].pos, 9f * (float)(k + 1)))
			{
				self.tail[k].pos = self.owner.bodyChunks[1].pos + Custom.DirVec(self.owner.bodyChunks[1].pos, self.tail[k].pos) * 9f * (float)(k + 1);
			}
			self.tail[k].vel += Custom.DirVec(vector2, self.tail[k].pos) * num9 / Vector2.Distance(vector2, self.tail[k].pos);
			num9 *= 0.5f;
			vector2 = pos;
			pos = self.tail[k].pos;
		}
		if (self.player.swallowAndRegurgitateCounter > 15 && self.player.swallowAndRegurgitateCounter % 10 == 0)
		{
			self.blink = Math.Max(self.blink, Random.Range(-5, 8));
		}
		if (self.swallowing > 0)
		{
			self.swallowing--;
			self.blink = 5;
			self.drawPositions[0, 0] = Vector2.Lerp(self.drawPositions[0, 0], self.drawPositions[1, 0], 0.4f * Mathf.Sin((float)self.swallowing / 12f * 3.1415927f));
		}
		else if ((self.player.objectInStomach != null || KarmaAppetite_ExtraInventory.HasSomethingInInventory(self.player)) && self.player.swallowAndRegurgitateCounter > 0) //Head movement on spit
		{
			if (self.player.swallowAndRegurgitateCounter > 30)
			{
				self.blink = 5;
			}
			float num10 = Mathf.InverseLerp(0f, 110f, (float)self.player.swallowAndRegurgitateCounter);
			float num11 = (float)self.player.swallowAndRegurgitateCounter / Mathf.Lerp(30f, 15f, num10);
			if (self.player.standing)
			{
				self.drawPositions[0, 0].y += Mathf.Sin(num11 * 3.1415927f * 2f) * num10 * 2f;
				self.drawPositions[1, 0].y += -Mathf.Sin((num11 + 0.2f) * 3.1415927f * 2f) * num10 * 3f;
			}
			else
			{
				self.drawPositions[0, 0].y += Mathf.Sin(num11 * 3.1415927f * 2f) * num10 * 3f;
				self.drawPositions[0, 0].x += Mathf.Cos(num11 * 3.1415927f * 2f) * num10 * 1f;
				self.drawPositions[1, 0].y += Mathf.Sin((num11 + 0.2f) * 3.1415927f * 2f) * num10 * 2f;
				self.drawPositions[1, 0].x += -Mathf.Cos(num11 * 3.1415927f * 2f) * num10 * 3f;
			}
		}
		self.blink--;
		if (self.blink < -Random.Range(2, 1800))
		{
			self.blink = Random.Range(3, Random.Range(3, 10));
		}
		if (!self.player.dead)
		{
			if (self.player.exhausted)
			{
				if (self.player.aerobicLevel > 0.8f)
				{
					self.blink = Math.Max(self.blink, 1);
				}
				else if (Random.value < 0.02f)
				{
					self.blink = Math.Max(self.blink, Random.Range(10, 20));
				}
			}
			if (self.player.lungsExhausted || self.player.exhausted)
			{
				self.objectLooker.LookAtNothing();
				GenericBodyPart genericBodyPart4 = self.head;
				genericBodyPart4.vel.y = genericBodyPart4.vel.y + Mathf.Sin(self.player.swimCycle * 3.1415927f * 2f) * ((!self.player.lungsExhausted) ? 0.25f : 1f);
				self.drawPositions[0, 0].y += Mathf.Sin(self.player.swimCycle * 3.1415927f * 2f) * ((!self.player.lungsExhausted) ? 0.75f : 2.5f);
				self.blink = 1;
			}
		}
		if (Random.value < 0.1f)
		{
			self.objectLooker.Update();
		}
		if (Random.value < 0.0025f)
		{
			self.objectLooker.LookAtNothing();
		}
		self.lastLookDir = self.lookDirection;
		if (self.player.Consious && self.objectLooker.looking)
		{
			self.lookDirection = Custom.DirVec(self.head.pos, self.objectLooker.mostInterestingLookPoint);
		}
		else
		{
			self.lookDirection *= 0f;
		}
		if (num > 0.86f)
		{
			self.blink = 5;
			self.lookDirection *= -1f;
		}
		if (self.player.grasps[0] != null && self.player.grasps[0].grabbed is JokeRifle)
		{
			self.lookDirection = (self.player.grasps[0].grabbed as JokeRifle).aimDir;
		}
		if (self.player.standing)
		{
			if (self.player.input[0].x == 0)
			{
				self.head.vel -= self.lookDirection * 0.5f;
			}
			self.drawPositions[0, 0] -= self.lookDirection * 2f;
		}
		else
		{
			self.head.vel += self.lookDirection;
		}
		Vector2 b = Custom.DirVec(self.drawPositions[1, 0], self.drawPositions[0, 0]) * 3f;
		if (self.player.bodyMode == Player.BodyModeIndex.Crawl)
		{
			b.x *= 2.5f;
		}
		else if (self.player.bodyMode == Player.BodyModeIndex.CorridorClimb && b.y < 0f)
		{
			b.y *= 2f;
		}
		self.head.Update();
		self.head.ConnectToPoint(Vector2.Lerp(self.drawPositions[0, 0], self.drawPositions[1, 0], 0.2f) + b, (self.player.animation != Player.AnimationIndex.HangFromBeam) ? 3f : 0f, false, 0.2f, self.owner.bodyChunks[0].vel, 0.7f, 0.1f);
		self.legs.Update();
		if (self.player.bodyMode == Player.BodyModeIndex.CorridorClimb)
		{
			self.legs.ConnectToPoint(self.owner.bodyChunks[1].pos + Custom.DirVec(self.owner.bodyChunks[0].pos, self.owner.bodyChunks[1].pos) * 4f, 2f, false, 0.25f, self.owner.bodyChunks[1].vel, 0.5f, 0.1f);
			int num12 = Mathf.RoundToInt((270f - Custom.AimFromOneVectorToAnother(self.owner.bodyChunks[1].pos, self.owner.bodyChunks[0].pos)) / 45f);
			int num13 = 10;
			int num14 = 0;
			for (int l = 0; l < 4; l++)
			{
				if (self.owner.room.GetTile(self.owner.room.GetTilePosition(self.owner.bodyChunks[1].pos) + Custom.eightDirections[(l + num12 + 10) % 8]).Terrain == Room.Tile.TerrainType.Solid && self.owner.room.GetTile(self.owner.room.GetTilePosition(self.owner.bodyChunks[1].pos) + Custom.eightDirections[(l + num12 + 14) % 8]).Terrain == Room.Tile.TerrainType.Solid)
				{
					int num15 = 0;
					if (l == 1)
					{
						num15 = ((self.player.flipDirection != -1) ? 2 : 1);
					}
					else if (l == 3)
					{
						num15 = ((self.player.flipDirection != 1) ? 2 : 1);
					}
					else if (l == 2)
					{
						num15 = 3;
					}
					if (num15 < num13)
					{
						num13 = num15;
						switch (l)
						{
							case 0:
								num14 = 0;
								break;
							case 1:
								num14 = 45;
								break;
							case 2:
								num14 = ((self.player.flipDirection != -1) ? 90 : -90);
								break;
							case 3:
								num14 = -45;
								break;
						}
					}
				}
			}
			self.legsDirection += Custom.DegToVec(Custom.AimFromOneVectorToAnother(self.owner.bodyChunks[0].pos, self.owner.bodyChunks[1].pos) + (float)num14);
		}
		else if (self.owner.bodyChunks[1].ContactPoint.y == -1 || self.player.animation == Player.AnimationIndex.StandOnBeam)
		{
			self.legs.ConnectToPoint(self.owner.bodyChunks[1].pos + new Vector2(self.legsDirection.x * 8f, 1f), 5f, false, 0.25f, new Vector2(self.owner.bodyChunks[1].vel.x, -10f), 0.5f, 0.1f);
			self.legsDirection.x = self.legsDirection.x - (float)self.owner.bodyChunks[1].onSlope;
			self.legsDirection.y = self.legsDirection.y - 1f;
		}
		else if (self.player.animation == Player.AnimationIndex.BeamTip)
		{
			self.legs.ConnectToPoint(self.owner.bodyChunks[1].pos + new Vector2(0f, -8f), 0f, false, 0.25f, new Vector2(0f, -10f), 0.5f, 0.1f);
			self.legsDirection += Custom.DirVec(self.drawPositions[0, 0], self.owner.room.MiddleOfTile(self.owner.bodyChunks[1].pos) + new Vector2(0f, -10f));
		}
		else if (self.player.animation == Player.AnimationIndex.ClimbOnBeam)
		{
			Vector2 b2 = new Vector2((float)(-(float)self.player.flipDirection) * (5f - Mathf.Sin((float)self.player.animationFrame / 20f * 3.1415927f * 2f)), -16f - 5f * Mathf.Cos((float)self.player.animationFrame / 20f * 3.1415927f * 2f));
			self.legs.ConnectToPoint(self.owner.bodyChunks[0].pos + b2, 0f, false, 0.25f, new Vector2(0f, 0f), 0.5f, 0.1f);
			self.legsDirection.y = self.legsDirection.y - 1f;
		}
		else if (self.player.animation == Player.AnimationIndex.ZeroGSwim || self.player.animation == Player.AnimationIndex.ZeroGPoleGrab)
		{
			self.legs.ConnectToPoint(self.owner.bodyChunks[1].pos + Custom.DirVec(self.owner.bodyChunks[0].pos, self.owner.bodyChunks[1].pos) * 4f, 4f, false, 0f, self.owner.bodyChunks[1].vel, 0.2f, 0f);
			self.legsDirection = Custom.DirVec(self.owner.bodyChunks[0].pos, self.owner.bodyChunks[1].pos);
			self.legs.vel += self.legsDirection * 0.2f;
		}
		else
		{
			self.legs.ConnectToPoint(self.owner.bodyChunks[1].pos + new Vector2(self.legsDirection.x * 8f, (self.player.animation != Player.AnimationIndex.HangFromBeam) ? -2f : -5f), 4f, false, 0.25f, new Vector2(self.owner.bodyChunks[1].vel.x, -10f), 0.5f, 0.1f);
			self.legsDirection += self.owner.bodyChunks[1].vel * 0.01f;
			self.legsDirection.y = self.legsDirection.y - 0.05f;
		}
		self.legsDirection.Normalize();
		if (self.player.Consious)
		{
			if (self.throwCounter > 0 && self.thrownObject != null)
			{
				self.hands[self.handEngagedInThrowing].reachingForObject = true;
				self.hands[self.handEngagedInThrowing].absoluteHuntPos = self.thrownObject.firstChunk.pos;
				if (Custom.DistLess(self.hands[self.handEngagedInThrowing].pos, self.thrownObject.firstChunk.pos, 40f))
				{
					self.hands[self.handEngagedInThrowing].pos = self.thrownObject.firstChunk.pos;
				}
				else
				{
					self.hands[self.handEngagedInThrowing].vel += Custom.DirVec(self.hands[self.handEngagedInThrowing].pos, self.thrownObject.firstChunk.pos) * 6f;
				}
				self.hands[1 - self.handEngagedInThrowing].vel -= Custom.DirVec(self.hands[self.handEngagedInThrowing].pos, self.thrownObject.firstChunk.pos) * 3f;
				self.throwCounter--;
			}
			else if (self.player.handOnExternalFoodSource != null)
			{
				int num16 = (self.player.handOnExternalFoodSource.Value.x >= self.player.mainBodyChunk.pos.x) ? 1 : 0;
				self.hands[num16].reachingForObject = true;
				if (self.player.eatExternalFoodSourceCounter < 3)
				{
					self.hands[num16].absoluteHuntPos = self.head.pos;
					self.blink = Math.Max(self.blink, 3);
				}
				else
				{
					self.hands[num16].absoluteHuntPos = self.player.handOnExternalFoodSource.Value;
				}
				self.drawPositions[0, 0] += Custom.DirVec(self.drawPositions[0, 0], self.player.handOnExternalFoodSource.Value) * 5f;
				self.head.vel += Custom.DirVec(self.drawPositions[0, 0], self.player.handOnExternalFoodSource.Value) * 2f;
			}
			else if ((self.player.grasps[0] != null && self.player.grasps[0].grabbed is TubeWorm) || (self.player.grasps[1] != null && self.player.grasps[1].grabbed is TubeWorm))
			{
				for (int m = 0; m < self.player.grasps.Length; m++)
				{
					if (self.player.grasps[m] != null && self.player.grasps[m].grabbed is TubeWorm)
					{
						self.hands[m].mode = Limb.Mode.HuntRelativePosition;
						self.hands[m].relativeHuntPos = new Vector2(5f * ((m != 0) ? 1f : -1f), -10f);
					}
				}
			}
			else if (self.player.spearOnBack != null && self.player.spearOnBack.counter > 5)
			{
				int num17 = -1;
				int num18 = 0;
				while (num18 < 2 && num17 == -1)
				{
					if ((self.player.spearOnBack.HasASpear && self.player.grasps[num18] == null) || (!self.player.spearOnBack.HasASpear && self.player.grasps[num18] != null && self.player.grasps[num18].grabbed is Spear))
					{
						num17 = num18;
					}
					num18++;
				}
				if (num17 > -1)
				{
					if (self.player.grasps[num17] != null && self.player.grasps[num17].grabbed is Weapon)
					{
						(self.player.grasps[num17].grabbed as Weapon).ChangeOverlap(false);
					}
					self.hands[num17].reachingForObject = true;
					self.hands[num17].mode = Limb.Mode.HuntRelativePosition;
					if (self.player.spearOnBack.HasASpear)
					{
						self.hands[num17].relativeHuntPos = Vector3.Slerp(new Vector2(((num17 != 0) ? 1f : -1f) * 20f, -30f) * Mathf.Sin(Mathf.InverseLerp(9f, 20f, (float)self.player.spearOnBack.counter) * 3.1415927f), new Vector2(0f, 1f), Mathf.InverseLerp(9f, 20f, (float)self.player.spearOnBack.counter));
					}
					else
					{
						self.hands[num17].relativeHuntPos = Vector3.Slerp(new Vector2(((num17 != 0) ? 1f : -1f) * 30f, -20f) * Mathf.Lerp(1f, 0.2f, Mathf.Abs(self.player.spearOnBack.flip)), new Vector2(1f, 1f), Mathf.InverseLerp(14f, 20f, (float)self.player.spearOnBack.counter));
					}
					self.drawPositions[0, 0] += Custom.DirVec(self.hands[num17].absoluteHuntPos, self.drawPositions[0, 0]) * 0.7f;
					self.head.vel += Custom.DirVec(self.hands[num17].absoluteHuntPos, self.head.pos) * 1.5f;
				}
			}
			else if (self.player.FoodInStomach < self.player.MaxFoodInStomach && self.objectLooker.currentMostInteresting != null && num2 < 2 && ((self.objectLooker.currentMostInteresting is Fly && (self.objectLooker.currentMostInteresting as Fly).PlayerAutoGrabable) || num > 0.8f) && Custom.DistLess(self.player.mainBodyChunk.pos, self.objectLooker.mostInterestingLookPoint, 80f) && self.player.room.VisualContact(self.player.mainBodyChunk.pos, self.objectLooker.mostInterestingLookPoint))
			{
				int num19 = -1;
				for (int n = 0; n < 2; n++)
				{
					if (self.player.grasps[n] == null && self.hands[1 - n].reachedSnapPosition)
					{
						num19 = n;
					}
				}
				if (self.objectLooker.currentMostInteresting is Fly && (self.objectLooker.currentMostInteresting as Fly).PlayerAutoGrabable && self.player.input[0].x != 0 && self.objectLooker.currentMostInteresting.bodyChunks[0].pos.x < self.player.mainBodyChunk.pos.x == self.player.input[0].x > 0)
				{
					num19 = -1;
				}
				if (num19 > -1)
				{
					self.hands[num19].reachingForObject = true;
					self.hands[num19].absoluteHuntPos = self.objectLooker.mostInterestingLookPoint;
					if (num == 0f)
					{
						self.drawPositions[0, 0] += Custom.DirVec(self.drawPositions[0, 0], self.objectLooker.mostInterestingLookPoint) * 5f;
						self.head.vel += Custom.DirVec(self.drawPositions[0, 0], self.objectLooker.mostInterestingLookPoint) * 2f;
					}
				}
			}
		}
		for (int num20 = 0; num20 < 2; num20++)
		{
			self.hands[num20].Update();
		}
		if (self.player.sleepCurlUp > 0f)
		{
			float num21 = Mathf.Sign(self.player.bodyChunks[0].pos.x - self.player.bodyChunks[1].pos.x);
			Vector2 vector3 = (self.player.bodyChunks[0].pos + self.player.bodyChunks[1].pos) / 2f;
			self.drawPositions[0, 0] = Vector2.Lerp(self.drawPositions[0, 0], vector3, self.player.sleepCurlUp * 0.2f);
			self.drawPositions[1, 0] = Vector2.Lerp(self.drawPositions[1, 0], vector3, self.player.sleepCurlUp * 0.2f);
			self.drawPositions[0, 0].y += 2f * self.player.sleepCurlUp;
			self.drawPositions[1, 0].y += 2f * self.player.sleepCurlUp;
			self.drawPositions[1, 0].x -= 3f * num21 * self.player.sleepCurlUp;
			for (int num22 = 0; num22 < self.tail.Length; num22++)
			{
				float num23 = (float)num22 / (float)(self.tail.Length - 1);
				self.tail[num22].vel *= 1f - 0.2f * self.player.sleepCurlUp;
				self.tail[num22].pos = Vector2.Lerp(self.tail[num22].pos, self.drawPositions[1, 0] + new Vector2((Mathf.Sin(num23 * 3.1415927f) * 25f - num23 * 10f) * -num21, Mathf.Lerp(5f, -15f, num23)), 0.1f * self.player.sleepCurlUp);
			}
			self.head.vel *= 1f - 0.4f * self.player.sleepCurlUp;
			self.head.pos = Vector2.Lerp(self.head.pos, vector3 + new Vector2(num21 * 5f, -3f), 0.5f * self.player.sleepCurlUp);
			if (self.player.sleepCurlUp == 1f || Random.value < 0.033333335f)
			{
				self.blink = Math.Max(2, self.blink);
			}
			for (int num24 = 0; num24 < 2; num24++)
			{
				self.hands[num24].absoluteHuntPos = vector3 + new Vector2(num21 * 10f, -20f);
			}
		}
		if (self.player.Adrenaline > 0f)
		{
			float d2 = Mathf.Pow(self.player.Adrenaline, 0.2f);
			self.drawPositions[0, 0] += Custom.RNV() * Random.value * d2 * 2f;
			self.drawPositions[0, 1] += Custom.RNV() * Random.value * d2 * 2f;
			self.head.pos += Custom.RNV() * Random.value * d2 * 1f;
			if (Random.value < 0.05f)
			{
				self.blink = Math.Max(self.blink, 3);
			}
		}
	}
}
