using BepInEx;
using RoR2;
using UnityEngine;
using System;
using System.Reflection;

namespace TestPlugin
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.ant2888.deathmatch", "DeathMatch", "1.0")]
    public class DeathMatch : BaseUnityPlugin
    {
        //The Awake() method is run at the very start when the game is initialized.
        public void Awake()
        {
            Logger.Log(BepInEx.Logging.LogLevel.All,"Test");
        }

        //The Update() method is run on every frame of the game.
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3)) {
                foreach (var pcmc in PlayerCharacterMasterController.instances) {
                    if (!pcmc.master.alive)
                        pcmc.master.RespawnExtraLife();
                }
            }
            bool timer_var = true;
            //This if statement checks if the player has currently pressed F2, and then proceeds into the statement:
            if (Input.GetKeyDown(KeyCode.F2))
            {
                if (timer_var) {
                    var pcmc0_master = PlayerCharacterMasterController.instances[0].master;
                    var pcmc1_master = PlayerCharacterMasterController.instances[1].master;
                    pcmc0_master.teamIndex = TeamIndex.Monster;
                    pcmc0_master.GetBody().teamComponent.teamIndex = TeamIndex.Monster;
                    pcmc1_master.teamIndex = TeamIndex.Player;
                    pcmc1_master.GetBody().teamComponent.teamIndex = TeamIndex.Player;
                } else
                {
                    var pcmc0_master = PlayerCharacterMasterController.instances[0].master;
                    var pcmc1_master = PlayerCharacterMasterController.instances[1].master;
                    pcmc1_master.teamIndex = TeamIndex.Monster;
                    pcmc1_master.GetBody().teamComponent.teamIndex = TeamIndex.Monster;
                    pcmc0_master.teamIndex = TeamIndex.Player;
                    pcmc0_master.GetBody().teamComponent.teamIndex = TeamIndex.Player;
                }
                timer_var = !timer_var;
            }
        }
    }
}