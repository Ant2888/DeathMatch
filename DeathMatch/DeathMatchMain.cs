using BepInEx;
using MonoMod;
using RoR2;
using RoR2.UI.MainMenu;
using R2API.Utils;
using System;
using System.Reflection;
using UnityEngine;
using BepInEx.Logging;

namespace DeathMatch {

  [BepInDependency("com.bepis.r2api")]
  [BepInPlugin("com.ant2888.deathmatch", "DeathMatch", "1.0")]
  public class DeathMatch : BaseUnityPlugin {
    public static ManualLogSource logger;

    public void Awake() {
      logger = Logger;
      logger.Log(LogLevel.All, "DeathMatch V1.0 Initialized.");
      // Disable mobs
      Run.onRunStartGlobal += (r) => { 
        RoR2.Console.instance.FindConVar("director_combat_disable").SetString("1");
        logger.Log(LogLevel.Info, "Game started... Initializing TDM.");
        TeamManager.instance.OnStart();
      };
      Run.onRunDestroyGlobal += (r) => {
        logger.Log(LogLevel.Info, "Game ended... Destorying TDM session.");
        TeamManager.instance.OnDestroy();
      };
    }

    public void Update() {
      TeamManager.instance.Update();
    }
  }
}
