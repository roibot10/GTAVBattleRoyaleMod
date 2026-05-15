using System;
using System.Linq;
using GTA;
using GTA.Native;
using GTA.Math;
using GTA.UI;
using GTAVBattleRoyaleMod;

namespace GTAVBattleRoyaleMain {

    [ScriptAttributes(NoDefaultInstance = true)]
    class AISpawner : Script {

        private int tickSpeed = 10;
        private Random random = new Random();

        // GAME
        private Ped planeDriver;
        private Ped[] squadPed;
        public static AITask[] task = new AITask[110];
        public static AITask[] playerTask = new AITask[4];
        private int pedIndex = 0;
        private int gameMaxGroups = 25;
        private int squadSize = 0;
        private int remainingPlayers = 0;
        private int safezoneSize = 1000;
        private Vector3 safezonePosition;

        private bool startSpawning = false;
        private bool isSpawnerFinishedBool = false;
        private UIDraw ui;
        public AISpawner() {
            Tick += OnTick;

            Interval = tickSpeed;
            ui = Script.InstantiateScript<UIDraw>();
        }

        private void OnTick(object sender, EventArgs e) {
            if (startSpawning) {
                for (int i = 0; i < gameMaxGroups - 1; i++) {
                    int leader = 0;

                    float pedLandingX = random.Next(-1287, 500);
                    float pedLandingY = random.Next(-1150, 300);
                    float pedLandingZ = World.GetGroundHeight(new Vector2(pedLandingX, pedLandingY));
                    Vector3 pedLanding = new Vector3(pedLandingX, pedLandingY, pedLandingZ);
                    //int group =  Function.Call<int>(Hash.CREATE_GROUP, 0);
                    for (int j = 0; j < squadSize; j++) {
                        task[pedIndex] = Script.InstantiateScript<AITask>();
                        if (j == 0) {
                            squadPed[pedIndex] = World.CreatePed(PedHash.Bevhills01AMY,planeDriver.GetOffsetPosition(new Vector3(0, -48, -15)), safezonePosition.ToHeading() + 180);
                            leader = pedIndex;
                            while(squadPed[pedIndex] == null) {
                                Wait(50);
                            }
                            task[pedIndex].setPed(squadPed[pedIndex], i, true, false, pedLanding);
                            task[pedIndex].clearPedTask = true;
                        }
                        else {
                            squadPed[pedIndex] = World.CreatePed(PedHash.Bevhills01AMY, squadPed[leader].Position.Around(40));
                            while (squadPed[pedIndex] == null) {
                                Wait(50);
                            }
                            task[pedIndex].setPed(squadPed[pedIndex], i, false, false, pedLanding);
                            task[pedIndex].clearPedTask = true;
                        }
                        Function.Call(Hash.SET_PED_RELATIONSHIP_GROUP_HASH, squadPed[pedIndex], i);
                        pedIndex++;
                    }
                    //squadGroup.Formation = Formation.Circle1;

                    Wait(random.Next(50, 3000));
                }
                startSpawning = false;
                isSpawnerFinishedBool = true;
                GTA.UI.Notification.Show("spawner finished" + startSpawning);
            }
            if (task != null) {
                for (int x = 0; x < task.Length; x++) {
                    if (task[x] != null) {
                        GTA.UI.Notification.Show("updating AI");
                        task[x].updateSafezone(safezonePosition, safezoneSize);
                    }
                }
            }
            if (isSpawnerFinishedBool) {
                for (int x = 0; x < task.Length; x++) {
                    if (task[x].getPed() == null) {
                        task[x] = null;
                    }
                }
                for (int x = 0; x < playerTask.Length; x++) {
                    if (playerTask[x].getPed() == null) {
                        playerTask[x] = null;
                    }
                }
                ui.setRemainingPlayers(task.Count(s => s != null) + playerTask.Count(s => s != null));
            }
        }

        public void SetupSquad(int squadSize, Ped planeDriver, Ped[] squadPed, bool startSpawning) {
            this.planeDriver = planeDriver;
            this.squadSize = squadSize;
            this.squadPed = squadPed;
            this.startSpawning = startSpawning;
            switch (squadSize) {
                case 1:
                    this.gameMaxGroups = 99;
                    break;
                case 2:
                    this.gameMaxGroups = 49;
                    break;
                case 4:
                    this.gameMaxGroups = 25; // 24
                    break;
            }
        }
        public void SetupPlayerSquad(AITask[] playerTasks) {
            playerTask = playerTasks;
        }
        public void spawnPlayerGroup() {
            for (int i = 0; i < gameMaxGroups - 1; i++) {
                int leader = 0;
                //int group =  Function.Call<int>(Hash.CREATE_GROUP, 0);
                for (int j = 0; j < squadSize; j++) {
                    task[pedIndex] = Script.InstantiateScript<AITask>();
                    if (j == 0) {
                        squadPed[pedIndex] = World.CreateRandomPed(planeDriver.GetOffsetPosition(new Vector3(0, -48, -15)));
                        leader = pedIndex;
                        task[pedIndex].setPed(squadPed[pedIndex], 500, true, false, Vector3.Zero);
                    }
                    else {
                        squadPed[pedIndex] = World.CreateRandomPed(squadPed[leader].Position.Around(15));
                        task[pedIndex].setPed(squadPed[pedIndex], 500, false, false, Vector3.Zero);
                    }
                    Function.Call(Hash.SET_PED_RELATIONSHIP_GROUP_HASH, squadPed[pedIndex], i);
                    pedIndex++;
                }
            }
        }
        public void updateSafezone(Vector3 safezonePosition, int safezoneSize) {
            this.safezonePosition = safezonePosition;
            this.safezoneSize = safezoneSize;

        }

        public void clearPedTasks() {
            if (task != null) {
                for (int x = 0; x < task.Length; x++) {
                    if (task[x] != null) {
                        GTA.UI.Notification.Show("updating AI");
                        task[x].clearPedTask = true;

                    }
                }
            }
        }
        public AITask[] getTask() {
            return task;
        }

        public bool isSpawnerFinished() {
            return isSpawnerFinishedBool;
        }
    }
}
