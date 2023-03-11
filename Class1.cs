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


namespace wallacepatch
{
	[BepInPlugin("us.wallace.plugins.llb.wallacepatch", "wallacepatch Plug-In", "1.0.0.1")]
	public class Plugin : BaseUnityPlugin
	{
		private void Awake()
		{
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
			//harmony.PatchAll(typeof(GrafPlayerSetEntityBoxesPatch)); //crashes game
			harmony.PatchAll(typeof(GrafSetFrontHitboxesAndParryBoxesPatch));
			harmony.PatchAll(typeof(KidPlayerSetEntityBoxesPatch));
			harmony.PatchAll(typeof(PongPlayerSetEntityBoxesPatch));
			harmony.PatchAll(typeof(RobotPlayerSetEntityBoxesPatch));
			harmony.PatchAll(typeof(SkatePlayerSetEntityBoxesPatch));
			//harmony.PatchAll(typeof(BagCheckActivationPatch)); //lags game
			//harmony.PatchAll(typeof(PongCheckActivationPatch)); //lags game
			//harmony.PatchAll(typeof(BALLSetEntityValuesPatch)); //crashes game
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
			if (flag && (__instance.MatchPowerupIs(Powerup.SHOCK, PowerupPhase.ANY) || ballEntity.ballData.ballState == BallState.BUBBLEBALL) && hitEntity.GetLastPlayerHitter() != __instance && __instance.character != global::Character.DUMMY)
			{
				ballEntity.StartHitstun(__instance.parryHitstunDuration, (ballEntity.ballData.ballState != BallState.BUBBLEBALL) ? HitstunState.WALL_STUN : HitstunState.BUBBLE_WALL_STUN);
				ballEntity.DeflectClashPlayer((PlayerEntity)__instance, boxName);
				return true;
			}
			if (!__instance.MatchPowerupIs(Powerup.PHANTOM, PowerupPhase.ANY) && flag)
			{
				global::IBGCBLLKIHA position = hitEntity.GetPosition();
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
				__instance.PlaySfx(Sfx.KILL);
				global::GameCamera.instance.StartShake(global::HHBCPNCDNDH.NKKIFJJEPOL(0.67m), global::HHBCPNCDNDH.NKKIFJJEPOL(0.4m));
				__instance.effectHandler.CreateHitBallBehindEffect(ballEntity.GetPosition());
				ballEntity.CreateDeadBallEffect();
				ballEntity.SetBallState(BallState.DEAD, HitstunState.NONE);
				return true;
			}
			__instance.AddTrackedHitEntityID(hitEntity.entityID);
			__instance.RallyEvent(hitEntity);
			hitEntity.HitByTeam(__instance.attackingData.team);
			int lastHitterIndex = hitEntity.hitableData.lastHitterIndex;
			hitEntity.hitableData.lastHitterIndex = __instance.playerIndex;
			if (flag && __instance.MatchPowerupIs(Powerup.HEAL, PowerupPhase.ANY))
			{
				__instance.abilityData.hitSomething = true;
				__instance.GiveBall(BallState.STICK_TO_PLAYER_POWERUP, ballEntity, false);
				__instance.playerHandler.GetPlayerEntity(lastHitterIndex).playerData.powerup = Powerup.NONE;
				__instance.abilityData.powerup = Powerup.HEAL;
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
				global::DCKIMOBPGBF ai = AIHandler.instance.GetAI(__instance.playerIndex);
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
		public static bool SetAbilityState_Prefix(GetUpGrabAbility __instance, string state)

		{
			if (state == "GET_UP_GRAB_PRE")
			{
				__instance.data.canBeHitByBall = false;

			}
			if (state == "GET_UP_GRAB_DURING")
			{
				__instance.data.inGetUpBlaze = true;
				__instance.data.canBeHitByBall = false;
				global::LLHandlers.EffectHandler.instance.CreateGetUpBlazeEffect(__instance.entity.GetPosition(), __instance.entity.GetTeam());
				__instance.entity.PlaySfx(global::LLHandlers.Sfx.GETUPGRAB); global::LLHandlers.EffectHandler.instance.CreateGetUpBlazeEffect(__instance.entity.GetPosition(), __instance.entity.GetTeam());
				__instance.entity.PlaySfx(global::LLHandlers.Sfx.GETUPGRAB);
			}
			else
			{
				__instance.data.inGetUpBlaze = false;
				__instance.data.canBeHitByBall = true;
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

	class GrafPlayerSetEntityBoxesPatch
	{
		[HarmonyPatch(typeof(GrafPlayer), nameof(GrafPlayer.SetEntityBoxes))]
		[HarmonyPrefix]
		public static bool SetEntityBoxes_Prefix(GrafPlayer __instance)
		{
			global::HHBCPNCDNDH fpixel_SIZE = global::World.FPIXEL_SIZE;
			__instance.moveBox = __instance.AddBox(new global::GameplayEntities.Box(__instance.GetPosition(), global::IBGCBLLKIHA.DBOMOJGKIFI, global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(60, __instance.pxHeight), fpixel_SIZE), global::GameplayEntities.BoxType.MOVEBOX, false));
			__instance.moveBox.active = true;
			__instance.CreateHurtbox("NORMAL_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(60, __instance.pxHeight), fpixel_SIZE), global::IBGCBLLKIHA.DBOMOJGKIFI);
			__instance.CreateHurtbox("HALF_CROUCH_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(60, 96), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(21)), fpixel_SIZE));
			__instance.CreateHurtbox("CROUCH_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(80, 64), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.DEKDADEGAIK, global::HHBCPNCDNDH.NKKIFJJEPOL(37)), fpixel_SIZE));
			
			__instance.graffitiBox = __instance.CreateHitbox("GRAFFITI_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(150, 78), fpixel_SIZE), global::IBGCBLLKIHA.DBOMOJGKIFI, global::Abilities.AbilityState.NO_ABILITYSTATE, false, default(global::HHBCPNCDNDH), string.Empty);
			__instance.graffitiBox.parentless = true;
			__instance.graffitiBox.checkByHand = true;
			__instance.graffitiBox.cantClash = true;
			__instance.SetNormalHitboxes(new global::IBGCBLLKIHA(120, __instance.pxHeight), new global::IBGCBLLKIHA(55, 0), new global::IBGCBLLKIHA(110, 100), new global::IBGCBLLKIHA(0, 88));
			__instance.SetEntityBoxes();

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
			__instance.CreateHitbox("LANDING_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(sizeSwing, fpixel_SIZE), new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(offsetSwing.GCPKPHMKLBN, fpixel_SIZE), global::HHBCPNCDNDH.AJOCFFLIIIH(offsetSwing.CGJJEHPPOAN, fpixel_SIZE)), "SMASH_FRONT_HIT", true, default(global::HHBCPNCDNDH), string.Empty);
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
			__instance.CreateHurtbox("CROUCH_HURTBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(new global::IBGCBLLKIHA(80, 74), fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.AJOCFFLIIIH(global::IBGCBLLKIHA.GGFFJDILCDA, global::HHBCPNCDNDH.NKKIFJJEPOL(-38)), fpixel_SIZE));
			global::IBGCBLLKIHA sizeSwing = new global::IBGCBLLKIHA(124, 148);
			global::IBGCBLLKIHA offsetSwing = new global::IBGCBLLKIHA(57, 0);
			global::IBGCBLLKIHA acihfibjnkm = new global::IBGCBLLKIHA(110, 64);
			global::IBGCBLLKIHA acihfibjnkm2 = new global::IBGCBLLKIHA(0, 106);
			__instance.SetFrontHitboxesAndParryBoxes(sizeSwing, offsetSwing);
			__instance.CreateHitbox("SMASH_TOP_HITBOX", global::IBGCBLLKIHA.AJOCFFLIIIH(acihfibjnkm, fpixel_SIZE), global::IBGCBLLKIHA.AJOCFFLIIIH(acihfibjnkm2, fpixel_SIZE), "SMASH_OVERHEAD_HIT", true, default(global::HHBCPNCDNDH), string.Empty);
			__instance.CreateHitbox("LANDING_HURTBOX", new global::IBGCBLLKIHA(global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.AJOCFFLIIIH(acihfibjnkm.GCPKPHMKLBN, fpixel_SIZE), global::HHBCPNCDNDH.NKKIFJJEPOL(1.1m)), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(170), fpixel_SIZE)), new global::IBGCBLLKIHA(global::HHBCPNCDNDH.NKKIFJJEPOL(0), global::HHBCPNCDNDH.AJOCFFLIIIH(global::HHBCPNCDNDH.NKKIFJJEPOL(-95), fpixel_SIZE)), "DOWN_AIR_HIT", false, default(global::HHBCPNCDNDH), string.Empty);
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
		public static bool CheckActivation_Prefix(PongAbility __instance, bool newPress = true)
		{
			return __instance.CheckActivation(newPress);

		}
	}

	class BagCheckActivationPatch
	{
		[HarmonyPatch(typeof(ShadowAbility), nameof(ShadowAbility.CheckActivation))]
		[HarmonyPrefix]
		public static bool CheckActivation_Prefix(ShadowAbility __instance, bool newPress = true)
		{
			return __instance.CheckActivation(newPress);

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
}
