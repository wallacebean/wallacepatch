using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using LLHandlers;
using GameplayEntities;
using LLGUI;
using Abilities;
using LLBML.Math;
using System.Reflection;
using System.Reflection.Emit;
using LLBML.Players;
using BepInEx.Logging;


namespace wallacepatch
{
	[BepInPlugin("us.wallace.plugins.llb.wallacepatch", "wallacepatch Plug-In", "1.0.0.8")]
	public class Plugin : BaseUnityPlugin

	{
		public static ManualLogSource Log { get; private set; } = null;
		private void Awake()
		{
			Log = this.Logger;

			Logger.LogDebug("Patching effects settings...");

			var harmony = new Harmony("us.wallace.plugins.llb.wallacepatch");

			harmony.PatchAll(typeof(HandleHitboxHitPatch));
			harmony.PatchAll(typeof(PLAYERSetEntityAbilitiesPatch));
			harmony.PatchAll(typeof(DNAHandleHitsPatch));
			harmony.PatchAll(typeof(BALLSetEntityVisualsPatch));
			harmony.PatchAll(typeof(SweepCollisionPatch));
			//harmony.PatchAll(typeof(GETUPSetAbilityStatePatch)); //lags game
			harmony.PatchAll(typeof(PLAYERSetEntityValuesPatch));
			harmony.PatchAll(typeof(SetFlySpeedOnHitPatch));
			harmony.PatchAll(typeof(SoundBlastPlayersInBoxPatch));
			harmony.PatchAll(typeof(CORPSESetEntityValuesPatch));
			//harmony.PatchAll(typeof(BagPlayerSetEntityBoxesPatch)); //crashes game
			harmony.PatchAll(typeof(BagSetFrontHitboxesAndParryBoxesPatch));
			harmony.PatchAll(typeof(BoomPlayerSetEntityBoxesPatch));
			harmony.PatchAll(typeof(BossPlayerSetEntityBoxesPatch));
			harmony.PatchAll(typeof(CandyPlayerSetEntityBoxesPatch));
			harmony.PatchAll(typeof(CopPlayerSetEntityBoxesPatch));
			harmony.PatchAll(typeof(CrocPlayerSetEntityBoxesPatch));
			harmony.PatchAll(typeof(ElectroPlayerSetEntityBoxesPatch));
			harmony.PatchAll(typeof(GrafSetNormalHitboxesPatch)); 
			harmony.PatchAll(typeof(GrafSetFrontHitboxesAndParryBoxesPatch));
			harmony.PatchAll(typeof(KidPlayerSetEntityBoxesPatch));
			harmony.PatchAll(typeof(PongPlayerSetEntityBoxesPatch));
			harmony.PatchAll(typeof(RobotPlayerSetEntityBoxesPatch));
			harmony.PatchAll(typeof(SkatePlayerSetEntityBoxesPatch));
			//harmony.PatchAll(typeof(BagCheckActivationPatch)); //lags game
			//harmony.PatchAll(typeof(PongCheckActivationPatch)); //lags game
			//harmony.PatchAll(typeof(BALLSetEntityValuesPatch)); //crashes game
			harmony.PatchAll(typeof(CONSTRUCTORDownAirSwingAbilityPatch));
			harmony.PatchAll(typeof(CONSTRUCTOREatAbilityPatch));
			harmony.PatchAll(typeof(CONSTRUCTORTauntAbilityPatch));
			harmony.PatchAll(typeof(MovableEntityMovementPatch));
			harmony.PatchAll(typeof(BAGMovementPatch));

		}
	}

	public static class PatchUtils
	{
		public static void LogInstructions(IEnumerable<CodeInstruction> instructions, int from = 0, int to = -1)
		{
			int i = 0;
			if (to == -1) to = instructions.Count();
			foreach (var inst in instructions)
			{
				if (i >= from && i <= to)
					Plugin.Log.LogDebug(i + ": " + inst);
				i++;
			}
		}

		public static void LogInstruction(CodeInstruction ci)
		{
			Plugin.Log.LogInfo("OpCode: " + ci.opcode + " | Operand: " + ci.operand);
		}
	}


	class HandleHitboxHitPatch
	{
		[HarmonyPatch(typeof(HittingEntity), nameof(HittingEntity.HandleHitboxHit))]
		[HarmonyPrefix]
		public static bool HandleHitboxHit_Prefix(HittingEntity __instance, string boxName, HitableEntity hitEntity)
		{
			PlayerHitbox playerHitbox = __instance.hitboxes[boxName];
			bool flag = hitEntity.IsBall();
			EntityType entityType = hitEntity.entityType;
			BallEntity ballEntity = (!flag) ? null : (hitEntity as BallEntity);
			if (flag && (__instance.MatchPowerupIs(LLHandlers.Powerup.SHOCK, LLHandlers.PowerupPhase.ANY) || ballEntity.ballData.ballState == BallState.BUBBLEBALL) && hitEntity.GetLastPlayerHitter() != __instance && __instance.character != global::Character.DUMMY)
			{
				ballEntity.StartHitstun(__instance.parryHitstunDuration, (ballEntity.ballData.ballState != BallState.BUBBLEBALL) ? HitstunState.WALL_STUN : HitstunState.BUBBLE_WALL_STUN);
				ballEntity.DeflectClashPlayer((PlayerEntity)__instance, boxName);
				return false;
			}
			if (!__instance.MatchPowerupIs(LLHandlers.Powerup.PHANTOM, LLHandlers.PowerupPhase.ANY) && flag)
			{
				global::IBGCBLLKIHA position = hitEntity.GetPosition();
				global::IBGCBLLKIHA ibgcbllkiha = global::IBGCBLLKIHA.FCKBPDNEAOG(playerHitbox.bounds.KHBAPGIEOIC, global::IBGCBLLKIHA.FCGOICMIBEA(playerHitbox.bounds.IACOKHPMNGN, global::HHBCPNCDNDH.NKKIFJJEPOL(20)));
				global::IBGCBLLKIHA ibgcbllkiha2 = global::IBGCBLLKIHA.GAFCIOAEGKD(playerHitbox.bounds.MOGDHBGHAOA, global::IBGCBLLKIHA.FCGOICMIBEA(playerHitbox.bounds.IACOKHPMNGN, global::HHBCPNCDNDH.NKKIFJJEPOL(20)));
				if (global::HHBCPNCDNDH.EAOICALCHJI(hitEntity.entityData.velocity.GCPKPHMKLBN, global::HHBCPNCDNDH.DBOMOJGKIFI))
				{
					position.GCPKPHMKLBN = global::HHBCPNCDNDH.ANJPNFDPHFP(position.GCPKPHMKLBN, ibgcbllkiha.GCPKPHMKLBN);
					position.GCPKPHMKLBN = global::HHBCPNCDNDH.BBGBJJELCFJ(position.GCPKPHMKLBN, ibgcbllkiha2.GCPKPHMKLBN);
				}
				if (global::HHBCPNCDNDH.EAOICALCHJI(hitEntity.entityData.velocity.CGJJEHPPOAN, global::HHBCPNCDNDH.DBOMOJGKIFI))
				{
					position.CGJJEHPPOAN = global::HHBCPNCDNDH.ANJPNFDPHFP(position.CGJJEHPPOAN, ibgcbllkiha.CGJJEHPPOAN);
					position.CGJJEHPPOAN = global::HHBCPNCDNDH.BBGBJJELCFJ(position.CGJJEHPPOAN, ibgcbllkiha2.CGJJEHPPOAN);
				}
				hitEntity.SetPosition(position);
				hitEntity.entityData.prePosition = __instance.GetPosition();
				hitEntity.ClampWithinStage();
			}
			hitEntity.ClampWithinVolleyballSide(__instance.attackingData.team);
			if (flag)
			{
				__instance.SetCombos(ballEntity, playerHitbox.bunts);
			}
			if (flag && global::JOMBNFKIHIC.GIGAKBJGFDI.PNJOKAICMNN == global::GameMode.VOLLEYBALL && ballEntity.ballData.volleyballCombos[__instance.playerIndex] > 3)
			{
				global::NCMFHODLNAJ.GIGAKBJGFDI.NDIODLNFCHC((__instance.attackingData.team != global::BGHNEHPFHGC.PDOIGCCJJEO) ? global::BGHNEHPFHGC.PDOIGCCJJEO : global::BGHNEHPFHGC.EHPJJADIPNG, __instance.playerIndex);
				__instance.PlaySfx(LLHandlers.Sfx.KILL);
				global::GameCamera.instance.StartShake(global::HHBCPNCDNDH.NKKIFJJEPOL(0.67m), global::HHBCPNCDNDH.NKKIFJJEPOL(0.4m));
				__instance.effectHandler.CreateHitBallBehindEffect(ballEntity.GetPosition());
				ballEntity.CreateDeadBallEffect();
				ballEntity.SetBallState(BallState.DEAD, HitstunState.NONE);
				return false;
			}
			__instance.AddTrackedHitEntityID(hitEntity.entityID);
			__instance.RallyEvent(hitEntity);
			hitEntity.HitByTeam(__instance.attackingData.team);
			int lastHitterIndex = hitEntity.hitableData.lastHitterIndex;
			hitEntity.hitableData.lastHitterIndex = __instance.playerIndex;
			if (flag && __instance.MatchPowerupIs(LLHandlers.Powerup.HEAL, LLHandlers.PowerupPhase.ANY))
			{
				__instance.abilityData.hitSomething = true;
				__instance.GiveBall(BallState.STICK_TO_PLAYER_POWERUP, ballEntity, false);
				__instance.playerHandler.GetPlayerEntity(lastHitterIndex).playerData.powerup = LLHandlers.Powerup.NONE;
				__instance.abilityData.powerup = LLHandlers.Powerup.HEAL;
			}
			else
			{
				if (!__instance.attackingData.hitSomething)
				{
					__instance.attackingData.preHitstunVelocity = __instance.attackingData.velocity;
				}
				if (playerHitbox.canCharge && __instance.ChargedEnoughToBeFullCharge())
				{
					__instance.SetAbilityState(playerHitbox.fullChargeHitAbilityState);
				}
				else
				{
					__instance.abilityStates.ContainsKey(playerHitbox.abilityHitState);
					__instance.SetAbilityState(playerHitbox.abilityHitState);
				}
				if (playerHitbox.bunts)
				{
					__instance.Hit(hitEntity, true, global::HHBCPNCDNDH.NKKIFJJEPOL(0.075m), HitstunState.BUNT_STUN, false, true);
				}
				else
				{
					__instance.Hit(hitEntity, entityType == EntityType.PLAYER, global::HHBCPNCDNDH.DBOMOJGKIFI, playerHitbox.stunState, false, true);
				}
			}
			__instance.UpdateAnimatableEntity();
			__instance.UpdateUnityTransform();
			if (flag && __instance.player.ALBOPCLADGN)
			{
				global::DCKIMOBPGBF ai = LLHandlers.AIHandler.instance.GetAI(__instance.playerIndex);
				if (ai != null)
				{
					ai.DDHBGKCDHFP(ballEntity);
				}
			}
			return false;
		}

	}


