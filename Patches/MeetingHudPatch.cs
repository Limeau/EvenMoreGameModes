using HarmonyLib;
using AmongUs.GameOptions;
using UnityEngine;
using Il2CppSystem.Collections.Generic;
using System.Linq;

namespace EvenMoreGameModes
{
    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Start))]
    class MeetingHudStartPatch
    {
        public static void Postfix(MeetingHud __instance)
        {
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic)
            {
                PlayerControl.LocalPlayer.GetRole().CreateMeetingButtons(__instance);
                if (PlayerControl.LocalPlayer.CanGuessInGuesserMode())
                    Guesser.CreateGuessButtons(__instance, Options.NeutralBenignCanBeGuessed.GetBool(), Options.NeutralEvilCanBeGuessed.GetBool(), Options.NeutralKillingCanBeGuessed.GetBool(), Options.AddOnsCanBeGuessed.GetBool(), Options.CrewmateRoleCanBeGuessed.GetBool());
                else if (PlayerControl.LocalPlayer.HasAddOn(AddOns.Guesser))
                    Guesser.CreateGuessButtons(__instance, Guesser.CanGuessNeutralBenign.GetBool(), Guesser.CanGuessNeutralEvil.GetBool(), Guesser.CanGuessNeutralKilling.GetBool(), Guesser.CanGuessAddOns.GetBool(), Guesser.CanGuessCrewmateRole.GetBool());
            }
            if (!AmongUsClient.Instance.AmHost) return;

            foreach (var pva in __instance.playerStates)
            {
                if (pva == null) continue;
                PlayerControl seer = PlayerControl.LocalPlayer;
                PlayerControl target = Utils.GetPlayerById(pva.PlayerId);
                if (target == null) continue;

                pva.NameText.text = Main.LastNotifyNames[(target.PlayerId, seer.PlayerId)];
            }
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.OnDestroy))]
    class MeetingHudOnDestroyPatch
    {
        public static void Postfix()
        {
            if (!AmongUsClient.Instance.AmHost) return;
            AntiBlackout.SetIsDead(true);
            AntiBlackout.RestoreIsDead(false);
            if (RandomItemsGamemode.instance != null)
                RandomItemsGamemode.instance.CamouflageTimer = -1f;
            if (Options.EnableRandomSpawn.GetBool() && Options.TeleportAfterMeeting.GetBool())
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                    pc.RpcRandomVentTeleport();
            }
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.VotingComplete))]
    class VotingCompletePatch
    {
        public static void Postfix(MeetingHud __instance, [HarmonyArgument(0)] MeetingHud.VoterState[] states, [HarmonyArgument(1)] NetworkedPlayerInfo exiled, [HarmonyArgument(2)] bool tie)
        {
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic)
            {
                foreach (var pva in __instance.playerStates)
                {
                    if (pva.transform.FindChild("MeetingButton") != null)
                        Object.Destroy(pva.transform.FindChild("MeetingButton").gameObject);
                    if (pva.transform.FindChild("GuessButton") != null)
                        Object.Destroy(pva.transform.FindChild("GuessButton").gameObject);
                }
            }
            if (!AmongUsClient.Instance.AmHost) return;
            CustomGamemode.Instance.OnVotingComplete(__instance, states, exiled, tie);
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CastVote))]
    class CastVotePatch
    {
        public static bool Prefix(MeetingHud __instance, [HarmonyArgument(0)] byte srcPlayerId, [HarmonyArgument(1)] byte suspectPlayerId)
        {
            if (!AmongUsClient.Instance.AmHost) return true;
            var player = GameData.Instance.GetPlayerById(suspectPlayerId);
            if (player != null && (player.IsDead || player.Disconnected))
            {
                var voter = Utils.GetPlayerById(srcPlayerId);
                __instance.RpcClearVote(voter.PlayerId);
                return false;
            }
            bool canceled = !CustomGamemode.Instance.OnCastVote(__instance, srcPlayerId, suspectPlayerId);
            if (canceled)
            {
                var voter = Utils.GetPlayerById(srcPlayerId);
                __instance.RpcClearVote(voter.PlayerId);
            }
            return !canceled;
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
    class UpdateButtonsPatch
    {
        public static bool Prefix(MeetingHud __instance)
        {
            if (PlayerControl.LocalPlayer.Data.IsDead)
		    {
			    __instance.SetForegroundForDead();
		    }
			for (int i = 0; i < __instance.playerStates.Length; i++)
			{
				PlayerVoteArea playerVoteArea = __instance.playerStates[i];
				NetworkedPlayerInfo playerById = GameData.Instance.GetPlayerById(playerVoteArea.PlayerId);
				if (playerById == null)
				{
					playerVoteArea.SetDisabled();
				}
				else
				{
					bool flag = playerById.Disconnected || playerById.IsDead;
					if (flag != playerVoteArea.AmDead)
					{
						playerVoteArea.SetDead(true);
						__instance.SetDirtyBit(1U);
                        if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic)
                        {
                            PlayerControl.LocalPlayer.GetRole().CreateMeetingButtons(__instance);
                            if (PlayerControl.LocalPlayer.CanGuessInGuesserMode())
                                Guesser.CreateGuessButtons(__instance, Options.NeutralBenignCanBeGuessed.GetBool(), Options.NeutralEvilCanBeGuessed.GetBool(), Options.NeutralKillingCanBeGuessed.GetBool(), Options.AddOnsCanBeGuessed.GetBool(), Options.CrewmateRoleCanBeGuessed.GetBool());
                            else if (PlayerControl.LocalPlayer.HasAddOn(AddOns.Guesser))
                                Guesser.CreateGuessButtons(__instance, Guesser.CanGuessNeutralBenign.GetBool(), Guesser.CanGuessNeutralEvil.GetBool(), Guesser.CanGuessNeutralKilling.GetBool(), Guesser.CanGuessAddOns.GetBool(), Guesser.CanGuessCrewmateRole.GetBool());
                        }
					}
				}
			}
            return false;
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CheckForEndVoting))]
    class CheckForEndVotingPatch
    {
        public static bool Prefix(MeetingHud __instance)
        {
            if (!AmongUsClient.Instance.AmHost || CustomGamemode.Instance.Gamemode != Gamemodes.Classic) return true;
            if (__instance.playerStates.All((PlayerVoteArea ps) => ps.AmDead || ps.DidVote))
		    {
			    Dictionary<byte, int> self = __instance.CalculateVotes();
			    bool tie;
			    KeyValuePair<byte, int> max = self.MaxPair(out tie);
			    NetworkedPlayerInfo exiled = GameData.Instance.AllPlayers.ToArray().FirstOrDefault((NetworkedPlayerInfo p) => !tie && p.PlayerId == max.Key);
			    System.Collections.Generic.List<MeetingHud.VoterState> votes = new();
			    for (int i = 0; i < __instance.playerStates.Length; i++)
			    {
			    	PlayerVoteArea playerVoteArea = __instance.playerStates[i];
                    NetworkedPlayerInfo playerInfo = GameData.Instance.GetPlayerById(playerVoteArea.PlayerId);
                    int voteCount = 1;
                    if (playerInfo != null && playerInfo.GetRole() != null)
                    {
                        voteCount += playerInfo.GetRole().AdditionalVisualVotes();
                        foreach (var addOn in playerInfo.GetAddOns())
                            voteCount += addOn.AdditionalVisualVotes();
                    }
                    for (int j = 1; j <= voteCount; ++j)
                    {
                        votes.Add(new MeetingHud.VoterState
			    	    { VoterId = playerVoteArea.PlayerId, VotedForId = playerVoteArea.VotedForId });
                    }
			    }
			    __instance.RpcVotingComplete(votes.ToArray(), exiled, tie, false, byte.MaxValue);
		    }
            return false;
        }
    }

    [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CalculateVotes))]
    class CalculateVotesPatch
    {
        public static bool Prefix(MeetingHud __instance, ref Dictionary<byte, int> __result)
        {
            if (!AmongUsClient.Instance.AmHost || CustomGamemode.Instance.Gamemode != Gamemodes.Classic) return true;
            Dictionary<byte, int> dictionary = new();
		    for (int i = 0; i < __instance.playerStates.Length; i++)
		    {
			    PlayerVoteArea playerVoteArea = __instance.playerStates[i];
			    if (playerVoteArea.VotedForId != 252 && playerVoteArea.VotedForId != 255 && playerVoteArea.VotedForId != 254)
			    {
                    NetworkedPlayerInfo playerInfo = GameData.Instance.GetPlayerById(playerVoteArea.PlayerId);
                    int voteCount = 1;
                    if (playerInfo != null && playerInfo.GetRole() != null)
                    {
                        voteCount += playerInfo.GetRole().AdditionalVotes();
                        foreach (var addOn in playerInfo.GetAddOns())
                            voteCount += addOn.AdditionalVotes();
                    }
				    int num;
				    if (dictionary.TryGetValue(playerVoteArea.VotedForId, out num))
                    {
                        dictionary[playerVoteArea.VotedForId] = num + voteCount;
                    }
                    else
                    {
                        dictionary[playerVoteArea.VotedForId] = voteCount;
                    }
			    }
		    }
		    __result = dictionary;
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerVoteArea), nameof(PlayerVoteArea.SetEnabled))]
    class SetEnabledPatch
    {
        public static void Postfix(PlayerVoteArea __instance)
        {
            if (CustomGamemode.Instance.Gamemode == Gamemodes.Classic && __instance.PlayerId == 253 && MeetingHud.Instance)
            {
                PlayerControl.LocalPlayer.GetRole().CreateMeetingButtons(MeetingHud.Instance);
                if (PlayerControl.LocalPlayer.CanGuessInGuesserMode())
                    Guesser.CreateGuessButtons(MeetingHud.Instance, Options.NeutralBenignCanBeGuessed.GetBool(), Options.NeutralEvilCanBeGuessed.GetBool(), Options.NeutralKillingCanBeGuessed.GetBool(), Options.AddOnsCanBeGuessed.GetBool(), Options.CrewmateRoleCanBeGuessed.GetBool());
                else if (PlayerControl.LocalPlayer.HasAddOn(AddOns.Guesser))
                    Guesser.CreateGuessButtons(MeetingHud.Instance, Guesser.CanGuessNeutralBenign.GetBool(), Guesser.CanGuessNeutralEvil.GetBool(), Guesser.CanGuessNeutralKilling.GetBool(), Guesser.CanGuessAddOns.GetBool(), Guesser.CanGuessCrewmateRole.GetBool());
            }
        }
    }

    [HarmonyPatch(typeof(ShapeshifterMinigame), nameof(ShapeshifterMinigame.OnDisable))]
    class ShapeshifterMinigameOnDisablePatch
    {
        public static void Postfix(ShapeshifterMinigame __instance)
        {
            if (MeetingHud.Instance != null && __instance.gameObject.name == "GuessMenu")
            {
                foreach (var pva in MeetingHud.Instance.playerStates)
                    pva.transform.localPosition -= new Vector3(0f, 100f, 0f);
            }
        }
    }
}
