using BepInEx.Logging;
using DeathMatch.utils;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DeathMatch {

  class PlayerData {

    public PlayerData(TeamIndex team) {
      kills = 0;
      deaths = 0;
      time_to_revive = float.MaxValue;
      this.team = team;
    }
    public float time_to_revive { get; set; }

    public int kills { get; set; }

    public int deaths { get; set; }

    public TeamIndex team { get; private set; }
  }

  class TeamManager : UnityInterface {

    static float respawn_delay = 5; /* Delay time in seconds */

    static TeamManager() { }

    private TeamManager() {
      teams = new Dictionary<CharacterMaster, PlayerData>();
      team1 = team2 = 0;
    }

    public static TeamManager instance {
      get { return instance_; }
    }

    public void OnStart() {
      init_ = true;
      NetworkUser.onNetworkUserDiscovered += UserDiscovered;
      NetworkUser.onNetworkUserLost += UserLost;
      On.RoR2.GlobalEventManager.OnPlayerCharacterDeath += UserDied;
      On.RoR2.CharacterMaster.OnBodyStart += UserStart;
      On.RoR2.CharacterMaster.OnBodyDeath += CharacterDied;

      selection = new WeightedSelection<List<ItemIndex>>();
      selection.AddChoice(ItemCatalog.tier3ItemList, .35f);
      selection.AddChoice(ItemCatalog.tier2ItemList, .30f);
      selection.AddChoice(ItemCatalog.tier1ItemList, .25f);
      selection.AddChoice(ItemCatalog.lunarItemList, .1f);
    }

    private void CharacterDied(On.RoR2.CharacterMaster.orig_OnBodyDeath orig, CharacterMaster self) {
      orig(self);
      var pcmc = self.GetComponent<PlayerCharacterMasterController>();
      // Stop the game from ending.
      if (pcmc != null)
        self.preventGameOver = true;
    }

    private void UserDied(On.RoR2.GlobalEventManager.orig_OnPlayerCharacterDeath orig, GlobalEventManager self, 
        DamageInfo damageInfo, GameObject victim, NetworkUser victimNetworkUser) {
      var killer = damageInfo.attacker.GetComponent<CharacterBody>();
      if (killer != null && teams.ContainsKey(killer.master))
        teams[killer.master].kills++;

      var char_master = victimNetworkUser.master;
      if (!teams.ContainsKey(char_master)) return;
      teams[char_master].time_to_revive = Run.TimeStamp.now.t + respawn_delay;
      teams[char_master].deaths++;
      // Give the user something nice :)
      var availableItems = selection.Evaluate(Run.instance.treasureRng.nextNormalizedFloat);
      var item = availableItems[Run.instance.treasureRng.RangeInt(0, availableItems.Count)];
      char_master.inventory.GiveItem(item);

      Chat.SendBroadcastChat(new Chat.UserChatMessage {
        sender = victimNetworkUser.gameObject,
        text = String.Format("{0} murdered me. Grabbing [{1}] for a boost.", 
         killer.master.GetComponent<PlayerCharacterMasterController>().networkUser.userName, 
         ItemCatalog.GetItemDef(item).unlockableName)
      });
      orig(self, damageInfo, victim, victimNetworkUser);
    }

    private void UserStart(On.RoR2.CharacterMaster.orig_OnBodyStart orig, CharacterMaster self, CharacterBody body) {
      RoR2.TeamManager.instance.SetTeamLevel(TeamIndex.Monster, 1);
      RoR2.TeamManager.instance.SetTeamLevel(TeamIndex.Player, 1);
      if (self.GetComponent<PlayerCharacterMasterController>() != null)
        GenerateUserTeam(self);
      orig(self, body);
    }


    private void UserLost(NetworkUser user) {
      DeathMatch.logger.Log(LogLevel.Info, String.Format("User [{0}] disconnected.", user.userName));
      RemoveUser(user.master); 
    }
    
    private void UserDiscovered(NetworkUser user) {
      DeathMatch.logger.Log(LogLevel.Info, String.Format("User [{0}] connected.", user.userName));
      GenerateUserTeam(user.master);
    }

    private void RemoveUser(CharacterMaster user) {
      if (!teams.ContainsKey(user)) return;
      var player = teams[user];
      if (player.team == TeamIndex.Monster) team2--;
      else team1--;
      teams.Remove(user);
    }

    private void GenerateUserTeam(CharacterMaster user) {
      if (teams.ContainsKey(user)) return;
      if (team1 <= team2) {
        teams.Add(user, new PlayerData(TeamIndex.Player));
        team1++;
      } else {
        teams.Add(user, new PlayerData(TeamIndex.Monster));
        team2++;
      }
      user.teamIndex = user.GetBody().teamComponent.teamIndex = teams[user].team;
      DeathMatch.logger.Log(LogLevel.Info,
        String.Format("User {0} assigned team {1}!",
          user.GetComponent<PlayerCharacterMasterController>().networkUser.userName, user.teamIndex));
    }

    public void OnDestroy() {
      init_ = false;
      NetworkUser.onNetworkUserDiscovered -= UserDiscovered;
      NetworkUser.onNetworkUserLost -= UserLost;
      On.RoR2.GlobalEventManager.OnPlayerCharacterDeath -= UserDied;
      On.RoR2.CharacterMaster.OnBodyStart -= UserStart;
      On.RoR2.CharacterMaster.OnBodyDeath -= CharacterDied; 
      teams.Clear();
      team1 = team2 = 0;
    }
    public void Update() {
      if (!init_) return;
      // Revive loop.
      foreach (var player in teams.Keys) {
        if (!player.alive && teams[player].time_to_revive <= Run.TimeStamp.now.t) {
          DeathMatch.logger.Log(LogLevel.Info,
            String.Format("Reviving {0}!",
              player.GetComponent<PlayerCharacterMasterController>().networkUser.userName));
          teams[player].time_to_revive = float.MaxValue;
          player.RespawnExtraLife();
        } 
      }
    }

    protected static readonly TeamManager instance_ = new TeamManager();
    protected static bool init_ = false;

    private Dictionary<CharacterMaster, PlayerData> teams;
    private int team1, team2;
    private WeightedSelection<List<ItemIndex>> selection;
  }
}