	class DNAHandleHitsPatch
	{
		[HarmonyPatch(typeof(ShadowAbility), nameof(ShadowAbility.HandleHits))]
		[HarmonyPrefix]
		public static bool HandleHits_Prefix(ShadowAbility __instance)
		{
			bool flag = __instance.entity.GetAnimDataOfVisual("shadowVisual").currentAnim.Contains("Bunt");
			global::IBGCBLLKIHA ibgcbllkiha = global::IBGCBLLKIHA.GAFCIOAEGKD(__instance.data.specialVector2, global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.GGFFJDILCDA, global::HHBCPNCDNDH.NKKIFJJEPOL(0.8m)));
			global::IBGCBLLKIHA BOOS = global::IBGCBLLKIHA.GAFCIOAEGKD(__instance.data.specialVector2, global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.GGFFJDILCDA, global::HHBCPNCDNDH.NKKIFJJEPOL(0.1m)));
			if (__instance.data.specialHeading == GameplayEntities.Side.RIGHT)
			{
				ibgcbllkiha = global::IBGCBLLKIHA.GAFCIOAEGKD(ibgcbllkiha, flag ? global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.KAOAKJMACGA, global::HHBCPNCDNDH.NKKIFJJEPOL(0.65m)) : global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.NANKJFEFMON, global::HHBCPNCDNDH.NKKIFJJEPOL(0.65m)));
			}
			else
			{
				ibgcbllkiha = global::IBGCBLLKIHA.GAFCIOAEGKD(ibgcbllkiha, flag ? global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.NANKJFEFMON, global::HHBCPNCDNDH.NKKIFJJEPOL(0.65m)) : global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.KAOAKJMACGA, global::HHBCPNCDNDH.NKKIFJJEPOL(0.65m)));
			}
			for (int i = 0; i < __instance.data.trackedHitEntityID.Count; i++)
			{
				if (__instance.data.trackedHitEntityID[i] != global::World.NO_ENTITY_ID)
				{
					GameplayEntities.HitableEntity hitableEntity = (GameplayEntities.HitableEntity)global::World.instance.GetEntity(__instance.data.trackedHitEntityID[i]);
					hitableEntity.ClampWithinStage();
					if (hitableEntity != null)
					{
						if (flag)
						{
							hitableEntity.SetPosition(BOOS);
							__instance.entity.Hit(hitableEntity, true, global::HHBCPNCDNDH.NKKIFJJEPOL(0.075m), GameplayEntities.HitstunState.SHADOWBALL_BUNT_STUN, true, false);
						}
						else
						{
							if (hitableEntity.IsBall())
							{
								hitableEntity.SetPosition(ibgcbllkiha);
								((GameplayEntities.BallEntity)hitableEntity).ballData.fromSpecial = true;
							}
							__instance.entity.Hit(hitableEntity, true, __instance.shadowHitstunDuration, GameplayEntities.HitstunState.SHADOWBALL_STUN, true, false);
							if (hitableEntity.IsBall())
							{
								hitableEntity.SetPosition(ibgcbllkiha);
								((GameplayEntities.BallEntity)hitableEntity).ballData.fromSpecial = true;
							}
						}
						if (hitableEntity.entityType == GameplayEntities.EntityType.CORPSE)
						{
							hitableEntity.SetPosition(ibgcbllkiha);
							hitableEntity.PlayAnim("knockback", "main");
						}
					}
				}
			}
			__instance.data.specialFAmount = global::HHBCPNCDNDH.NKKIFJJEPOL(0);
			__instance.data.specialAmount = 4;
			return false;
		}

	}
	class SweepCollisionPatch
	{
		[HarmonyPatch(typeof(CollidableEntity), nameof(CollidableEntity.SweepCollision))]
		[HarmonyPrefix]
		public static bool SweepCollision_Prefix(CollidableEntity __instance)
		{
			global::IBGCBLLKIHA velocity = __instance.entityData.velocity;
			global::HHBCPNCDNDH hhbcpncdndh = velocity.KEMFCABCHLO;
			global::IBGCBLLKIHA acihfibjnkm = (!global::HHBCPNCDNDH.ODMJDNBPOIH(hhbcpncdndh, global::HHBCPNCDNDH.DBOMOJGKIFI)) ? global::IBGCBLLKIHA.FCGOICMIBEA(velocity, hhbcpncdndh) : global::IBGCBLLKIHA.DBOMOJGKIFI;
			global::IBGCBLLKIHA position = __instance.GetPosition();
			global::HHBCPNCDNDH smallestCollisionSize;
			if (__instance.GetType() == typeof(global::GameplayEntities.BallEntity))
			{
				smallestCollisionSize = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(1m), global::World.FPIXEL_SIZE);
			}
			else
			{
				smallestCollisionSize = __instance.GetSmallestCollisionSize();
			}
			int num = global::HHBCPNCDNDH.HGDAIHMEFKC(global::HHBCPNCDNDH.FCGOICMIBEA(global::HHBCPNCDNDH.AJOCFFLIIIH(hhbcpncdndh, global::World.FDELTA_TIME), smallestCollisionSize));
			global::IBGCBLLKIHA jobhdehllbl = global::IBGCBLLKIHA.AJOCFFLIIIH(acihfibjnkm, smallestCollisionSize);
			for (int i = 0; i <= num; i++)
			{
				if (i == num)
				{
					__instance.SetPosition(global::IBGCBLLKIHA.GAFCIOAEGKD(position, global::IBGCBLLKIHA.AJOCFFLIIIH(__instance.entityData.velocity, global::World.FDELTA_TIME)));
				}
				else
				{
					__instance.SetPosition(global::IBGCBLLKIHA.GAFCIOAEGKD(__instance.GetPosition(), jobhdehllbl));
				}
				if (__instance.CheckSweep())
				{
					break;
				}
			}
			return false;
		}
	}

	class BALLSetEntityVisualsPatch
	{
		[HarmonyPatch(typeof(BallEntity), nameof(BallEntity.SetEntityVisuals))]
		[HarmonyPrefix]
		public static bool SetEntityVisuals_Prefix(BallEntity __instance)

		{
			__instance.entityLayer = global::GameplayEntities.Layer.GAMEPLAY;
			__instance.ballSprite = "ball";
			__instance.SetVisualSprite("main2D", __instance.ballSprite, global::BGHNEHPFHGC.NMJDMHNMDNJ, true, false, global::GameplayEntities.BallEntity.GetFrameSize(), global::JKMAAHELEMF.DBOMOJGKIFI, 0f, false, 1f, global::GameplayEntities.Layer.GAMEPLAY, default(global::UnityEngine.Color32));
			__instance.AddBallTrail(90);
			__instance.standardScale = 1f;
			__instance.AddExtraBallVisuals(1f);
			__instance.SetVisualModel("main", global::AOOJOMIECLD.EGOPOAJDFAJ("ball", new string[]
			{
				"ballMat",
				"multiballMat"
			}, 1f, 0, global::FKBHNEMDBMK.NMJDMHNMDNJ), false, true);
			__instance.GetVisual("main").flipMode = global::GameplayEntities.FlipMode.NOT_AUTO;
			return false;
		}

	}


	class GETUPSetAbilityStatePatch
	{
		[HarmonyPatch(typeof(GetUpGrabAbility), nameof(GetUpGrabAbility.SetAbilityState))]
		[HarmonyPrefix]
		public static bool SetAbilityState_Prefix(GetUpGrabAbility __instance, ref bool __return, string state)

		{
			if (state == "GET_UP_GRAB_PRE")
			{
				__return = __instance.data.canBeHitByBall = false;

			}
			if (state == "GET_UP_GRAB_DURING")
			{
				__return = __instance.data.inGetUpBlaze = true;
				__return = __instance.data.canBeHitByBall = false;
				global::LLHandlers.EffectHandler.instance.CreateGetUpBlazeEffect(__instance.entity.GetPosition(), __instance.entity.GetTeam());
				__instance.entity.PlaySfx(global::LLHandlers.Sfx.GETUPGRAB); global::LLHandlers.EffectHandler.instance.CreateGetUpBlazeEffect(__instance.entity.GetPosition(), __instance.entity.GetTeam());
				__instance.entity.PlaySfx(global::LLHandlers.Sfx.GETUPGRAB);
			}
			else
			{
				__return = __instance.data.inGetUpBlaze = false;
				__return = __instance.data.canBeHitByBall = true;
			}
			__instance.SetAbilityState(state);
			return false;
		}

	}

	class PLAYERSetEntityValuesPatch
	{
		[HarmonyPatch(typeof(PlayerEntity), nameof(PlayerEntity.SetEntityValues))]
		[HarmonyPrefix]
		public static bool SetEntityValues_Prefix(PlayerEntity __instance)
		{
			global::HHBCPNCDNDH fpixel60_SIZE = global::World.FPIXEL60_SIZE;
			global::HHBCPNCDNDH gcpkphmklbn = global::HHBCPNCDNDH.FCGOICMIBEA(global::HHBCPNCDNDH.GNIKMEGGCEP, global::HHBCPNCDNDH.NKKIFJJEPOL(60.0m));
			__instance.chargeMaxDuration = __instance.framesDuration60fps(19);
			__instance.fullChargeMargin = __instance.framesDuration60fps(4);
			__instance.getHitKillDuration = __instance.framesDuration60fps(26);
			__instance.deathRattle = global::HHBCPNCDNDH.NKKIFJJEPOL(1.0m);
			__instance.getHitDuration = __instance.framesDuration60fps(6);
			__instance.parryHitstunDuration = global::HHBCPNCDNDH.AJOCFFLIIIH(gcpkphmklbn, global::HHBCPNCDNDH.NKKIFJJEPOL(15));
			__instance.parryKnockbackDurationStandard = global::HHBCPNCDNDH.AJOCFFLIIIH(gcpkphmklbn, global::HHBCPNCDNDH.NKKIFJJEPOL(29));
			__instance.parryKnockbackSpeed = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(13.0m), fpixel60_SIZE);
			__instance.deflectVelocity = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(16.0m), global::World.FPIXEL_SIZE);
			__instance.deflectWindUpDuration = global::HHBCPNCDNDH.AJOCFFLIIIH(gcpkphmklbn, global::HHBCPNCDNDH.NKKIFJJEPOL(11.0m));
			__instance.slideDeacc = global::HHBCPNCDNDH.NKKIFJJEPOL(13);
			__instance.autoSlideDeacc = global::HHBCPNCDNDH.NKKIFJJEPOL(34);
			__instance.crouchInDuration = global::HHBCPNCDNDH.NKKIFJJEPOL(0.03m);
			__instance.crouchOutDuration = global::HHBCPNCDNDH.NKKIFJJEPOL(0.03m);
			__instance.crouchMinDuration = global::HHBCPNCDNDH.NKKIFJJEPOL(0.09m);
			__instance.maxAirMove = global::HHBCPNCDNDH.NKKIFJJEPOL(6.0m);
			__instance.extraJumpAmount = 1;
			__instance.landDuration = __instance.framesDuration60fps(5);
			__instance.turnRunFrames = 14;
			__instance.preWindUpDuration = __instance.framesDuration60fps(6);
			__instance.windUpDuration = __instance.framesDuration60fps(7);
			__instance.swing1Duration = __instance.framesDuration60fps(4);
			__instance.swing2Duration = __instance.framesDuration60fps(3);
			__instance.followThroughDuration = __instance.framesDuration60fps(8);
			__instance.swingReturnDuration = __instance.framesDuration60fps(3);
			__instance.preBuntDuration = __instance.framesDuration60fps(4);
			__instance.buntInDuration = __instance.framesDuration60fps(10);
			__instance.buntWooshDuration = __instance.framesDuration60fps(3);
			__instance.buntOutDuration = __instance.framesDuration60fps(8);
			__instance.smashWindUpDuration = __instance.framesDuration60fps(5);
			__instance.smashReadyDuration = __instance.framesDuration60fps(9);
			__instance.smashOverheadSwingDuration = __instance.framesDuration60fps(4);
			__instance.smashFrontSwingDuration = __instance.framesDuration60fps(3);
			__instance.smashOutDuration = __instance.framesDuration60fps(11);
			__instance.expressWindUpDuration = global::HHBCPNCDNDH.NKKIFJJEPOL(1.0m);
			__instance.expressCooldownDuration = global::HHBCPNCDNDH.NKKIFJJEPOL(1.2m);
			__instance.gravityForce = global::HHBCPNCDNDH.NKKIFJJEPOL(54);
			__instance.gravityForceUp = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(1.0m), fpixel60_SIZE), global::HHBCPNCDNDH.NKKIFJJEPOL(60));
			__instance.gravityForceApex = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(0.3m), fpixel60_SIZE), global::HHBCPNCDNDH.NKKIFJJEPOL(60));
			__instance.jumpPower = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(22.0m), fpixel60_SIZE);
			__instance.groundAcc = (__instance.groundDeacc = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(2.0m), fpixel60_SIZE), global::HHBCPNCDNDH.NKKIFJJEPOL(60)));
			__instance.airDeacc = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(0.4m), fpixel60_SIZE), global::HHBCPNCDNDH.NKKIFJJEPOL(60));
			__instance.maxMove = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(11.0m), fpixel60_SIZE);
			__instance.apexIn = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(4.0m), fpixel60_SIZE);
			__instance.apexOut = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.GANELPBAOPN(global::HHBCPNCDNDH.NKKIFJJEPOL(4.0m)), fpixel60_SIZE);
			__instance.gravityForceFastFall = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(24.0m), fpixel60_SIZE);
			__instance.maxGravity = __instance.gravityForceFastFall;
			__instance.knockbackGravity = global::HHBCPNCDNDH.NKKIFJJEPOL(58m);
			__instance.stunGroundDeacc = global::HHBCPNCDNDH.NKKIFJJEPOL(31m);
			__instance.stunAirDeacc = global::HHBCPNCDNDH.NKKIFJJEPOL(7.2m);
			__instance.hitAngleUpForward = __instance.hitAngleUp;
			__instance.startRunDuration = __instance.framesDuration60fps(9);
			__instance.airAcc = __instance.groundAcc;
			__instance.startParryBeforeEndMinusThis = __instance.framesDuration60fps(10);
			__instance.minimumParryDuration = __instance.framesDuration60fps(24);
			__instance.canJumpCancelCharge = true;
			return false;
		}

	}
	class SoundBlastPlayersInBoxPatch
	{
		[HarmonyPatch(typeof(PlayerHandler), nameof(PlayerHandler.SoundBlastPlayersInBox))]
		[HarmonyPrefix]
		public static bool SoundBlastPlayersInBox_Prefix(PlayerEntity __instance, global::JEPKNLONCHD box, global::IBGCBLLKIHA dir, global::HHBCPNCDNDH speed, global::GameplayEntities.PlayerEntity exceptPlayer)
		{
			global::ALDOKEMAOMB.ICOCPAFKCCE(delegate (global::GameplayEntities.PlayerEntity playerEntity)
			{
				if (playerEntity != null && playerEntity != exceptPlayer && playerEntity.OnGround() && playerEntity.playerData.playerState != global::GameplayEntities.PlayerState.HITPAUSE && playerEntity.playerData.canBeHitByBall && !playerEntity.AnythingTracked() && playerEntity.character != global::Character.DUMMY && playerEntity.GetCurrentlyActiveHurtbox() != null && box.GJNBHIPKIFH(playerEntity.GetCurrentlyActiveHurtbox().bounds))
				{
					global::HHBCPNCDNDH bjdabajoebn = global::HHBCPNCDNDH.NKKIFJJEPOL(1);
					playerEntity.playerData.heading = ((!global::HHBCPNCDNDH.OAHDEOGKOIM(dir.GCPKPHMKLBN, global::HHBCPNCDNDH.NKKIFJJEPOL(0))) ? global::GameplayEntities.Side.RIGHT : global::GameplayEntities.Side.LEFT);
					playerEntity.StartHitstun(playerEntity.smallKnockbackDuration, global::GameplayEntities.HitstunState.HIT_BY_SOUNDBLAST_STUN);
					playerEntity.playerData.velocity = global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(dir, speed), bjdabajoebn);
					playerEntity.playerData.canBeHitByBall = (playerEntity.GetCurrentlyActiveHurtbox() != null);
				}
			});
			return false;
		}
	}
	class SetFlySpeedOnHitPatch
	{
		[HarmonyPatch(typeof(CorpseEntity), nameof(CorpseEntity.SetFlySpeedOnHit))]
		[HarmonyPrefix]
		public static bool SetFlySpeedOnHit_Prefix(CorpseEntity __instance, int charge)
		{
			__instance.SetFlySpeed(global::HHBCPNCDNDH.GAFCIOAEGKD(__instance.hitableData.flySpeed, global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(charge), global::World.FPIXEL60_SIZE)), true);
			return false;
		}
	}
	class CORPSESetEntityValuesPatch
	{
		[HarmonyPatch(typeof(CorpseEntity), nameof(CorpseEntity.SetEntityValues))]
		[HarmonyPrefix]
		public static bool SetEntityValues_Prefix(CorpseEntity __instance)
		{
			__instance.itemType = LLHandlers.ItemType.CORPSE;
			__instance.maxFlySpeed = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(100.0m), global::World.FPIXEL60_SIZE);
			__instance.minFlySpeed = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(19.0m), global::World.FPIXEL60_SIZE);
			__instance.maxGravity = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(1200.0m), global::World.FPIXEL_SIZE);
			__instance.buntUpSpeed = global::HHBCPNCDNDH.NKKIFJJEPOL(18.0m);
			__instance.duration = global::HHBCPNCDNDH.NKKIFJJEPOL(-1);
			__instance.gravity = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(0x1B), global::World.FPIXEL60_SIZE);
			__instance.canBeDamagedBy = ItemEntity.ITEM_DAMAGE_TYPE.NOTHING;
			__instance.deacc = (__instance.airDeacc = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(6), global::World.FPIXEL60_SIZE));
			__instance.hitstunDuration = global::HHBCPNCDNDH.DBOMOJGKIFI;
			__instance.fullHp = 0xA;
			__instance.rotateByHit = true;
			__instance.itemData.hasStageCollision = false;
			__instance.itemData.canBeHitByPlayer = true;
			__instance.canHitPlayersInGeneral = false;
			__instance.ResetChangingDataValues();
			__instance.isCorpse = true;
			__instance.noGravityIfHit = true;
			return false;
		}
	}

	class BagPlayerSetEntityBoxesPatch
	{
		[HarmonyPatch(typeof(BagPlayer), nameof(BagPlayer.SetEntityBoxes))]
		[HarmonyPrefix]
		public static bool SetEntityBoxes_Prefix(BagPlayer __instance)
		{
			global::HHBCPNCDNDH fpixel_SIZE = global::World.FPIXEL_SIZE;
			__instance.moveBox = __instance.AddBox(new global::GameplayEntities.Box(__instance.GetPosition(), global::IBGCBLLKIHA.DBOMOJGKIFI, global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(60, __instance.pxHeight), fpixel_SIZE), global::GameplayEntities.BoxType.MOVEBOX, false));
			__instance.moveBox.active = true;
			__instance.CreateHurtbox("NORMAL_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(60, __instance.pxHeight), fpixel_SIZE), global::IBGCBLLKIHA.DBOMOJGKIFI);
			__instance.CreateHurtbox("HALF_CROUCH_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(60, 96), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(21)), fpixel_SIZE));
			__instance.CreateHurtbox("CROUCH_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(80, 64), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(37)), fpixel_SIZE));

			global::IBGCBLLKIHA sizeSwing = new global::IBGCBLLKIHA(110, __instance.pxHeight);
			global::IBGCBLLKIHA offsetSwing = new global::IBGCBLLKIHA(55, 0);
			global::IBGCBLLKIHA acihfibjnkm = new global::IBGCBLLKIHA(110, 85);
			global::IBGCBLLKIHA acihfibjnkm2 = new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(0), global::HHBCPNCDNDH.NKKIFJJEPOL(113.5m));
			__instance.SetFrontHitboxesAndParryBoxes(sizeSwing, offsetSwing);
			__instance.CreateHitbox("SMASH_TOP_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(acihfibjnkm, fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(acihfibjnkm2, fpixel_SIZE), "SMASH_OVERHEAD_HIT", true, default(global::HHBCPNCDNDH), string.Empty);
			__instance.CreateHitbox("DOWN_AIR_HITBOX", new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(acihfibjnkm.GCPKPHMKLBN, fpixel_SIZE), global::HHBCPNCDNDH.NKKIFJJEPOL(1.1m)), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(170), fpixel_SIZE)), new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(0), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(-95), fpixel_SIZE)), "DOWN_AIR_HIT", false, default(global::HHBCPNCDNDH), string.Empty);
			global::GameplayEntities.PlayerHitbox playerHitbox = __instance.CreateHitbox("BUNT_HITBOX", new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.GCPKPHMKLBN, fpixel_SIZE), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.NKKIFJJEPOL(0.75m)), fpixel_SIZE)), new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(offsetSwing.GCPKPHMKLBN, fpixel_SIZE), global::HHBCPNCDNDH.FCKBPDNEAOG(offsetSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.NKKIFJJEPOL(0.125m)), fpixel_SIZE))), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty);
			playerHitbox.bunts = true;
			playerHitbox = __instance.CreateHitbox("BUNT_HITBOX2", new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(80), fpixel_SIZE), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.GAFCIOAEGKD(global::HHBCPNCDNDH.NKKIFJJEPOL(40), global::HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.NKKIFJJEPOL(0.25m))), fpixel_SIZE)), new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(70), fpixel_SIZE), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.FCKBPDNEAOG(global::HHBCPNCDNDH.GAFCIOAEGKD(global::HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.NKKIFJJEPOL(0.5m)), global::HHBCPNCDNDH.NKKIFJJEPOL(20)), global::HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.NKKIFJJEPOL(0.125m))), fpixel_SIZE)), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty);
			playerHitbox.bunts = true;
			__instance.CreateHitbox("TAUNT_ACTION_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(55, 40), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(60, -55), fpixel_SIZE), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty).bunts = true;
			__instance.CreateHurtbox("LIE_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(sizeSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.NKKIFJJEPOL(45)), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(40)), fpixel_SIZE));
			__instance.SetEntityBoxes();
			return false;

		}

	}

	class BagSetFrontHitboxesAndParryBoxesPatch
	{
		[HarmonyPatch(typeof(BagPlayer), nameof(BagPlayer.SetFrontHitboxesAndParryBoxes))]
		[HarmonyPrefix]
		public static bool SetFrontHitboxesAndParryBoxes_Prefix(BagPlayer __instance, global::IBGCBLLKIHA sizeSwing, global::IBGCBLLKIHA offsetSwing)
		{
			global::HHBCPNCDNDH fpixel_SIZE = global::World.FPIXEL_SIZE;
			__instance.CreateHurtbox("LANDING_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(80, 94), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(22)), fpixel_SIZE));
			__instance.CreateHitbox("NEUTRAL_HITBOX", new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.GAFCIOAEGKD(sizeSwing.GCPKPHMKLBN, global::HHBCPNCDNDH.NKKIFJJEPOL(14)), fpixel_SIZE), global::HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.CGJJEHPPOAN, fpixel_SIZE)), new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.GAFCIOAEGKD(offsetSwing.GCPKPHMKLBN, global::HHBCPNCDNDH.NKKIFJJEPOL(7)), fpixel_SIZE), global::HHBCPNCDNDH.AJOCFFLIIIH(offsetSwing.CGJJEHPPOAN, fpixel_SIZE)), "SWING_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0), "SWING_FULLCHARGE_HIT");
			__instance.CreateHitbox("SMASH_HITBOX", new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.GAFCIOAEGKD(sizeSwing.GCPKPHMKLBN, global::HHBCPNCDNDH.NKKIFJJEPOL(14)), fpixel_SIZE), global::HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.CGJJEHPPOAN, fpixel_SIZE)), new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.GAFCIOAEGKD(offsetSwing.GCPKPHMKLBN, global::HHBCPNCDNDH.NKKIFJJEPOL(7)), fpixel_SIZE), global::HHBCPNCDNDH.AJOCFFLIIIH(offsetSwing.CGJJEHPPOAN, fpixel_SIZE)), "SMASH_FRONT_HIT", true, default(global::HHBCPNCDNDH), string.Empty);
			__instance.parryBox = __instance.AddBox(new global::GameplayEntities.Box(__instance.GetPosition(), global::IBGCBLLKIHA.DBOMOJGKIFI, global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(100, 100), global::World.FPIXEL_SIZE), global::GameplayEntities.BoxType.OTHER, true));
			__instance.counterParryBox = __instance.AddBox(new global::GameplayEntities.Box(__instance.GetPosition(), new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(offsetSwing.GCPKPHMKLBN, fpixel_SIZE), global::HHBCPNCDNDH.AJOCFFLIIIH(offsetSwing.CGJJEHPPOAN, fpixel_SIZE)), global::IBGCBLLKIHA.AJOCFFLIIIH(sizeSwing, fpixel_SIZE), global::GameplayEntities.BoxType.OTHER, true));
			__instance.parryBox.parentless = true;
			global::GameplayEntities.PlayerHitbox playerHitbox = __instance.CreateHitbox("GRAB_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(sizeSwing, fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(offsetSwing, fpixel_SIZE), "GRAB_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.2m), string.Empty);
			playerHitbox.grabs = true;
			playerHitbox.cantBeParried = true;
			return false;
		}
	}

	class BoomPlayerSetEntityBoxesPatch
	{
		[HarmonyPatch(typeof(BoomPlayer), nameof(BoomPlayer.SetEntityBoxes))]
		[HarmonyPrefix]
		public static bool SetEntityBoxes_Prefix(BoomPlayer __instance)
		{
			global::HHBCPNCDNDH fpixel_SIZE = global::World.FPIXEL_SIZE;
			__instance.moveBox = __instance.AddBox(new global::GameplayEntities.Box(__instance.GetPosition(), global::IBGCBLLKIHA.DBOMOJGKIFI, global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(60, 140), fpixel_SIZE), global::GameplayEntities.BoxType.MOVEBOX, false));
			__instance.moveBox.active = true;
			__instance.CreateHurtbox("NORMAL_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(60, __instance.pxHeight), fpixel_SIZE), global::IBGCBLLKIHA.DBOMOJGKIFI);
			__instance.CreateHurtbox("HALF_CROUCH_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(60, 104), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(18)), fpixel_SIZE));
			__instance.CreateHurtbox("CROUCH_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(80, 70), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(35)), fpixel_SIZE));
			__instance.CreateHurtbox("LANDING_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(80, 100), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(20)), fpixel_SIZE));
			global::IBGCBLLKIHA sizeSwing = new global::IBGCBLLKIHA(124, __instance.pxHeight);
			global::IBGCBLLKIHA offsetSwing = new global::IBGCBLLKIHA(57, 0);
			global::IBGCBLLKIHA acihfibjnkm = new global::IBGCBLLKIHA(110, 68);
			global::IBGCBLLKIHA acihfibjnkm2 = new global::IBGCBLLKIHA(0, 104);
			__instance.SetFrontHitboxesAndParryBoxes(sizeSwing, offsetSwing);
			__instance.CreateHitbox("SMASH_TOP_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(acihfibjnkm, fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(acihfibjnkm2, fpixel_SIZE), "SMASH_OVERHEAD_HIT", true, default(global::HHBCPNCDNDH), string.Empty);
			__instance.CreateHitbox("DOWN_AIR_HITBOX", new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(acihfibjnkm.GCPKPHMKLBN, fpixel_SIZE), global::HHBCPNCDNDH.NKKIFJJEPOL(1.1m)), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(170), fpixel_SIZE)), new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(0), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(-95), fpixel_SIZE)), "DOWN_AIR_HIT", false, default(global::HHBCPNCDNDH), string.Empty);
			global::GameplayEntities.PlayerHitbox playerHitbox = __instance.CreateHitbox("BUNT_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(124, 106), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(57, -17), fpixel_SIZE), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty);
			playerHitbox.bunts = true;
			playerHitbox = __instance.CreateHitbox("BUNT_HITBOX2", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(89, 74), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(74.5m), global::HHBCPNCDNDH.NKKIFJJEPOL(73)), fpixel_SIZE), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty);
			playerHitbox.bunts = true;
			__instance.CreateHitbox("TAUNT_ACTION_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(80, 35), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(-35), global::HHBCPNCDNDH.NKKIFJJEPOL(-5)), fpixel_SIZE), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty).bunts = true;
			__instance.CreateHurtbox("LIE_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(sizeSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.NKKIFJJEPOL(45)), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(40)), fpixel_SIZE));
			return false;
		}

	}

	class BossPlayerSetEntityBoxesPatch
	{
		[HarmonyPatch(typeof(BossPlayer), nameof(BossPlayer.SetEntityBoxes))]
		[HarmonyPrefix]
		public static bool SetEntityBoxes_Prefix(BossPlayer __instance)
		{
			global::HHBCPNCDNDH fpixel_SIZE = global::World.FPIXEL_SIZE;
			__instance.moveBox = __instance.AddBox(new global::GameplayEntities.Box(__instance.GetPosition(), global::IBGCBLLKIHA.DBOMOJGKIFI, global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(100, __instance.pxHeight), fpixel_SIZE), global::GameplayEntities.BoxType.MOVEBOX, false));
			__instance.moveBox.active = true;
			__instance.CreateHurtbox("NORMAL_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(100, __instance.pxHeight), fpixel_SIZE), global::IBGCBLLKIHA.DBOMOJGKIFI);
			__instance.CreateHurtbox("HALF_CROUCH_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(100, 84), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(43)), fpixel_SIZE));
			__instance.CreateHurtbox("CROUCH_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(100, 60), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(62)), fpixel_SIZE));
			__instance.CreateHurtbox("LANDING_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(100, 150), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(12)), fpixel_SIZE));
			global::IBGCBLLKIHA sizeSwing = new global::IBGCBLLKIHA(170, __instance.pxHeight);
			global::IBGCBLLKIHA offsetSwing = new global::IBGCBLLKIHA(87, 0);
			__instance.SetFrontHitboxesAndParryBoxes(sizeSwing, offsetSwing);
			__instance.CreateHitbox("SMASH_TOP_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(120, 120), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(10, 145), fpixel_SIZE), "SMASH_OVERHEAD_HIT", true, default(global::HHBCPNCDNDH), string.Empty);
			__instance.CreateHitbox("DOWN_AIR_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(120, 170), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(0, -95), fpixel_SIZE), "DOWN_AIR_HIT", false, default(global::HHBCPNCDNDH), string.Empty);
			global::GameplayEntities.PlayerHitbox playerHitbox = __instance.CreateHitbox("BUNT_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(sizeSwing.GCPKPHMKLBN, global::HHBCPNCDNDH.NKKIFJJEPOL(128)), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(offsetSwing.GCPKPHMKLBN, global::HHBCPNCDNDH.NKKIFJJEPOL(-21)), fpixel_SIZE), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty);
			playerHitbox.bunts = true;
			playerHitbox = __instance.CreateHitbox("BUNT_HITBOX2", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(122, 82), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(111, 84), fpixel_SIZE), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty);
			playerHitbox.bunts = true;
			__instance.CreateHitbox("TAUNT_ACTION_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(40, 40), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(5, -25), fpixel_SIZE), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty).bunts = true;
			playerHitbox = __instance.CreateHitbox("SOUNDBLAST_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(300, 110), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(120, -40), fpixel_SIZE), "SOUNDBLAST_HIT", false, __instance.framesDuration60fps(23), string.Empty);
			playerHitbox.stunState = global::GameplayEntities.HitstunState.HIT_BY_SOUNDBLAST_STUN;
			__instance.CreateHurtbox("LIE_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(sizeSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.NKKIFJJEPOL(45)), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(40)), fpixel_SIZE));
			return false;
		}

	}

	class CandyPlayerSetEntityBoxesPatch
	{
		[HarmonyPatch(typeof(CandyPlayer), nameof(CandyPlayer.SetEntityBoxes))]
		[HarmonyPrefix]
		public static bool SetEntityBoxes_Prefix(CandyPlayer __instance)
		{
			global::HHBCPNCDNDH fpixel_SIZE = global::World.FPIXEL_SIZE;
			__instance.moveBox = __instance.AddBox(new global::GameplayEntities.Box(__instance.GetPosition(), global::IBGCBLLKIHA.DBOMOJGKIFI, global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(60, __instance.pxHeight), fpixel_SIZE), global::GameplayEntities.BoxType.MOVEBOX, false));
			__instance.moveBox.active = true;
			__instance.CreateHurtbox("NORMAL_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(60, __instance.pxHeight), fpixel_SIZE), global::IBGCBLLKIHA.DBOMOJGKIFI);
			__instance.CreateHurtbox("HALF_CROUCH_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(60, 110), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(21)), fpixel_SIZE));
			__instance.CreateHurtbox("CROUCH_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(80, 74), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(39)), fpixel_SIZE));
			__instance.CreateHurtbox("LANDING_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(80, 74), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(39)), fpixel_SIZE));
			global::IBGCBLLKIHA sizeSwing = new global::IBGCBLLKIHA(124, __instance.pxHeight);
			global::IBGCBLLKIHA offsetSwing = new global::IBGCBLLKIHA(57, 0);
			global::IBGCBLLKIHA acihfibjnkm = new global::IBGCBLLKIHA(110, 70);
			global::IBGCBLLKIHA acihfibjnkm2 = new global::IBGCBLLKIHA(0, 111);
			__instance.SetFrontHitboxesAndParryBoxes(sizeSwing, offsetSwing);
			__instance.CreateHitbox("SMASH_TOP_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(acihfibjnkm, fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(acihfibjnkm2, fpixel_SIZE), "SMASH_OVERHEAD_HIT", true, default(global::HHBCPNCDNDH), string.Empty);
			__instance.CreateHitbox("DOWN_AIR_HITBOX", new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(acihfibjnkm.GCPKPHMKLBN, fpixel_SIZE), global::HHBCPNCDNDH.NKKIFJJEPOL(1.1m)), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(170), fpixel_SIZE)), new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(0), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(-95), fpixel_SIZE)), "DOWN_AIR_HIT", false, default(global::HHBCPNCDNDH), string.Empty);
			global::GameplayEntities.PlayerHitbox playerHitbox = __instance.CreateHitbox("BUNT_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(90, 78), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(75, 77), fpixel_SIZE), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty);
			playerHitbox.bunts = true;
			playerHitbox = __instance.CreateHitbox("BUNT_HITBOX2", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(126, 114), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(57, -19), fpixel_SIZE), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty);
			playerHitbox.bunts = true;
			__instance.CreateHitbox("TAUNT_ACTION_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(40, 40), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(15, 0), fpixel_SIZE), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty).bunts = true;
			__instance.CreateHurtbox("LIE_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(sizeSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.NKKIFJJEPOL(45)), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(40)), fpixel_SIZE));
			return false;
		}

	}

	class CopPlayerSetEntityBoxesPatch
	{
		[HarmonyPatch(typeof(CopPlayer), nameof(CopPlayer.SetEntityBoxes))]
		[HarmonyPrefix]
		public static bool SetEntityBoxes_Prefix(CopPlayer __instance)
		{
			global::HHBCPNCDNDH fpixel_SIZE = global::World.FPIXEL_SIZE;
			__instance.moveBox = __instance.AddBox(new global::GameplayEntities.Box(__instance.GetPosition(), global::IBGCBLLKIHA.DBOMOJGKIFI, global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(65, __instance.pxHeight), fpixel_SIZE), global::GameplayEntities.BoxType.MOVEBOX, false));
			__instance.moveBox.active = true;
			__instance.CreateHurtbox("NORMAL_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(65, __instance.pxHeight), fpixel_SIZE), global::IBGCBLLKIHA.DBOMOJGKIFI);
			__instance.CreateHurtbox("HALF_CROUCH_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(65, 114), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(17)), fpixel_SIZE));
			__instance.CreateHurtbox("CROUCH_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(80, 90), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(29)), fpixel_SIZE));
			__instance.CreateHurtbox("LANDING_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(80, 90), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(29)), fpixel_SIZE));
			global::IBGCBLLKIHA sizeSwing = new global::IBGCBLLKIHA(124, __instance.pxHeight);
			global::IBGCBLLKIHA offsetSwing = new global::IBGCBLLKIHA(57, 0);
			global::IBGCBLLKIHA acihfibjnkm = new global::IBGCBLLKIHA(110, 64);
			global::IBGCBLLKIHA acihfibjnkm2 = new global::IBGCBLLKIHA(0, 106);
			__instance.SetFrontHitboxesAndParryBoxes(sizeSwing, offsetSwing);
			__instance.CreateHitbox("SMASH_TOP_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(acihfibjnkm, fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(acihfibjnkm2, fpixel_SIZE), "SMASH_OVERHEAD_HIT", true, default(global::HHBCPNCDNDH), string.Empty);
			__instance.CreateHitbox("DOWN_AIR_HITBOX", new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(acihfibjnkm.GCPKPHMKLBN, fpixel_SIZE), global::HHBCPNCDNDH.NKKIFJJEPOL(1.1m)), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(170), fpixel_SIZE)), new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(0), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(-95), fpixel_SIZE)), "DOWN_AIR_HIT", false, default(global::HHBCPNCDNDH), string.Empty);
			global::GameplayEntities.PlayerHitbox playerHitbox = __instance.CreateHitbox("BUNT_HITBOX", new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.GCPKPHMKLBN, fpixel_SIZE), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.NKKIFJJEPOL(0.75m)), fpixel_SIZE)), new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(offsetSwing.GCPKPHMKLBN, fpixel_SIZE), global::HHBCPNCDNDH.FCKBPDNEAOG(offsetSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.NKKIFJJEPOL(0.125m)), fpixel_SIZE))), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty);
			playerHitbox.bunts = true;
			playerHitbox = __instance.CreateHitbox("BUNT_HITBOX2", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(86.5m), global::HHBCPNCDNDH.NKKIFJJEPOL(77)), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(75.75m), global::HHBCPNCDNDH.NKKIFJJEPOL(75.5m)), fpixel_SIZE), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty);
			playerHitbox.bunts = true;
			__instance.CreateHitbox("TAUNT_ACTION_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(60), global::HHBCPNCDNDH.NKKIFJJEPOL(60)), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(58m), global::HHBCPNCDNDH.NKKIFJJEPOL(53m)), fpixel_SIZE), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty).bunts = true;
			__instance.CreateHurtbox("LIE_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(sizeSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.NKKIFJJEPOL(45)), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(40)), fpixel_SIZE));
			return false;
		}

	}

	class CrocPlayerSetEntityBoxesPatch
	{
		[HarmonyPatch(typeof(CrocPlayer), nameof(CrocPlayer.SetEntityBoxes))]
		[HarmonyPrefix]
		public static bool SetEntityBoxes_Prefix(CrocPlayer __instance)
		{
			global::HHBCPNCDNDH fpixel_SIZE = global::World.FPIXEL_SIZE;
			__instance.moveBox = __instance.AddBox(new global::GameplayEntities.Box(__instance.GetPosition(), global::IBGCBLLKIHA.DBOMOJGKIFI, global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(80, 136), fpixel_SIZE), global::GameplayEntities.BoxType.MOVEBOX, false));
			__instance.moveBox.active = true;
			__instance.CreateHurtbox("NORMAL_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(80, __instance.pxHeight), fpixel_SIZE), global::IBGCBLLKIHA.DBOMOJGKIFI);
			__instance.CreateHurtbox("HALF_CROUCH_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(80, 96), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(20)), fpixel_SIZE));
			__instance.CreateHurtbox("CROUCH_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(80, 60), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(38)), fpixel_SIZE));
			__instance.CreateHurtbox("LANDING_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(80, 80), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(28)), fpixel_SIZE));
			__instance.CreateHitbox("WALL_SWING", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(130, 158), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(55, 11), fpixel_SIZE), "CROC_CLIMB_SWING_HIT", false, default(global::HHBCPNCDNDH), string.Empty);
			global::IBGCBLLKIHA sizeSwing = new global::IBGCBLLKIHA(124, __instance.pxHeight);
			global::IBGCBLLKIHA offsetSwing = new global::IBGCBLLKIHA(57, 0);
			global::IBGCBLLKIHA acihfibjnkm = new global::IBGCBLLKIHA(110, 72);
			global::IBGCBLLKIHA acihfibjnkm2 = new global::IBGCBLLKIHA(0, 104);
			__instance.SetFrontHitboxesAndParryBoxes(sizeSwing, offsetSwing);
			__instance.CreateHitbox("SMASH_TOP_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(acihfibjnkm, fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(acihfibjnkm2, fpixel_SIZE), "SMASH_OVERHEAD_HIT", true, default(global::HHBCPNCDNDH), string.Empty);
			__instance.CreateHitbox("DOWN_AIR_HITBOX", new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(acihfibjnkm.GCPKPHMKLBN, fpixel_SIZE), global::HHBCPNCDNDH.NKKIFJJEPOL(1.1m)), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(170), fpixel_SIZE)), new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(0), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(-95), fpixel_SIZE)), "DOWN_AIR_HIT", false, default(global::HHBCPNCDNDH), string.Empty);
			global::GameplayEntities.PlayerHitbox playerHitbox = __instance.CreateHitbox("BUNT_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(79, 74), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(79.5m), global::HHBCPNCDNDH.NKKIFJJEPOL(71)), fpixel_SIZE), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty);
			playerHitbox.bunts = true;
			playerHitbox = __instance.CreateHitbox("BUNT_HITBOX2", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(124, 102), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(57, -17), fpixel_SIZE), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty);
			playerHitbox.bunts = true;
			__instance.CreateHitbox("TAUNT_ACTION_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(55, 55), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(80, 15), fpixel_SIZE), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty).bunts = true;
			__instance.CreateHurtbox("LIE_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(sizeSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.NKKIFJJEPOL(45)), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(40)), fpixel_SIZE));
			return false;
		}

	}

	class ElectroPlayerSetEntityBoxesPatch
	{
		[HarmonyPatch(typeof(ElectroPlayer), nameof(ElectroPlayer.SetEntityBoxes))]
		[HarmonyPrefix]
		public static bool SetEntityBoxes_Prefix(ElectroPlayer __instance)
		{
			global::HHBCPNCDNDH fpixel_SIZE = global::World.FPIXEL_SIZE;
			__instance.moveBox = __instance.AddBox(new global::GameplayEntities.Box(__instance.GetPosition(), global::IBGCBLLKIHA.DBOMOJGKIFI, global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(70, __instance.pxHeight), fpixel_SIZE), global::GameplayEntities.BoxType.MOVEBOX, false));
			__instance.moveBox.active = true;
			__instance.CreateHurtbox("NORMAL_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(70, __instance.pxHeight), fpixel_SIZE), global::IBGCBLLKIHA.DBOMOJGKIFI);
			__instance.CreateHurtbox("HALF_CROUCH_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(70, 89), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(35)), fpixel_SIZE));
			__instance.CreateHurtbox("CROUCH_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(70, 89), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(35)), fpixel_SIZE));
			__instance.CreateHurtbox("LANDING_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(70, 119), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(20)), fpixel_SIZE));
			global::IBGCBLLKIHA sizeSwing = new global::IBGCBLLKIHA(130, __instance.pxHeight);
			global::IBGCBLLKIHA offsetSwing = new global::IBGCBLLKIHA(55, 0);
			global::IBGCBLLKIHA acihfibjnkm = new global::IBGCBLLKIHA(110, 90);
			global::IBGCBLLKIHA acihfibjnkm2 = new global::IBGCBLLKIHA(0, 127);
			__instance.SetFrontHitboxesAndParryBoxes(sizeSwing, offsetSwing);
			__instance.CreateHitbox("SMASH_TOP_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(acihfibjnkm, fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(acihfibjnkm2, fpixel_SIZE), "SMASH_OVERHEAD_HIT", true, default(global::HHBCPNCDNDH), string.Empty);
			__instance.CreateHitbox("DOWN_AIR_HITBOX", new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(acihfibjnkm.GCPKPHMKLBN, fpixel_SIZE), global::HHBCPNCDNDH.NKKIFJJEPOL(1.1m)), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(170), fpixel_SIZE)), new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(0), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(-95), fpixel_SIZE)), "DOWN_AIR_HIT", false, default(global::HHBCPNCDNDH), string.Empty);
			global::GameplayEntities.PlayerHitbox playerHitbox = __instance.CreateHitbox("BUNT_HITBOX", new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.GCPKPHMKLBN, fpixel_SIZE), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.NKKIFJJEPOL(0.75m)), fpixel_SIZE)), new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(offsetSwing.GCPKPHMKLBN, fpixel_SIZE), global::HHBCPNCDNDH.FCKBPDNEAOG(offsetSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.NKKIFJJEPOL(0.125m)), fpixel_SIZE))), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty);
			playerHitbox.bunts = true;
			playerHitbox = __instance.CreateHitbox("BUNT_HITBOX2", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(85, 80), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(77.5m), global::HHBCPNCDNDH.NKKIFJJEPOL(80)), fpixel_SIZE), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty);
			playerHitbox.bunts = true;
			__instance.CreateHitbox("TAUNT_ACTION_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(75, 60), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(100), global::HHBCPNCDNDH.NKKIFJJEPOL(65)), fpixel_SIZE), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty).bunts = true;
			__instance.CreateHurtbox("LIE_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(sizeSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.NKKIFJJEPOL(45)), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(40)), fpixel_SIZE));
			return false;
		}

	}

	class GrafSetNormalHitboxesPatch
	{
		[HarmonyPatch(typeof(PlayerEntity), nameof(PlayerEntity.SetNormalHitboxes))]
		[HarmonyPrefix]
		public static bool SetNormalHitboxes_Prefix(PlayerEntity __instance, IBGCBLLKIHA sizeSwing, IBGCBLLKIHA offsetSwing, IBGCBLLKIHA sizeTop, IBGCBLLKIHA offsetTop)
		{
			HHBCPNCDNDH fpixel_SIZE = World.FPIXEL_SIZE;
			__instance.SetFrontHitboxesAndParryBoxes(sizeSwing, offsetSwing);
			__instance.CreateHitbox("SMASH_TOP_HITBOX", IBGCBLLKIHA.AJOCFFLIIIH(sizeTop, fpixel_SIZE), IBGCBLLKIHA.AJOCFFLIIIH(offsetTop, fpixel_SIZE), "SMASH_OVERHEAD_HIT", true, default(HHBCPNCDNDH), string.Empty);
			__instance.CreateHitbox("DOWN_AIR_HITBOX", new IBGCBLLKIHA(HHBCPNCDNDH.AJOCFFLIIIH(HHBCPNCDNDH.AJOCFFLIIIH(sizeTop.GCPKPHMKLBN, fpixel_SIZE), HHBCPNCDNDH.NKKIFJJEPOL(1.1m)), HHBCPNCDNDH.AJOCFFLIIIH(HHBCPNCDNDH.NKKIFJJEPOL(170), fpixel_SIZE)), new IBGCBLLKIHA(HHBCPNCDNDH.NKKIFJJEPOL(0), HHBCPNCDNDH.AJOCFFLIIIH(HHBCPNCDNDH.NKKIFJJEPOL(-95), fpixel_SIZE)), "DOWN_AIR_HIT", false, default(HHBCPNCDNDH), string.Empty);
			PlayerHitbox playerHitbox = __instance.CreateHitbox("BUNT_HITBOX", new IBGCBLLKIHA(HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.GCPKPHMKLBN, fpixel_SIZE), HHBCPNCDNDH.AJOCFFLIIIH(HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.CGJJEHPPOAN, HHBCPNCDNDH.NKKIFJJEPOL(.75m)), fpixel_SIZE)), new IBGCBLLKIHA(HHBCPNCDNDH.AJOCFFLIIIH(offsetSwing.GCPKPHMKLBN, fpixel_SIZE), HHBCPNCDNDH.FCKBPDNEAOG(offsetSwing.CGJJEHPPOAN, HHBCPNCDNDH.AJOCFFLIIIH(HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.CGJJEHPPOAN, HHBCPNCDNDH.NKKIFJJEPOL(0.125m)), fpixel_SIZE))), "BUNT_HIT", false, HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty);
			playerHitbox.bunts = true;
			playerHitbox = __instance.CreateHitbox("BUNT_HITBOX2", new IBGCBLLKIHA(HHBCPNCDNDH.AJOCFFLIIIH(HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.GCPKPHMKLBN, HHBCPNCDNDH.NKKIFJJEPOL(0.75m)), fpixel_SIZE), HHBCPNCDNDH.AJOCFFLIIIH(HHBCPNCDNDH.GAFCIOAEGKD(HHBCPNCDNDH.NKKIFJJEPOL(40), HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.CGJJEHPPOAN, HHBCPNCDNDH.NKKIFJJEPOL(0.25m))), fpixel_SIZE)), new IBGCBLLKIHA(HHBCPNCDNDH.AJOCFFLIIIH(HHBCPNCDNDH.AJOCFFLIIIH(offsetSwing.GCPKPHMKLBN, HHBCPNCDNDH.NKKIFJJEPOL(1.25m)), fpixel_SIZE), HHBCPNCDNDH.AJOCFFLIIIH(HHBCPNCDNDH.FCKBPDNEAOG(HHBCPNCDNDH.GAFCIOAEGKD(HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.CGJJEHPPOAN, HHBCPNCDNDH.NKKIFJJEPOL(0.5m)), HHBCPNCDNDH.NKKIFJJEPOL(20)), HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.CGJJEHPPOAN, HHBCPNCDNDH.NKKIFJJEPOL(0.125m))), fpixel_SIZE)), "BUNT_HIT", false, HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty);
			playerHitbox.bunts = true;
			__instance.CreateHurtbox("LIE_HURTBOX", IBGCBLLKIHA.AJOCFFLIIIH(new IBGCBLLKIHA(sizeSwing.CGJJEHPPOAN, HHBCPNCDNDH.NKKIFJJEPOL(45)), fpixel_SIZE), IBGCBLLKIHA.AJOCFFLIIIH(IBGCBLLKIHA.AJOCFFLIIIH(IBGCBLLKIHA.DEKDADEGAIK, HHBCPNCDNDH.NKKIFJJEPOL(40)), fpixel_SIZE));
			return false;
		}

	}

	class GrafSetFrontHitboxesAndParryBoxesPatch
	{
		[HarmonyPatch(typeof(GrafPlayer), nameof(GrafPlayer.SetFrontHitboxesAndParryBoxes))]
		[HarmonyPrefix]
		public static bool SetFrontHitboxesAndParryBoxes_Prefix(GrafPlayer __instance, global::IBGCBLLKIHA sizeSwing, global::IBGCBLLKIHA offsetSwing)
		{
			global::HHBCPNCDNDH fpixel_SIZE = global::World.FPIXEL_SIZE;
			__instance.CreateHurtbox("LANDING_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(80, 94), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(22)), fpixel_SIZE));
			__instance.CreateHitbox("TAUNT_ACTION_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(40, 60), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(50), global::HHBCPNCDNDH.NKKIFJJEPOL(10)), fpixel_SIZE), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty).bunts = true;
			__instance.CreateHitbox("NEUTRAL_HITBOX", new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.GAFCIOAEGKD(sizeSwing.GCPKPHMKLBN, global::HHBCPNCDNDH.NKKIFJJEPOL(14)), fpixel_SIZE), global::HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.CGJJEHPPOAN, fpixel_SIZE)), new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.GAFCIOAEGKD(offsetSwing.GCPKPHMKLBN, global::HHBCPNCDNDH.NKKIFJJEPOL(7)), fpixel_SIZE), global::HHBCPNCDNDH.AJOCFFLIIIH(offsetSwing.CGJJEHPPOAN, fpixel_SIZE)), "SWING_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0), "SWING_FULLCHARGE_HIT");
			__instance.CreateHitbox("SMASH_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(sizeSwing, fpixel_SIZE), new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(offsetSwing.GCPKPHMKLBN, fpixel_SIZE), global::HHBCPNCDNDH.AJOCFFLIIIH(offsetSwing.CGJJEHPPOAN, fpixel_SIZE)), "SMASH_FRONT_HIT", true, default(global::HHBCPNCDNDH), string.Empty);
			__instance.parryBox = __instance.AddBox(new global::GameplayEntities.Box(__instance.GetPosition(), global::IBGCBLLKIHA.DBOMOJGKIFI, global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(100, 100), global::World.FPIXEL_SIZE), global::GameplayEntities.BoxType.OTHER, true));
			__instance.counterParryBox = __instance.AddBox(new global::GameplayEntities.Box(__instance.GetPosition(), new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(offsetSwing.GCPKPHMKLBN, fpixel_SIZE), global::HHBCPNCDNDH.AJOCFFLIIIH(offsetSwing.CGJJEHPPOAN, fpixel_SIZE)), global::IBGCBLLKIHA.AJOCFFLIIIH(sizeSwing, fpixel_SIZE), global::GameplayEntities.BoxType.OTHER, true));
			__instance.parryBox.parentless = true;
			global::GameplayEntities.PlayerHitbox playerHitbox = __instance.CreateHitbox("GRAB_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(sizeSwing, fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(offsetSwing, fpixel_SIZE), "GRAB_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.2m), string.Empty);
			playerHitbox.grabs = true;
			playerHitbox.cantBeParried = true;

			return false;
		}

	}

	class KidPlayerSetEntityBoxesPatch
	{
		[HarmonyPatch(typeof(KidPlayer), nameof(KidPlayer.SetEntityBoxes))]
		[HarmonyPrefix]
		public static bool SetEntityBoxes_Prefix(KidPlayer __instance)
		{
			global::HHBCPNCDNDH fpixel_SIZE = global::World.FPIXEL_SIZE;
			__instance.moveBox = __instance.AddBox(new global::GameplayEntities.Box(__instance.GetPosition(), global::IBGCBLLKIHA.DBOMOJGKIFI, global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(60, __instance.pxHeight), fpixel_SIZE), global::GameplayEntities.BoxType.MOVEBOX, false));
			__instance.moveBox.active = true;
			__instance.CreateHurtbox("NORMAL_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(60, __instance.pxHeight), fpixel_SIZE), global::IBGCBLLKIHA.DBOMOJGKIFI);
			__instance.CreateHurtbox("HALF_CROUCH_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(60, 96), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(15)), fpixel_SIZE));
			__instance.CreateHurtbox("CROUCH_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(80, 64), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(31)), fpixel_SIZE));
			__instance.CreateHurtbox("LANDING_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(80, 74), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(26)), fpixel_SIZE));
			global::IBGCBLLKIHA sizeSwing = new global::IBGCBLLKIHA(124, __instance.pxHeight);
			global::IBGCBLLKIHA offsetSwing = new global::IBGCBLLKIHA(57, 0);
			global::IBGCBLLKIHA acihfibjnkm = new global::IBGCBLLKIHA(110, 72);
			global::IBGCBLLKIHA acihfibjnkm2 = new global::IBGCBLLKIHA(0, 99);
			__instance.SetFrontHitboxesAndParryBoxes(sizeSwing, offsetSwing);
			__instance.CreateHitbox("SMASH_TOP_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(acihfibjnkm, fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(acihfibjnkm2, fpixel_SIZE), "SMASH_OVERHEAD_HIT", true, default(global::HHBCPNCDNDH), string.Empty);
			__instance.CreateHitbox("DOWN_AIR_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(121, 170), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(0, -95), fpixel_SIZE), "DOWN_AIR_HIT", false, default(global::HHBCPNCDNDH), string.Empty);
			global::GameplayEntities.PlayerHitbox playerHitbox = __instance.CreateHitbox("BUNT_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(124, 94), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(57, -16), fpixel_SIZE), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty);
			playerHitbox.bunts = true;
			playerHitbox = __instance.CreateHitbox("BUNT_HITBOX2", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(89, 72), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(74.5m), global::HHBCPNCDNDH.NKKIFJJEPOL(67)), fpixel_SIZE), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty);
			playerHitbox.bunts = true;
			__instance.CreateHitbox("TAUNT_ACTION_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(30, 50), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(10m), global::HHBCPNCDNDH.NKKIFJJEPOL(83)), fpixel_SIZE), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty).bunts = true;
			__instance.CreateHurtbox("LIE_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(sizeSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.NKKIFJJEPOL(45)), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(40)), fpixel_SIZE));
			return false;
		}

	}

	class PongPlayerSetEntityBoxesPatch
	{
		[HarmonyPatch(typeof(PongPlayer), nameof(PongPlayer.SetEntityBoxes))]
		[HarmonyPrefix]
		public static bool SetEntityBoxes_Prefix(PongPlayer __instance)
		{
			global::HHBCPNCDNDH fpixel_SIZE = global::World.FPIXEL_SIZE;
			__instance.moveBox = __instance.AddBox(new global::GameplayEntities.Box(__instance.GetPosition(), global::IBGCBLLKIHA.DBOMOJGKIFI, global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(60, __instance.pxHeight), fpixel_SIZE), global::GameplayEntities.BoxType.MOVEBOX, false));
			__instance.moveBox.active = true;
			__instance.CreateHurtbox("NORMAL_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(60, 148), fpixel_SIZE), global::IBGCBLLKIHA.DBOMOJGKIFI);
			__instance.CreateHurtbox("HALF_CROUCH_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(60, 110), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.GGFFJDILCDA, global::HHBCPNCDNDH.NKKIFJJEPOL(-19)), fpixel_SIZE));
			__instance.CreateHurtbox("CROUCH_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(80, 74), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.GGFFJDILCDA, global::HHBCPNCDNDH.NKKIFJJEPOL(-38)), fpixel_SIZE));
			__instance.CreateHurtbox("LANDING_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(80, 74), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.GGFFJDILCDA, global::HHBCPNCDNDH.NKKIFJJEPOL(-38)), fpixel_SIZE));
			global::IBGCBLLKIHA sizeSwing = new global::IBGCBLLKIHA(124, 148);
			global::IBGCBLLKIHA offsetSwing = new global::IBGCBLLKIHA(57, 0);
			global::IBGCBLLKIHA acihfibjnkm = new global::IBGCBLLKIHA(110, 64);
			global::IBGCBLLKIHA acihfibjnkm2 = new global::IBGCBLLKIHA(0, 106);
			__instance.SetFrontHitboxesAndParryBoxes(sizeSwing, offsetSwing);
			__instance.CreateHitbox("SMASH_TOP_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(acihfibjnkm, fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(acihfibjnkm2, fpixel_SIZE), "SMASH_OVERHEAD_HIT", true, default(global::HHBCPNCDNDH), string.Empty);
			__instance.CreateHitbox("DOWN_AIR_HITBOX", new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(acihfibjnkm.GCPKPHMKLBN, fpixel_SIZE), global::HHBCPNCDNDH.NKKIFJJEPOL(1.1m)), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(170), fpixel_SIZE)), new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(0), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(-95), fpixel_SIZE)), "DOWN_AIR_HIT", false, default(global::HHBCPNCDNDH), string.Empty);
			global::GameplayEntities.PlayerHitbox playerHitbox = __instance.CreateHitbox("BUNT_HITBOX", new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.GCPKPHMKLBN, fpixel_SIZE), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.NKKIFJJEPOL(0.75m)), fpixel_SIZE)), new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(offsetSwing.GCPKPHMKLBN, fpixel_SIZE), global::HHBCPNCDNDH.FCKBPDNEAOG(offsetSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.NKKIFJJEPOL(0.125m)), fpixel_SIZE))), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty);
			playerHitbox.bunts = true;
			playerHitbox = __instance.CreateHitbox("BUNT_HITBOX2", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(89, 77), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(74.5m), global::HHBCPNCDNDH.NKKIFJJEPOL(75.5m)), fpixel_SIZE), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty);
			playerHitbox.stunState = global::GameplayEntities.HitstunState.BUNT_STUN;
			playerHitbox.bunts = true;
			__instance.CreateHitbox("TAUNT_ACTION_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(50, 50), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(77.5m), global::HHBCPNCDNDH.NKKIFJJEPOL(10.5m)), fpixel_SIZE), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty).bunts = true;
			__instance.CreateHurtbox("LIE_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(sizeSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.NKKIFJJEPOL(45)), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(40)), fpixel_SIZE));
			return false;
		}

	}

	class RobotPlayerSetEntityBoxesPatch
	{
		[HarmonyPatch(typeof(RobotPlayer), nameof(RobotPlayer.SetEntityBoxes))]
		[HarmonyPrefix]
		public static bool SetEntityBoxes_Prefix(RobotPlayer __instance)
		{
			global::HHBCPNCDNDH fpixel_SIZE = global::World.FPIXEL_SIZE;
			__instance.moveBox = __instance.AddBox(new global::GameplayEntities.Box(__instance.GetPosition(), global::IBGCBLLKIHA.DBOMOJGKIFI, global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(60, __instance.pxHeight), fpixel_SIZE), global::GameplayEntities.BoxType.MOVEBOX, false));
			__instance.moveBox.active = true;
			__instance.CreateHurtbox("NORMAL_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(60, __instance.pxHeight), fpixel_SIZE), global::IBGCBLLKIHA.DBOMOJGKIFI);
			__instance.CreateHurtbox("HALF_CROUCH_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(60, 104), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(21)), fpixel_SIZE));
			__instance.CreateHurtbox("CROUCH_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(80, 70), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(38)), fpixel_SIZE));
			__instance.CreateHurtbox("LANDING_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(80, 100), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(23)), fpixel_SIZE));
			__instance.CreateHitbox("KICKFLIP", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(150, 130), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(0, -47), fpixel_SIZE), "ROBOT_KICKFLIP_HIT", false, default(global::HHBCPNCDNDH), string.Empty);
			__instance.CreateHitbox("KICKFLIP_CEILING", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(150, 130), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(0, 100), fpixel_SIZE), "ROBOT_KICKFLIP_CEILING_HIT", false, default(global::HHBCPNCDNDH), string.Empty);
			global::IBGCBLLKIHA sizeSwing = new global::IBGCBLLKIHA(124, __instance.pxHeight);
			global::IBGCBLLKIHA offsetSwing = new global::IBGCBLLKIHA(57, 0);
			global::IBGCBLLKIHA acihfibjnkm = new global::IBGCBLLKIHA(110, 62);
			global::IBGCBLLKIHA acihfibjnkm2 = new global::IBGCBLLKIHA(0, 104);
			__instance.SetFrontHitboxesAndParryBoxes(sizeSwing, offsetSwing);
			__instance.CreateHitbox("SMASH_TOP_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(acihfibjnkm, fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(acihfibjnkm2, fpixel_SIZE), "SMASH_OVERHEAD_HIT", true, default(global::HHBCPNCDNDH), string.Empty);
			__instance.CreateHitbox("DOWN_AIR_HITBOX", new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(acihfibjnkm.GCPKPHMKLBN, fpixel_SIZE), global::HHBCPNCDNDH.NKKIFJJEPOL(1.1m)), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(170), fpixel_SIZE)), new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(0), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(-95), fpixel_SIZE)), "DOWN_AIR_HIT", false, default(global::HHBCPNCDNDH), string.Empty);
			global::GameplayEntities.PlayerHitbox playerHitbox = __instance.CreateHitbox("BUNT_HITBOX", new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.GCPKPHMKLBN, fpixel_SIZE), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.NKKIFJJEPOL(0.75m)), fpixel_SIZE)), new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(offsetSwing.GCPKPHMKLBN, fpixel_SIZE), global::HHBCPNCDNDH.FCKBPDNEAOG(offsetSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.NKKIFJJEPOL(0.125m)), fpixel_SIZE))), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty);
			playerHitbox.bunts = true;
			playerHitbox = __instance.CreateHitbox("BUNT_HITBOX2", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(89, 76), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(74.5m), global::HHBCPNCDNDH.NKKIFJJEPOL(74.5m)), fpixel_SIZE), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty);
			playerHitbox.stunState = global::GameplayEntities.HitstunState.BUNT_STUN;
			playerHitbox.bunts = true;
			__instance.CreateHitbox("TAUNT_ACTION_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(110, 30), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(5m), global::HHBCPNCDNDH.NKKIFJJEPOL(-65m)), fpixel_SIZE), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty).bunts = true;
			__instance.CreateHurtbox("LIE_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(sizeSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.NKKIFJJEPOL(45)), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(40)), fpixel_SIZE));
			return false;
		}

	}

	class SkatePlayerSetEntityBoxesPatch
	{
		[HarmonyPatch(typeof(SkatePlayer), nameof(SkatePlayer.SetEntityBoxes))]
		[HarmonyPrefix]
		public static bool SetEntityBoxes_Prefix(SkatePlayer __instance)
		{
			global::HHBCPNCDNDH fpixel_SIZE = global::World.FPIXEL_SIZE;
			__instance.moveBox = __instance.AddBox(new global::GameplayEntities.Box(__instance.GetPosition(), global::IBGCBLLKIHA.DBOMOJGKIFI, global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(60, __instance.pxHeight), fpixel_SIZE), global::GameplayEntities.BoxType.MOVEBOX, false));
			__instance.moveBox.active = true;
			__instance.CreateHurtbox("NORMAL_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(60, __instance.pxHeight), fpixel_SIZE), global::IBGCBLLKIHA.DBOMOJGKIFI);
			__instance.CreateHurtbox("HALF_CROUCH_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(60, 104), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(22)), fpixel_SIZE));
			__instance.CreateHurtbox("CROUCH_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(80, 70), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(39)), fpixel_SIZE));
			__instance.CreateHurtbox("LANDING_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(80, 100), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(24)), fpixel_SIZE));
			global::IBGCBLLKIHA sizeSwing = new global::IBGCBLLKIHA(124, __instance.pxHeight);
			global::IBGCBLLKIHA offsetSwing = new global::IBGCBLLKIHA(57, 0);
			global::IBGCBLLKIHA acihfibjnkm = new global::IBGCBLLKIHA(110, 64);
			global::IBGCBLLKIHA acihfibjnkm2 = new global::IBGCBLLKIHA(0, 106);
			__instance.SetFrontHitboxesAndParryBoxes(sizeSwing, offsetSwing);
			__instance.CreateHitbox("SMASH_TOP_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(acihfibjnkm, fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(acihfibjnkm2, fpixel_SIZE), "SMASH_OVERHEAD_HIT", true, default(global::HHBCPNCDNDH), string.Empty);
			__instance.CreateHitbox("DOWN_AIR_HITBOX", new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(acihfibjnkm.GCPKPHMKLBN, fpixel_SIZE), global::HHBCPNCDNDH.NKKIFJJEPOL(1.1m)), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(170), fpixel_SIZE)), new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(0), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(-95), fpixel_SIZE)), "DOWN_AIR_HIT", false, default(global::HHBCPNCDNDH), string.Empty);
			global::GameplayEntities.PlayerHitbox playerHitbox = __instance.CreateHitbox("BUNT_HITBOX", new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.GCPKPHMKLBN, fpixel_SIZE), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.NKKIFJJEPOL(0.75m)), fpixel_SIZE)), new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(offsetSwing.GCPKPHMKLBN, fpixel_SIZE), global::HHBCPNCDNDH.FCKBPDNEAOG(offsetSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(sizeSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.NKKIFJJEPOL(0.125m)), fpixel_SIZE))), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty);
			playerHitbox.bunts = true;
			playerHitbox = __instance.CreateHitbox("BUNT_HITBOX2", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(89, 77), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(74.5m), global::HHBCPNCDNDH.NKKIFJJEPOL(75.5m)), fpixel_SIZE), "BUNT_HIT", false, global::HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty);
			playerHitbox.bunts = true;
			__instance.CreateHitbox("TAUNT_ACTION_HITBOX", IBGCBLLKIHA.AJOCFFLIIIH(new IBGCBLLKIHA(40, 40), fpixel_SIZE), IBGCBLLKIHA.AJOCFFLIIIH(new IBGCBLLKIHA(HHBCPNCDNDH.NKKIFJJEPOL(11m), HHBCPNCDNDH.NKKIFJJEPOL(55m)), fpixel_SIZE), "BUNT_HIT", false, HHBCPNCDNDH.NKKIFJJEPOL(0.08m), string.Empty).bunts = true;
			__instance.CreateHurtbox("LIE_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(sizeSwing.CGJJEHPPOAN, global::HHBCPNCDNDH.NKKIFJJEPOL(45)), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(40)), fpixel_SIZE));
			return false;
		}

	}

	class PongCheckActivationPatch
	{
		[HarmonyPatch(typeof(PongAbility), nameof(PongAbility.CheckActivation))]
		[HarmonyPrefix]
		public static bool CheckActivation_Prefix(PongAbility __instance, ref bool __result, bool newPress = true)
		{
			__result = global::HHBCPNCDNDH.HPLPMEAOJPM(__instance.entity.RemainingHitstunTime(), global::HHBCPNCDNDH.NKKIFJJEPOL(1.75m)) && __instance.CheckActivation(newPress);
			return false;
		}
	}

	class BagCheckActivationPatch
	{
		[HarmonyPatch(typeof(ShadowAbility), nameof(ShadowAbility.CheckActivation))]
		[HarmonyPrefix]
		public static bool CheckActivation_Prefix(ShadowAbility __instance, ref bool __result, bool newPress = true)
		{
			__result = global::HHBCPNCDNDH.HPLPMEAOJPM(__instance.entity.RemainingHitstunTime(), global::HHBCPNCDNDH.NKKIFJJEPOL(1.75m)) && __instance.CheckActivation(newPress);
			return false;
		}
	}

	class BALLSetEntityValuesPatch
	{
		[HarmonyPatch(typeof(BallEntity), nameof(BallEntity.SetEntityValues))]
		[HarmonyPrefix]
		public static bool SetEntityValues_Prefix(BallEntity __instance)
		{
			__instance.SetEntityValues();
			global::HHBCPNCDNDH fpixel60_SIZE = global::World.FPIXEL60_SIZE;
			global::HHBCPNCDNDH gcpkphmklbn = global::HHBCPNCDNDH.FCGOICMIBEA(global::HHBCPNCDNDH.GNIKMEGGCEP, global::HHBCPNCDNDH.NKKIFJJEPOL(60.0m));
			__instance.deadDuration = global::HHBCPNCDNDH.NKKIFJJEPOL(1.5m);
			__instance.wallStunDuration = global::HHBCPNCDNDH.AJOCFFLIIIH(gcpkphmklbn, global::HHBCPNCDNDH.NKKIFJJEPOL(4));
			__instance.pongWallStunDuration = global::HHBCPNCDNDH.AJOCFFLIIIH(gcpkphmklbn, global::HHBCPNCDNDH.NKKIFJJEPOL(10));
			__instance.cuffedWallStunDuration = global::HHBCPNCDNDH.AJOCFFLIIIH(gcpkphmklbn, global::HHBCPNCDNDH.NKKIFJJEPOL(20));
			__instance.soundBallSpeed = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(32.0m), fpixel60_SIZE);
			__instance.pongBallSpeed = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(34.0m), fpixel60_SIZE);
			__instance.wallGrindAcc = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(3.8m), fpixel60_SIZE), global::HHBCPNCDNDH.NKKIFJJEPOL(60));
			__instance.wallGrindSpeed = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(32.0m), fpixel60_SIZE);
			__instance.ceilingGrindSpeed = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(38.0m), fpixel60_SIZE);
			__instance.candyBallMaxSpeed = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(50.0m), fpixel60_SIZE);
			__instance.candyBallMinSpeed = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(8.0m), fpixel60_SIZE);
			__instance.pitchedBallMinSpeed = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(42.0m), fpixel60_SIZE);
			__instance.invisiballSpeed = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(30), global::World.FPIXEL60_SIZE);
			__instance.kickflipBallMinSpeed = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(35), global::World.FPIXEL60_SIZE);
			__instance.graffitiBallMinSpeed = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(30), global::World.FPIXEL60_SIZE);
			__instance.snipeBallMinSpeed = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(20), global::World.FPIXEL60_SIZE);
			__instance.shadowBallMinSpeed = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(22), global::World.FPIXEL60_SIZE);
			__instance.cuffBallSpeed = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(40), global::World.FPIXEL60_SIZE);
			__instance.cuffBallMaxSpeed = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(400), global::World.FPIXEL60_SIZE);
			__instance.bubbleBallInSpeed = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(5), global::World.FPIXEL60_SIZE);
			__instance.bubbleBallOutMinSpeed = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(32), global::World.FPIXEL60_SIZE);
			__instance.bubbleBallOutMaxSpeed = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(70), global::World.FPIXEL60_SIZE);
			__instance.gravityForce = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(0.5m), fpixel60_SIZE), global::HHBCPNCDNDH.NKKIFJJEPOL(60));
			__instance.maxGravity = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(15.0m), fpixel60_SIZE);
			__instance.maxFlySpeed = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(1000000.0m), fpixel60_SIZE);
			__instance.minFlySpeed = global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(global::JOMBNFKIHIC.GIGAKBJGFDI.KHOEFMFHDMC), fpixel60_SIZE);
			__instance.ballType = global::BallType.REGULAR;
			return false;
		}
	}
	class PLAYERSetEntityAbilitiesPatch
	{
		[HarmonyPatch(typeof(AbilityEntity), nameof(AbilityEntity.SetEntityAbilities))]
		[HarmonyPrefix]
		public static bool SetEntityAbilities_Prefix(AbilityEntity __instance)
		{
			__instance.AddSingleAbilityState(new global::Abilities.AbilityState(global::GameplayEntities.PlayerState.NORMAL, global::Abilities.AbilityState.NO_ABILITYSTATE, global::Abilities.AbilityState.NO_ABILITYSTATE, string.Empty, null, global::Abilities.AbilityGroundType.AIR)).moveableOn = global::Abilities.AbilityGroundType.BOTH;
			global::Abilities.AbilityState abilityState = __instance.AddSingleAbilityState(new global::Abilities.AbilityState(global::GameplayEntities.PlayerState.ACTION, "LAND", global::Abilities.AbilityState.NO_ABILITYSTATE, "land", __instance.landDuration, null, global::Abilities.AbilityGroundType.AIR));
			abilityState.hurtboxes = new global::System.Collections.Generic.List<string>
			{
				"LANDING_HURTBOX"
			};
			abilityState.canTurnOnStart = true;
			abilityState.canBeCancelledByAnyAction = true;
			abilityState.canMoveForwardCancel = true;
			abilityState.canTurnCancel = true;
			abilityState.canTurnOnStart = true;

			abilityState = __instance.AddSingleAbilityState(new global::Abilities.AbilityState(global::GameplayEntities.PlayerState.ACTION, "STOP", global::Abilities.AbilityState.NO_ABILITYSTATE, "stop", __instance.framesDuration60fps(8), null, global::Abilities.AbilityGroundType.AIR));
			abilityState.canBeCancelledByAnyAction = true;
			abilityState.canTurnCancel = true;
			abilityState.canTurnOnStart = true;
			abilityState.canMoveForwardCancel = true;
			abilityState = __instance.AddSingleAbilityState(new global::Abilities.AbilityState(global::GameplayEntities.PlayerState.ACTION, "TURN_RUN", "TURN_RUN2", "turnRun", __instance.framesDuration60fps(__instance.turnRunFrames / 2), null, global::Abilities.AbilityGroundType.AIR));
			abilityState.canOnlyMoveInHeading = true;
			abilityState.canBeCancelledByAnyAction = true;
			abilityState = __instance.AddSingleAbilityState(new global::Abilities.AbilityState(global::GameplayEntities.PlayerState.ACTION, "TURN_RUN2", global::Abilities.AbilityState.NO_ABILITYSTATE, string.Empty, __instance.framesDuration60fps(__instance.turnRunFrames / 2), null, global::Abilities.AbilityGroundType.AIR));
			abilityState.canOnlyMoveInHeading = true;
			abilityState.canTurnCancel = true;
			abilityState.canBeCancelledByAnyAction = true;
			abilityState.moveableOn = global::Abilities.AbilityGroundType.GROUND;
			abilityState = __instance.AddSingleAbilityState(new global::Abilities.AbilityState(global::GameplayEntities.PlayerState.ACTION, "TURN", "TURN2", "turn", __instance.framesDuration60fps(3), null, global::Abilities.AbilityGroundType.AIR));
			abilityState.canBeCancelledByAnyAction = true;
			abilityState = __instance.AddSingleAbilityState(new global::Abilities.AbilityState(global::GameplayEntities.PlayerState.ACTION, "TURN2", global::Abilities.AbilityState.NO_ABILITYSTATE, string.Empty, __instance.framesDuration60fps(3), null, global::Abilities.AbilityGroundType.AIR));
			abilityState.canMoveForwardCancel = true;
			abilityState.canBeCancelledByAnyAction = true;
			abilityState.canTurnCancel = true;
			abilityState = __instance.AddSingleAbilityState(new global::Abilities.AbilityState(global::GameplayEntities.PlayerState.ACTION, "START_RUN", "START_RUN2", "startRun", global::HHBCPNCDNDH.FCGOICMIBEA(__instance.startRunDuration, global::HHBCPNCDNDH.NKKIFJJEPOL(2)), null, global::Abilities.AbilityGroundType.AIR));
			abilityState.moveableOn = global::Abilities.AbilityGroundType.GROUND;
			abilityState.canBeCancelledByAnyAction = true;
			abilityState.canTurnCancel = true;
			abilityState.canOnlyMoveInHeading = true;
			abilityState.startBoost = true;
			abilityState = __instance.AddSingleAbilityState(new global::Abilities.AbilityState(global::GameplayEntities.PlayerState.ACTION, "START_RUN2", global::Abilities.AbilityState.NO_ABILITYSTATE, string.Empty, global::HHBCPNCDNDH.FCGOICMIBEA(__instance.startRunDuration, global::HHBCPNCDNDH.NKKIFJJEPOL(2)), null, global::Abilities.AbilityGroundType.AIR));
			abilityState.moveableOn = global::Abilities.AbilityGroundType.GROUND;
			abilityState.canBeCancelledByAnyAction = true;
			abilityState.canTurnCancel = true;
			abilityState.canOnlyMoveInHeading = true;
			__instance.AddAbility(new global::Abilities.KnockedOutAbility(__instance));
			__instance.AddAbility(new global::Abilities.GetUpGrabAbility(__instance));
			__instance.AddAbility(new global::Abilities.GetUpAbility(__instance));
			__instance.smashAbility = (global::Abilities.SmashAbility)__instance.AddAbility(new global::Abilities.SmashAbility(__instance));
			__instance.downAirSwingAbility = (global::Abilities.DownAirSwingAbility)__instance.AddAbility(new global::Abilities.DownAirSwingAbility(__instance));
			__instance.grabAbility = (global::Abilities.GrabAbility)__instance.AddAbility(new global::Abilities.GrabAbility(__instance));
			__instance.AddAbility(new global::Abilities.TauntAbility(__instance));
			__instance.AddAbility(new global::Abilities.ExpressionAbility(__instance));
			__instance.neutralSwingAbility = (global::Abilities.NeutralSwingAbility)__instance.AddAbility(new global::Abilities.NeutralSwingAbility(__instance));
			__instance.AddAbility(new global::Abilities.BuntAbility(__instance));
			__instance.crouchAbility = (global::Abilities.CrouchAbility)__instance.AddAbility(new global::Abilities.CrouchAbility(__instance));
			__instance.pitchAbility = (global::Abilities.PitchAbility)__instance.AddAbility(new global::Abilities.PitchAbility(__instance));
			return false;
		}
	}

	class CONSTRUCTORTauntAbilityPatch
	{
		[HarmonyPatch(typeof(TauntAbility), MethodType.Constructor, new Type[] { typeof(AbilityEntity) })]
		[HarmonyPrefix]
		public static bool Prefix(TauntAbility __instance, global::GameplayEntities.AbilityEntity abilityEntity)
		{
			__instance.Init("taunt", abilityEntity, InputAction.TAUNT, AbilityGroundType.GROUND, int.MaxValue, false, PlayerState.ACTION);
			__instance.AddState(new AbilityState("TAUNT1", "taunt", __instance.entity.expressTauntPhase1Duration, string.Empty, AbilityGroundType.AIR)).castShadowForBag = false;
			__instance.AddState(new AbilityState("TAUNT2", string.Empty, __instance.entity.expressTauntPhase2Duration, new List<string>
			{
				"TAUNT_ACTION_HITBOX"
			}, AbilityGroundType.AIR)).castShadowForBag = false;
			__instance.AddSingleState(new AbilityState("TAUNT_IDLE", "tauntIdle", HHBCPNCDNDH.NKKIFJJEPOL(AbilityState.NO_DURATION), new List<string>
			{
				"TAUNT_ACTION_HITBOX"
			}, AbilityGroundType.AIR)).castShadowForBag = false;
			__instance.AddSingleState(new AbilityState("TAUNT_IDLE_OUT", "tauntOut", __instance.entity.expressTauntPhase2Duration, string.Empty, AbilityGroundType.AIR)).castShadowForBag = false;
			return false;
		}

	}
	class CONSTRUCTOREatAbilityPatch
	{
		[HarmonyPatch(typeof(EatAbility), MethodType.Constructor, new Type[] { typeof(AbilityEntity) })]
		[HarmonyPrefix]
		public static bool Prefix(EatAbility __instance, global::GameplayEntities.AbilityEntity abilityEntity)
		{
			__instance.Init("eat", abilityEntity, global::LLHandlers.InputAction.SWING, global::Abilities.AbilityGroundType.BOTH, int.MaxValue, false, global::GameplayEntities.PlayerState.ACTION);
			__instance.SetSettingsForSpecialMove("eat");
			__instance.activatedByHand = false;
			global::HHBCPNCDNDH gcpkphmklbn = global::HHBCPNCDNDH.NKKIFJJEPOL(0.0166667m);
			__instance.spitWindUpDuration = global::HHBCPNCDNDH.AJOCFFLIIIH(gcpkphmklbn, global::HHBCPNCDNDH.NKKIFJJEPOL(6));
			__instance.spitDuration = global::HHBCPNCDNDH.AJOCFFLIIIH(gcpkphmklbn, global::HHBCPNCDNDH.NKKIFJJEPOL(7));
			__instance.spitOutDuration = global::HHBCPNCDNDH.AJOCFFLIIIH(gcpkphmklbn, global::HHBCPNCDNDH.NKKIFJJEPOL(13));
			__instance.eatDuration = global::HHBCPNCDNDH.AJOCFFLIIIH(gcpkphmklbn, global::HHBCPNCDNDH.NKKIFJJEPOL(10));
			__instance.AddState(new global::Abilities.AbilityState("CROC_BALL_EAT", "eat", __instance.eatDuration, string.Empty, global::Abilities.AbilityGroundType.NONE)).blinkColorOutline = true;
			__instance.AddState(new global::Abilities.AbilityState(global::GameplayEntities.PlayerState.ACTION, "CROC_BALL_MOVE", global::Abilities.AbilityState.NO_ABILITYSTATE, string.Empty, global::HHBCPNCDNDH.NKKIFJJEPOL(global::Abilities.AbilityState.NO_DURATION), null, global::Abilities.AbilityGroundType.BOTH)).blinkColorOutline = true;
			__instance.AddSingleState(new global::Abilities.AbilityState(global::GameplayEntities.PlayerState.ACTION, "CROC_BALL_SPIT_WIND_UP", "CROC_BALL_SPIT", "spitWindUp", __instance.spitWindUpDuration, null, global::Abilities.AbilityGroundType.NONE)).blinkColorOutline = true;
			__instance.AddSingleState(new global::Abilities.AbilityState(global::GameplayEntities.PlayerState.ACTION, "CROC_BALL_EAT_CORPSE", global::Abilities.AbilityState.NO_ABILITYSTATE, "eat", __instance.eatDuration, null, global::Abilities.AbilityGroundType.NONE));
			global::Abilities.AbilityState abilityState = __instance.AddSingleState(new global::Abilities.AbilityState(global::GameplayEntities.PlayerState.ACTION, "CROC_BALL_SPIT", "CROC_BALL_SPIT_FOLLOW_THROUGH", string.Empty, __instance.spitDuration, null, global::Abilities.AbilityGroundType.NONE));
			abilityState.blinkColorOutline = true;
			abilityState.noAim = true;
			abilityState = __instance.AddSingleState(new global::Abilities.AbilityState(global::GameplayEntities.PlayerState.ACTION, "CROC_BALL_SPIT_DEFLECT", "CROC_BALL_SPIT_FOLLOW_THROUGH", string.Empty, __instance.spitDuration, null, global::Abilities.AbilityGroundType.NONE));
			abilityState.blinkColorOutline = true;
			abilityState.noAim = true;
			abilityState = __instance.AddSingleState(new global::Abilities.AbilityState(global::GameplayEntities.PlayerState.ACTION, "CROC_BALL_SPIT_FOLLOW_THROUGH", global::Abilities.AbilityState.NO_ABILITYSTATE, string.Empty, __instance.spitOutDuration, null, global::Abilities.AbilityGroundType.NONE));
			abilityState.noAim = true;
			__instance.ballSpitOffset = global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(45, 15), global::World.FPIXEL_SIZE);
			return false;
		}
	}
	class CONSTRUCTORDownAirSwingAbilityPatch
	{
		[HarmonyPatch(typeof(DownAirSwingAbility), MethodType.Constructor, new Type[] { typeof(AbilityEntity) })]
		[HarmonyPrefix]
		public static bool Prefix(DownAirSwingAbility __instance, global::GameplayEntities.AbilityEntity abilityEntity)
		{
			__instance.Init("downAirSwing", abilityEntity, LLHandlers.InputAction.SWING, AbilityGroundType.AIR, LLHandlers.InputAction.DOWN, false, GameplayEntities.PlayerState.ACTION);
			__instance.isSwingingTypeAbility = true;
			if (global::DebugSettings.instance.testAirAttacksOnGround)
			{
				__instance.activateOnGroundType = AbilityGroundType.BOTH;
				__instance.directionalInput = LLHandlers.InputAction.NONE;
			}
			__instance.swishOffset = global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(0, -0x5A), global::World.FPIXEL_SIZE);
			global::Character character = __instance.entity.character;
			int frames;
			int frames2;
			int frames3;
			int frames4;
			if (character != global::Character.CANDY)
			{
				frames = 4;
				frames2 = 0xB;
				frames3 = 3;
				frames4 = 9;
			}
			else
			{
				frames = 4;
				frames2 = 0xB;
				frames3 = 3;
				frames4 = 9;
			}
			AbilityState abilityState = __instance.AddState(new AbilityState("DOWN_AIR_WIND_UP", "downSwing", __instance.framesDuration60fps(frames), "DOWN_AIR_HITBOX", AbilityGroundType.AIR));
			abilityState.canBufferSpecial = true;
			abilityState.canHitItems = false;
			abilityState = __instance.AddState(new AbilityState("DOWN_AIR_WOOSH", string.Empty, __instance.framesDuration60fps(frames2), "DOWN_AIR_HITBOX", AbilityGroundType.AIR));
			abilityState.canBufferSpecial = true;
			abilityState.castShadowForBag = false;
			abilityState = __instance.AddState(new AbilityState("DOWN_AIR_FOLLOW_THROUGH", "downSwingOut", __instance.framesDuration60fps(frames3), "DOWN_AIR_HITBOX", AbilityGroundType.AIR));
			abilityState.canBufferSpecial = true;
			abilityState.hitboxesOffAfterHitSomething = true;
			abilityState.castShadowForBag = false;
			abilityState = __instance.AddState(new AbilityState("DOWN_AIR_OUT", string.Empty, __instance.framesDuration60fps(frames4), string.Empty, AbilityGroundType.AIR));
			abilityState.castShadowForBag = false;
			abilityState = __instance.AddSingleState(new AbilityState(GameplayEntities.PlayerState.HITPAUSE, "DOWN_AIR_HIT", "DOWN_AIR_FOLLOW_THROUGH", "downSwingHit", new List<string>
			{
				"DOWN_AIR_HITBOX"
			}, AbilityGroundType.AIR));
			abilityState.castShadowForBag = false;
			abilityState = __instance.AddSingleState(new AbilityState(GameplayEntities.PlayerState.ACTION, "DOWN_AIR_LAND", AbilityState.NO_ABILITYSTATE, "abilityLand", __instance.entity.framesDuration60fps(0xC), null, AbilityGroundType.AIR));
			abilityState.moveableOn = AbilityGroundType.NONE;
			return false;
		}
	}

	class MovableEntityMovementPatch
	{
		[HarmonyPatch(typeof(MovableEntity), nameof(MovableEntity.Movement))]
		[HarmonyPrefix]
		public static bool Movement_Prefix(MovableEntity __instance)
		{
			if (global::HHBCPNCDNDH.OAHDEOGKOIM(__instance.moveableData.airControlTimer, global::HHBCPNCDNDH.DBOMOJGKIFI))
			{
				global::GameplayEntities.MovableData movableData = __instance.moveableData;
				movableData.airControlTimer = global::HHBCPNCDNDH.FCKBPDNEAOG(movableData.airControlTimer, global::World.FDELTA_TIME);
			}
			global::IBGCBLLKIHA velocity = __instance.moveableData.velocity;
			global::HHBCPNCDNDH gcpkphmklbn = (!__instance.OnGround()) ? __instance.airAcc : __instance.groundAcc;
			global::HHBCPNCDNDH hhbcpncdndh = (!__instance.OnGround()) ? __instance.maxAirMove : __instance.maxMove;
			hhbcpncdndh = global::HHBCPNCDNDH.GAFCIOAEGKD(hhbcpncdndh, global::HHBCPNCDNDH.NKKIFJJEPOL(0.3m));
			bool flag = __instance.GetAnimDataOfVisual("main").currentAnim == "startRun" && global::HHBCPNCDNDH.HPLPMEAOJPM(__instance.GetAnimDataOfVisual("main").animTime, global::HHBCPNCDNDH.NKKIFJJEPOL(0m));
			bool flag2 = __instance.GetInput(global::LLHandlers.InputAction.RIGHT) || (flag && __instance.moveableData.heading == global::GameplayEntities.Side.RIGHT);
			bool flag3 = __instance.GetInput(global::LLHandlers.InputAction.LEFT) || (flag && __instance.moveableData.heading == global::GameplayEntities.Side.LEFT);
			if (__instance.GetCurrentAbilityState().useUniqueAcc)
			{
				gcpkphmklbn = ((!__instance.OnGround()) ? __instance.GetCurrentAbilityState().airAcc : __instance.GetCurrentAbilityState().groundAcc);
			}
			if (__instance.GetCurrentAbilityState().useUniqueMaxMove)
			{
				hhbcpncdndh = __instance.GetCurrentAbilityState().maxMove;
			}
			global::GameplayEntities.PlayerState playerState = __instance.moveableData.playerState;
			if (__instance.GetInputNew(global::LLHandlers.InputAction.UP) && !__instance.GetInputNew(global::LLHandlers.InputAction.DOWN))
			{
				__instance.moveableData.pivotBuffer = __instance.framesDuration60fps(8);
			}
			if (global::HHBCPNCDNDH.EAOICALCHJI(__instance.moveableData.pivotBuffer, global::HHBCPNCDNDH.DBOMOJGKIFI) && !__instance.MoveLock())
			{
				string currentAnim = __instance.GetAnimDataOfVisual("main").currentAnim;
				if ((__instance.GetInputNew(global::LLHandlers.InputAction.LEFT) || __instance.GetInputNew(global::LLHandlers.InputAction.RIGHT)) && __instance.OnGround() && (__instance.moveableData.playerState == global::GameplayEntities.PlayerState.NORMAL || currentAnim == "stop" || currentAnim == "turnRun" || currentAnim == "turn" || currentAnim == "startRun"))
				{
					if (__instance.GetInputNew(global::LLHandlers.InputAction.LEFT))
					{
						global::LLHandlers.EffectHandler.instance.CreateSlowDustEffect(new global::IBGCBLLKIHA(global::HHBCPNCDNDH.FCKBPDNEAOG(__instance.GetPosition().GCPKPHMKLBN, global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(14m), global::World.FPIXEL_SIZE)), __instance.moveBox.bounds.MOGDHBGHAOA.CGJJEHPPOAN), global::GameplayEntities.Side.RIGHT);
						__instance.SetMoveDirection(global::GameplayEntities.Side.LEFT, global::IBGCBLLKIHA.DBOMOJGKIFI);
					}
					else
					{
						global::LLHandlers.EffectHandler.instance.CreateSlowDustEffect(new global::IBGCBLLKIHA(global::HHBCPNCDNDH.GAFCIOAEGKD(__instance.GetPosition().GCPKPHMKLBN, global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(46m), global::World.FPIXEL_SIZE)), __instance.moveBox.bounds.MOGDHBGHAOA.CGJJEHPPOAN), global::GameplayEntities.Side.LEFT);
						__instance.SetMoveDirection(global::GameplayEntities.Side.RIGHT, global::IBGCBLLKIHA.DBOMOJGKIFI);
					}
					__instance.SetAbilityState("START_RUN");
					__instance.moveableData.pivotBuffer = global::HHBCPNCDNDH.GANELPBAOPN(global::HHBCPNCDNDH.NPDCPLFLLIG(__instance.moveableData.pivotBuffer));
				}
				else if (global::HHBCPNCDNDH.OAHDEOGKOIM(__instance.moveableData.pivotBuffer, global::HHBCPNCDNDH.DBOMOJGKIFI))
				{
					global::GameplayEntities.MovableData movableData2 = __instance.moveableData;
					movableData2.pivotBuffer = global::HHBCPNCDNDH.FCKBPDNEAOG(movableData2.pivotBuffer, __instance.framesDuration60fps(1));
					if (global::HHBCPNCDNDH.HPLPMEAOJPM(__instance.moveableData.pivotBuffer, global::HHBCPNCDNDH.DBOMOJGKIFI))
					{
						__instance.moveableData.pivotBuffer = global::HHBCPNCDNDH.DBOMOJGKIFI;
					}
				}
			}
			if (global::HHBCPNCDNDH.HPLPMEAOJPM(__instance.moveableData.pivotBuffer, global::HHBCPNCDNDH.DBOMOJGKIFI))
			{
				if (__instance.moveableData.abilityState == "START_RUN" && !__instance.GetInput(global::LLHandlers.InputAction.LEFT) && !__instance.GetInput(global::LLHandlers.InputAction.RIGHT))
				{
					__instance.SetAbilityState("STOP");
					if (flag2 || flag3)
					{
						__instance.Deaccelerate();
					}
					__instance.moveableData.pivotBuffer = global::HHBCPNCDNDH.NPDCPLFLLIG(__instance.moveableData.pivotBuffer);
				}
				else
				{
					global::GameplayEntities.MovableData movableData3 = __instance.moveableData;
					movableData3.pivotBuffer = global::HHBCPNCDNDH.GAFCIOAEGKD(movableData3.pivotBuffer, __instance.framesDuration60fps(1));
					if (global::HHBCPNCDNDH.OAHDEOGKOIM(__instance.moveableData.pivotBuffer, global::HHBCPNCDNDH.DBOMOJGKIFI))
					{
						__instance.moveableData.pivotBuffer = global::HHBCPNCDNDH.DBOMOJGKIFI;
					}
				}
			}
			if (((!__instance.CanMove(global::GameplayEntities.Side.RIGHT) && !__instance.CanMove(global::GameplayEntities.Side.LEFT)) || (!flag2 && !flag3) || (flag2 && flag3)) && global::HHBCPNCDNDH.CJBFNLGJNIH(__instance.moveableData.airControlTimer, global::HHBCPNCDNDH.NKKIFJJEPOL(0)) && (__instance.moveableData.hitstunState == global::GameplayEntities.HitstunState.NONE || __instance.moveableData.hitstunState == global::GameplayEntities.HitstunState.PARRY_KNOCKBACK_STUN))
			{
				__instance.Deaccelerate();
				if (playerState == global::GameplayEntities.PlayerState.NORMAL && __instance.moveableData.hitstunState == global::GameplayEntities.HitstunState.NONE)
				{
					__instance.SetMoveDirection(global::GameplayEntities.Side.NONE, velocity);
				}
			}
			else if (__instance.CanMove(global::GameplayEntities.Side.RIGHT) && flag2)
			{
				if (global::HHBCPNCDNDH.CJBFNLGJNIH(__instance.moveableData.airControlTimer, global::HHBCPNCDNDH.NKKIFJJEPOL(0)))
				{
					global::HHBCPNCDNDH gcpkphmklbn2 = global::HHBCPNCDNDH.GAFCIOAEGKD(__instance.moveableData.velocity.GCPKPHMKLBN, global::HHBCPNCDNDH.AJOCFFLIIIH(gcpkphmklbn, global::World.FDELTA_TIME));
					if (global::HHBCPNCDNDH.CJBFNLGJNIH(__instance.moveableData.velocity.GCPKPHMKLBN, hhbcpncdndh) && global::HHBCPNCDNDH.OCDKNPDIPOB(gcpkphmklbn2, hhbcpncdndh))
					{
						__instance.moveableData.velocity.GCPKPHMKLBN = hhbcpncdndh;
					}
					else if (global::HHBCPNCDNDH.HPLPMEAOJPM(gcpkphmklbn2, hhbcpncdndh))
					{
						__instance.moveableData.velocity.GCPKPHMKLBN = gcpkphmklbn2;
					}
				}
				if (playerState == global::GameplayEntities.PlayerState.NORMAL && global::HHBCPNCDNDH.CJBFNLGJNIH(__instance.moveableData.airControlTimer, global::HHBCPNCDNDH.NKKIFJJEPOL(0)) && __instance.moveableData.hitstunState == global::GameplayEntities.HitstunState.NONE)
				{
					__instance.SetMoveDirection(global::GameplayEntities.Side.RIGHT, velocity);
				}
			}
			else if (__instance.CanMove(global::GameplayEntities.Side.LEFT) && flag3)
			{
				if (global::HHBCPNCDNDH.CJBFNLGJNIH(__instance.moveableData.airControlTimer, global::HHBCPNCDNDH.NKKIFJJEPOL(0)))
				{
					global::HHBCPNCDNDH gcpkphmklbn3 = global::HHBCPNCDNDH.FCKBPDNEAOG(__instance.moveableData.velocity.GCPKPHMKLBN, global::HHBCPNCDNDH.AJOCFFLIIIH(gcpkphmklbn, global::World.FDELTA_TIME));
					if (global::HHBCPNCDNDH.OCDKNPDIPOB(__instance.moveableData.velocity.GCPKPHMKLBN, global::HHBCPNCDNDH.GANELPBAOPN(hhbcpncdndh)) && global::HHBCPNCDNDH.CJBFNLGJNIH(gcpkphmklbn3, global::HHBCPNCDNDH.GANELPBAOPN(hhbcpncdndh)))
					{
						__instance.moveableData.velocity.GCPKPHMKLBN = global::HHBCPNCDNDH.GANELPBAOPN(hhbcpncdndh);
					}
					else if (global::HHBCPNCDNDH.OAHDEOGKOIM(gcpkphmklbn3, global::HHBCPNCDNDH.GANELPBAOPN(hhbcpncdndh)))
					{
						__instance.moveableData.velocity.GCPKPHMKLBN = gcpkphmklbn3;
					}
				}
				if (playerState == global::GameplayEntities.PlayerState.NORMAL && global::HHBCPNCDNDH.CJBFNLGJNIH(__instance.moveableData.airControlTimer, global::HHBCPNCDNDH.NKKIFJJEPOL(0)) && __instance.moveableData.hitstunState == global::GameplayEntities.HitstunState.NONE)
				{
					__instance.SetMoveDirection(global::GameplayEntities.Side.LEFT, velocity);
				}
			}
			if (!__instance.entityData.onLeftWall && !__instance.entityData.onRightWall && __instance.OnGround() && global::HHBCPNCDNDH.EAOICALCHJI(__instance.moveableData.velocity.GCPKPHMKLBN, global::HHBCPNCDNDH.DBOMOJGKIFI) && global::HHBCPNCDNDH.ODMJDNBPOIH(velocity.GCPKPHMKLBN, global::HHBCPNCDNDH.DBOMOJGKIFI))
			{
				__instance.effectHandler.CreateStartMoveDustEffect(new global::IBGCBLLKIHA(__instance.GetPosition().GCPKPHMKLBN, __instance.moveBox.bounds.MOGDHBGHAOA.CGJJEHPPOAN), __instance.moveableData.heading);
			}
			return false;
		}
	}

	public static class BAGMovementPatch
	{

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(BagPlayer), nameof(BagPlayer.Movement))]
		public static IEnumerable<CodeInstruction> BagPlayerMovement_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iL)
		{
			PatchUtils.LogInstructions(instructions, 0, 5);
			CodeMatcher cm = new CodeMatcher(instructions, iL);
			cm.Advance(3);

			PatchUtils.LogInstruction(cm.InstructionAt(2));
			var retLabel = cm.InstructionAt(2).operand;
			try
			{
				cm.Insert(
					new CodeInstruction(OpCodes.Ldarg_0),
					Transpilers.EmitDelegate<Action<BagPlayer>>((BagPlayer __instance) =>
					{
						if (!__instance.OnGround() && __instance.abilityData.playerState == PlayerState.NORMAL &&
							__instance.playerData.specialAmount == 0 && __instance.GetInput(InputAction.TAUNT) &&
							HHBCPNCDNDH.HPLPMEAOJPM(__instance.playerData.velocity.CGJJEHPPOAN, HHBCPNCDNDH.NKKIFJJEPOL(6)) &&
							__instance.playerData.extraJumps < 10 && HHBCPNCDNDH.CJBFNLGJNIH(__instance.playerData.reUseWallTimer, HHBCPNCDNDH.NKKIFJJEPOL(0)) &&
							(HHBCPNCDNDH.OCDKNPDIPOB(__instance.playerData.specialFAmount, HHBCPNCDNDH.NKKIFJJEPOL(0)) || __instance.playerData.hasExtraAirMove))
						{
							__instance.SetAbilityState("BAG_KITE");
							if (__instance.GetInputNew(InputAction.SWING))
							{
								if (__instance.GetInput(InputAction.DOWN))
								{
									__instance.StartAbility("downAirSwing");
								}
								else if (!__instance.GetInput(InputAction.UP) && (__instance.GetInput(InputAction.RIGHT) || __instance.GetInput(InputAction.LEFT)))
								{
									__instance.StartAbility("smash");
								}
								else
								{
									__instance.StartAbility("neutralSwing");
								}
							}
							else if (__instance.GetInputNew(InputAction.BUNT))
							{
								__instance.StartAbility("bunt");
							}
							if (__instance.GetInputNew(InputAction.GRAB))
							{
								__instance.StartAbility("grab");
							}
						}
					}),
					new CodeInstruction(OpCodes.Br, retLabel) //position
				);
			}
			catch (Exception e)
			{
				Plugin.Log.LogInfo(e);
			}
			PatchUtils.LogInstructions(cm.InstructionEnumeration(), 0, 5);
			return cm.InstructionEnumeration();
		}
	}

	public static class BAGUpdateSpecialPatch
	{

		[HarmonyTranspiler]
		[HarmonyPatch(typeof(BagPlayer), nameof(BagPlayer.UpdateSpecial))]
		public static IEnumerable<CodeInstruction> BagPlayerUpdateSpecial_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator iL)
		{
			PatchUtils.LogInstructions(instructions, 0, 5);
			CodeMatcher cm = new CodeMatcher(instructions, iL);
			cm.Advance(3);

			PatchUtils.LogInstruction(cm.InstructionAt(2));
			var retLabel = cm.InstructionAt(2).operand;
			try
			{
				cm.Insert(
					new CodeInstruction(OpCodes.Ldarg_0),
					Transpilers.EmitDelegate<Action<BagPlayer>>((BagPlayer __instance) =>
					{
						global::GameplayEntities.PlayerData playerData = __instance.playerData;
						playerData.abilityStateTimer = global::HHBCPNCDNDH.GAFCIOAEGKD(playerData.abilityStateTimer, global::World.FDELTA_TIME);
						if (__instance.playerData.abilityState == "BAG_KITE")
						{
							__instance.Movement();
							__instance.Gravity();
							global::GameplayEntities.PlayerData playerData2 = __instance.playerData;
							playerData2.specialFAmount = global::HHBCPNCDNDH.FCKBPDNEAOG(playerData2.specialFAmount, global::World.FDELTA_TIME);
							if (!__instance.OnGround() && global::HHBCPNCDNDH.OAHDEOGKOIM(__instance.playerData.specialFAmount, global::HHBCPNCDNDH.NKKIFJJEPOL(0)) && (__instance.GetInput(global::LLHandlers.InputAction.JUMP) || global::HHBCPNCDNDH.OAHDEOGKOIM(__instance.playerData.reUseWallTimer, global::HHBCPNCDNDH.NKKIFJJEPOL(0))))
							{
								if (__instance.GetInputNew(global::LLHandlers.InputAction.SWING))
								{
									if (__instance.GetInput(global::LLHandlers.InputAction.DOWN))
									{
										__instance.StartAbility("downAirSwing");
									}
									else if (!__instance.GetInput(global::LLHandlers.InputAction.UP) && (__instance.GetInput(global::LLHandlers.InputAction.RIGHT) || __instance.GetInput(global::LLHandlers.InputAction.LEFT)))
									{
										__instance.StartAbility("smash");
									}
									else
									{
										__instance.StartAbility("neutralSwing");
									}
								}
								else if (__instance.GetInputNew(global::LLHandlers.InputAction.BUNT))
								{
									__instance.StartAbility("bunt");
								}
								if (__instance.GetInputNew(global::LLHandlers.InputAction.GRAB))
								{
									__instance.StartAbility("grab");
								}
							}
							else
							{
								__instance.StopKite();
							}
						}
					}),
					new CodeInstruction(OpCodes.Br, retLabel) //position
				);
			}
			catch (Exception e)
			{
				Plugin.Log.LogInfo(e);
			}
			PatchUtils.LogInstructions(cm.InstructionEnumeration(), 0, 5);
			return cm.InstructionEnumeration();
		}
	}
}

