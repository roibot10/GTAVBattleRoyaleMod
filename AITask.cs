
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Media;
using System.Collections.Generic;
using GTA;
using GTA.Math;
using GTA.Native;
using System.Threading.Tasks;

namespace GTAVBattleRoyaleMain {

    [ScriptAttributes(NoDefaultInstance = true)]
    public class AITask : Script {

        private int tickSpeed = 3000;
        private int tickState = 0;
        private Ped ped;
        private Random random = new Random();

        private int groupID;

        private bool isPlayersGroup = false;
        private bool isPlayer = false;
        private bool isLeader = false;
        private bool isPedDoingTask = false;
        private bool arrivedSafezone = false;
        public bool clearPedTask { get; set; }
        private Vector3 safezonePosition;
        private Vector3 parachuteTarget;
        private int safezoneSize = 0;
        private bool vehicleForEveryoneSpawned = false;
        private bool isPedEquipped = false;
        private Vehicle vehicleForEveryone;
        private RelationshipGroup gang;
        private RelationshipGroup gangEnemy;
        private RelationshipGroup player;
        //private TaskSequence sequence;
        private List<string> inventory;
        private Prop pedProp;
        private uint[] GANG_NAMES = {
            0x90C7DA60,0x11A9A7E3,0x45897C40,
            0xC26D562A,0x7972FFBD,0x783E3868,
            0x936E7EFB,0x6A3B9F86,0xB3598E9C,
            0x4325F88A,0x11DE95FC,0x8DC30DC3,
            0x0DBF2731,0x02B8FA80,0x47033600,
            0xF50B51B7,0xA882EB57,0xFC2CA767,
            0xD9D08749,0x80401068,0x49292237,
            0x5B4DC680,0x392C823E,0x270A5DFA,
            0x024F9485,0x14CAB97B,0xE3D976F3,
            0xEB47D4E0,0xB0423AA0,0x7EA26372,
            0x90C7DA60,0x11A9A7E3,0x45897C40,
            0xC26D562A,0x7972FFBD,0x783E3868,
            0x936E7EFB,0x6A3B9F86,0xB3598E9C,
            0x4325F88A,0x11DE95FC,0x8DC30DC3,
            0x0DBF2731,0x02B8FA80,0x47033600,
            0xF50B51B7,0xA882EB57,0xFC2CA767,
            0xD9D08749,0x80401068,0x49292237,
            0x5B4DC680,0x392C823E,0x270A5DFA,
            0x024F9485,0x14CAB97B,0xE3D976F3,
            0xEB47D4E0,0xB0423AA0,0x7EA26372,
        };

        public AITask() {
            Tick += OnTick;

            KeyDown += OnKeyDown;
            Interval = tickSpeed;
        }
        public void setPed(Ped ped, int groupID, bool isLeader, bool isPlayersGroup, Vector3 parachuteTarget) {
            this.ped = ped;
            this.groupID = groupID;
            this.ped.Weapons.Give(WeaponHash.Parachute, 0, true, false);
            this.ped.Armor = 100;
            this.isLeader = isLeader;
            this.isPlayersGroup = isPlayersGroup;
            this.parachuteTarget = parachuteTarget;

            gang = new RelationshipGroup(GANG_NAMES[groupID]);
            gangEnemy = new RelationshipGroup(GANG_NAMES[groupID + 1]);
            player = new RelationshipGroup(0x6F0783F5);

            if (!isPlayersGroup) {
                //pedBlip.Alpha = 0;
                ped.RelationshipGroup = gang;
                ped.AddBlip();
            }
            if (isLeader && !isPlayersGroup) {
                Function.Call(Hash.SET_PED_AS_GROUP_LEADER, this.ped, groupID);
                //pedBlip.Color = BlipColor.Red;
                //ped.AddBlip();
                
            }
            if (!isLeader && !isPlayersGroup) {
                Function.Call(Hash.SET_PED_AS_GROUP_MEMBER, this.ped, groupID);
                // pedBlip.Color = BlipColor.Red;
                //ped.AddBlip();
            }
            if (!isLeader && isPlayersGroup) {
                Function.Call(Hash.SET_PED_AS_GROUP_MEMBER, this.ped, Function.Call<int>(Hash.GET_PLAYER_GROUP, Game.Player));
                //pedBlip.Color = BlipColor.Green;
                //ped.AddBlip();
                ped.RelationshipGroup = player;
            }
            if (isLeader && isPlayersGroup) {
                Function.Call(Hash.SET_PED_AS_GROUP_LEADER, Game.Player.Character, Function.Call<int>(Hash.GET_PLAYER_GROUP, Game.Player));
                ped.RelationshipGroup = player;
                Game.Player.ChangeModel(this.ped.Model);
                Game.Player.Character.Position = this.ped.Position;
                Game.Player.Character.Armor = 100;
                Game.Player.CanControlCharacter = true;
                this.ped.Delete();
                this.ped = Game.Player.Character;
                this.ped.Weapons.RemoveAll();
                this.ped.Weapons.Give(WeaponHash.Parachute, 0, false, false);
                isPlayer = true;
            }

            GTA.UI.Notification.Show("spawn ped");
            Function.Call(Hash.SET_PED_NEVER_LEAVES_GROUP, ped, true);
            Function.Call(Hash.SET_GROUP_FORMATION, groupID, 1);
            // firing pattern 
            if (random.Next(1, 4) == 1) {
                Function.Call(Hash.SET_PED_FIRING_PATTERN, this.ped, 0xD6FF6D61);
            }
            if (random.Next(1, 4) == 2) {
                Function.Call(Hash.SET_PED_FIRING_PATTERN, this.ped, 0x1A92D7DF);
            }
            if (random.Next(1, 4) == 3) {
                Function.Call(Hash.SET_PED_FIRING_PATTERN, this.ped, 0xC6EE6B4C);
            }
            Function.Call(Hash.SET_PED_SHOOT_RATE, this.ped, random.Next(1, 1000));
            Function.Call(Hash.SET_PED_ACCURACY, this.ped, random.Next(1,100));
            Function.Call(Hash.SET_ENTITY_LOAD_COLLISION_FLAG, this.ped, true);
            Function.Call(Hash.SET_ENTITY_LOD_DIST, this.ped, 10000);
            Function.Call(Hash.SET_PED_GET_OUT_UPSIDE_DOWN_VEHICLE, this.ped, true);
            while (!Function.Call<bool>(Hash.HAS_COLLISION_LOADED_AROUND_ENTITY, this.ped)) {
                Wait(50);
            }
        }

        // fix run to safezone, 

        public void OnKeyDown(object sender, KeyEventArgs e) {
            if(e.KeyCode == Keys.I) {
                ped.Kill();
            }
        }

        public Ped getPed() {
            return ped;
        }
        private void OnTick(object sender, EventArgs e) {
            int randomTask = random.Next(1, 4);
            if (ped != null && safezonePosition != null) {

                bool isOnGround = ped.HeightAboveGround < 10;
                bool isPedInsideSafezone = World.GetDistance(new Vector3(ped.Position.X, ped.Position.Y, 0), safezonePosition) < safezoneSize - 2;
                bool isPedNearSafezone = World.GetDistance(ped.Position, safezonePosition) < safezoneSize + 100;

                Function.Call(Hash.SET_ENTITY_LOAD_COLLISION_FLAG, ped, true);
                Function.Call(Hash.SET_ENTITY_LOD_DIST, ped, 10000);
                Function.Call(Hash.REQUEST_COLLISION_AT_COORD, ped.Position.X, ped.Position.Z, ped.Position.Y);
                Function.Call(Hash.REQUEST_COLLISION_FOR_MODEL, ped.GetHashCode());
                /*
                while (!Function.Call<bool>(Hash.HAS_COLLISION_LOADED_AROUND_ENTITY, ped)) {
                    Wait(50);
                }*/
                Function.Call((Hash)0x26695EC767728D84, ped, 1);
                if (isOnGround && isLeader && !vehicleForEveryoneSpawned) {
                    float x = ped.Position.Around(60).X;
                    float y = ped.Position.Around(60).Y;
                    int randomCar = random.Next(1, 4);

                    Vector3 temp = World.GetNextPositionOnStreet(new Vector2(x, y));
                    if (randomCar == 1) {
                        vehicleForEveryone = World.CreateVehicle(VehicleHash.Crusader, temp);
                    }
                    if (randomCar == 2) {
                        vehicleForEveryone = World.CreateVehicle(VehicleHash.Retinue, temp);
                    }
                    if (randomCar == 3) {
                        vehicleForEveryone = World.CreateVehicle(VehicleHash.Dune, temp);
                    }
                    vehicleForEveryone.IsPersistent = true;
                    vehicleForEveryoneSpawned = true;
                }

                if(isOnGround && !isPedEquipped) {
                    setPedAttributes();
                    isPedEquipped = true;
                }
                if (!isPedInsideSafezone && isPedEquipped) {
                    //ped.Health -= 1;
                }
                if (!isPlayer) {
                    bool isPedNearPlayer = World.GetDistance(Game.Player.Character.Position, ped.Position) < 500;
                    GTA.UI.Notification.Show("ped task progress: " + ped.TaskSequenceProgress);
                    if (ped.HeightAboveGround <= 400 && !isOnGround && ped.IsInParachuteFreeFall) {
                        GTA.UI.Notification.Show("open parachute");
                        parachute();
                    }
                    /*
                    if (ped.IsFalling && ped.HeightAboveGround <= 200 && isPedNearPlayer) {

                        GTA.UI.Notification.Show("ped fell, will teleport");
                        ped.Position = new Vector3(ped.Position.X, ped.Position.Y, World.GetGroundHeight(new Vector2(ped.Position.X, ped.Position.Y)));*

                        parachute();
                    }*/
                    if (ped.IsFalling && ped.HeightAboveGround <= 100) {
                        GTA.UI.Notification.Show("ped fell, will teleport");
                        ped.Position = new Vector3(ped.Position.X, ped.Position.Y, World.GetGroundHeight(new Vector2(ped.Position.X, ped.Position.Y)));
                        ped.Health = 200;

                    }
                    if (ped.HeightAboveGround <= -20) {
                        GTA.UI.Notification.Show("ped fell out of bounds, will teleport to surface");
                        ped.Position = new Vector3(ped.Position.X, ped.Position.Y, World.GetGroundHeight(new Vector2(ped.Position.X, ped.Position.Y)));
                        ped.Health = 200;
                    }

                    if (isOnGround && !isPlayersGroup) {
                        if (safezonePosition != null) {
                            if (!isPedInsideSafezone && clearPedTask) {
                                GTA.UI.Notification.Show("cleared ped task " + random.Next(1, 5100));
                                Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped, 46, false);
                                ped.Task.ClearAllImmediately();
                                clearPedTask = false;
                            }
                            if (ped.TaskSequenceProgress == -1) {
                                GTA.UI.Notification.Show("trigger task " + random.Next(1, 5100));
                                if (!isPedInsideSafezone) {
                                    if (!ped.IsInVehicle() && !isPedNearSafezone) {
                                        GTA.UI.Notification.Show("far from safezone. finding vehicle" + random.Next(1, 5100));
                                        findVehicle();
                                    }
                                    if (ped.IsInVehicle() && !isPedNearSafezone) {
                                        GTA.UI.Notification.Show("ped seat index " + ped.SeatIndex + random.Next(1, 5100));
                                        driveVehicle();
                                    }

                                    if (isPedNearSafezone && !ped.IsInVehicle()) {
                                        GTA.UI.Notification.Show("near from safezone. will just run" + random.Next(1, 5100));
                                        runToSafezone();
                                        arrivedSafezone = false;
                                    }
                                }
                                if (isPedInsideSafezone) {
                                    isPedDoingTask = true;
                                    Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped, 46, true);
                                    if (!ped.IsInVehicle()) {
                                        Ped nearby = World.GetClosestPed(ped.Position, 100);
                                        //var lootBox = World.GetClosestProp();
                                        // to do 
                                        // make peds go to loot
                                        if (nearby != null) {
                                            bool isNearbyEnemy = Function.Call<int>(Hash.GET_RELATIONSHIP_BETWEEN_PEDS, ped, nearby) > 3;
                                            if (isNearbyEnemy) {
                                                Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, ped, true);
                                                Function.Call(Hash.TASK_SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, ped, true);
                                                TaskSequence sequence = new TaskSequence();
                                                sequence.AddTask.FightAgainstHatedTargets(2000f);
                                                sequence.Close();
                                                ped.Task.PerformSequence(sequence);
                                                sequence.Dispose();
                                            }
                                            else {
                                                if (isLeader) {
                                                    TaskSequence sequence = new TaskSequence();
                                                    sequence.AddTask.RunTo(ped.Position.Around(90));
                                                    sequence.Close();
                                                    ped.Task.PerformSequence(sequence);
                                                    sequence.Dispose();
                                                }
                                            }
                                        }
                                        else {
                                            if (isLeader) {
                                                TaskSequence sequence = new TaskSequence();
                                                sequence.AddTask.RunTo(ped.Position.Around(90));
                                                sequence.Close();
                                                ped.Task.PerformSequence(sequence);
                                                sequence.Dispose();
                                            }
                                        }
                                        //doRandomTask(randomTask);
                                        /*
                                        GTA.UI.Notification.Show("ped inside safezone");
                                        TaskSequence sequence = new TaskSequence();
                                        sequence.AddTask.FightAgainstHatedTargets(2000f);
                                        sequence.Close();
                                        ped.Task.PerformSequence(sequence);
                                        sequence.Dispose();*/
                                    }
                                    else {
                                        exitVehicle();
                                    }

                                }
                            }

                            if (isPedInsideSafezone && ped.TaskSequenceProgress > -1 && ped.IsInVehicle()) {
                                ped.Task.ClearAllImmediately();
                                exitVehicle();
                            }
                            if (isPedInsideSafezone && ped.TaskSequenceProgress > -1 && !ped.IsInVehicle() && !arrivedSafezone) {
                                ped.Task.ClearAllImmediately();
                                arrivedSafezone = true;
                            }
                        }
                    }
                    if (ped.IsShooting && !isPlayersGroup) {
                        //Function.Call(Hash.SET_BLIP_FADE, ped.AttachedBlip, 255, 100);
                    }
                    if (!ped.IsShooting && !isPlayersGroup) {
                        //Function.Call(Hash.SET_BLIP_FADE, ped.AttachedBlip, 255, 0);
                    }
                    if (ped.IsDead || !ped.IsAlive) {
                        killPed();
                    }
                }
                
                if (!isPlayer && !ped.IsDead) {
                    //if (ped.AttachedBlips != null) {
                        //Function.Call(Hash.SET_BLIP_COORDS, ped.AttachedBlip, true);
                    //}
                }
                
            }
        }
       
        public void killPed() {
            string killer = "";
            string killed = "";
            if(ped.Killer == Game.Player.Character) {
                killer = GameData.player_name;
                UIDraw.addKilledPlayers();
            }
            else {
                for (int x = 0; x < AISpawner.task.Length; x++) {
                    if (AISpawner.task[x] != null) {
                        if (AISpawner.task[x].getPed() == ped.Killer) {
                            killer = GameData.bot_names[x];
                        }
                    }
                }
                for (int x = 0; x < AISpawner.playerTask.Length; x++) {
                    if (AISpawner.playerTask[x] != null) {
                        if (AISpawner.playerTask[x].getPed() == ped.Killer) {
                            killer = GameData.bot_names[x];
                        }
                    }
                }
            }
            for (int x = 0; x < AISpawner.task.Length; x++) {
                if (AISpawner.task[x] != null) {
                    if (AISpawner.task[x].getPed() == ped) {
                        killed = GameData.bot_names[x];
                    }
                }
            }
            for (int x = 0; x < AISpawner.playerTask.Length; x++) {
                if (AISpawner.playerTask[x] != null) {
                    if (AISpawner.playerTask[x].getPed() == ped) {
                        killed = GameData.bot_names[x];
                    }
                }
            }
            if (killer != "" && killed != "") {
                UIDraw.updateKillFeed(killer, killed, 1);
            }
            Wait(5000);
            Function.Call(Hash.CREATE_AMBIENT_PICKUP, 0x8F707C18,
                        ped.Position.Around(1).X,
                        ped.Position.Around(1).Y,
                        World.GetGroundHeight(new Vector2(ped.Position.Around(1).X, ped.Position.Around(1).Y)),
                        0, 1, 0x8F707C18, false, false);
            Function.Call(Hash.CREATE_AMBIENT_PICKUP, 0x4BFB42D1,
                        ped.Position.Around(1).X,
                        ped.Position.Around(1).Y,
                        World.GetGroundHeight(new Vector2(ped.Position.Around(1).X, ped.Position.Around(1).Y)),
                        0, 1, 0x4BFB42D1, false, false);
            Function.Call(Hash.CREATE_AMBIENT_PICKUP, 0xF33C83B0,
                        ped.Position.Around(1).X,
                        ped.Position.Around(1).Y,
                        World.GetGroundHeight(new Vector2(ped.Position.Around(1).X, ped.Position.Around(1).Y)),
                        0, 10, 0xF33C83B0, false, false);

            var model = new Model("prop_drop_crate_01");
            model.Request(250);
            if (model.IsInCdImage && model.IsValid) {

                pedProp = World.CreateProp(model, ped.Position, false, true);
                pedProp.LodDistance = 1000;
            }
            model.MarkAsNoLongerNeeded();
            //World.ShootBullet(ped.Position, ped.Position.Around(0.2f), Game.Player.Character, new WeaponAsset(WeaponHash.Flare), 0);
            ped.Delete();
            ped = null;
            Abort();
        }
        public bool isNearbyPedGroupmate(Ped ped) {
            Ped nearbyPed = World.GetClosestPed(ped.Position, 500f);
            return nearbyPed.GetRelationshipWithPed(ped) == Relationship.Companion ||
                                      nearbyPed.GetRelationshipWithPed(ped) == Relationship.Like ||
                                      nearbyPed.GetRelationshipWithPed(ped) == Relationship.Respect;
        }
        public void setPedAttributes() {
            //sequence = new TaskSequence();
            GTA.UI.Notification.Show("group name " + GANG_NAMES[groupID]);
            GTA.UI.Notification.Show("enemy group name " + GANG_NAMES[groupID + 1]);
            
            gang.SetRelationshipBetweenGroups(gangEnemy, Relationship.Hate, true);
            gang.SetRelationshipBetweenGroups(player, Relationship.Respect, true);
            ped.Weapons.Give(WeaponHash.AdvancedRifle, 50, true, true);
            ped.Weapons.Select(WeaponHash.AdvancedRifle);
            Function.Call(Hash.SET_PED_COMBAT_MOVEMENT, ped, 2);
            Function.Call(Hash.SET_CAN_ATTACK_FRIENDLY, ped, true, false);
            Function.Call(Hash.SET_PED_ALERTNESS, ped, 3);
            Function.Call(Hash.SET_PED_HEARING_RANGE, ped, 50000);
            Function.Call(Hash.SET_PED_PATH_CAN_DROP_FROM_HEIGHT, ped, true);
            Function.Call(Hash.SET_PED_PATH_CAN_USE_LADDERS, ped, true);
            Function.Call(Hash.SET_PED_PATH_CAN_USE_CLIMBOVERS, ped, true);
            Function.Call(Hash.SET_PED_PATH_AVOID_FIRE, ped, true);
            Function.Call(Hash.SET_PED_PATH_PREFER_TO_AVOID_WATER, ped, true);

            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped, 0, true);
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped, 1, true);
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped, 2, true);
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped, 3, true);
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped, 52, true);
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped, 5, false);
            Function.Call(Hash.SET_PED_COMBAT_ATTRIBUTES, ped, 46, true);
            Function.Call(Hash.SET_PED_RAGDOLL_ON_COLLISION, ped, false);
            if (isPlayersGroup && !isPlayer) {
                ped.Position = Game.Player.Character.Position.Around(50);
                ped.AddBlip();
                Function.Call(Hash.SET_BLIP_AS_FRIENDLY, ped.AttachedBlip, true);
            }
            ped.Health = 100;
            //ped.Position = World.GetNextPositionOnSidewalk(new Vector2(ped.Position.X, ped.Position.Y));
            ped.CancelRagdoll();
        }
        public void parachute() {
            Function.Call(Hash.SET_PARACHUTE_TASK_THRUST, 200f);
            Function.Call(Hash.SET_PARACHUTE_TASK_TARGET, ped, parachuteTarget.X, parachuteTarget.Y, parachuteTarget.Z);
            Function.Call(Hash.TASK_PARACHUTE_TO_TARGET, ped, parachuteTarget.X, parachuteTarget.Y, parachuteTarget.Z);

            //TaskSequence sequence = new TaskSequence();
            //sequence.AddTask.RappelFromHelicopter();
        }
        public void findVehicle() {
            bool isPedNearPlayer = World.GetDistance(Game.Player.Character.Position, ped.Position) < 0;
            bool hasVehiclesNearby = World.GetNearbyVehicles(ped, 100f).Length >= 1;
            ped.Task.ClearAllImmediately();
            Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, ped, true);
            Function.Call(Hash.TASK_SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, ped, true);
            GTA.UI.Notification.Show("has vehicles nearby " + hasVehiclesNearby);
            if (isPedNearPlayer) {
                if (hasVehiclesNearby) {
                    Vehicle nearbyVehicle = World.GetClosestVehicle(ped.Position, 100f);
                    bool doesVehicleHaveDriver = nearbyVehicle.GetPedOnSeat(VehicleSeat.Driver) != null;
                    if (isLeader) {
                        TaskSequence sequence = new TaskSequence();
                        sequence.AddTask.RunTo(nearbyVehicle.GetOffsetPosition(new Vector3(-3, 0, 0)));
                        sequence.AddTask.EnterVehicle(nearbyVehicle, VehicleSeat.Driver);
                        Vehicle current = Function.Call<Vehicle>(Hash.GET_VEHICLE_PED_IS_USING, ped);
                        sequence.AddTask.Wait(3000);
                        GTA.UI.Notification.Show("found a vehicle, drive to safezone = " + safezonePosition);
                        sequence.Close();
                        ped.Task.PerformSequence(sequence);
                        sequence.Dispose();
                    }
                    if (!isLeader) {
                        if (doesVehicleHaveDriver) {
                            TaskSequence sequence = new TaskSequence();
                            sequence.AddTask.RunTo(nearbyVehicle.GetOffsetPosition(new Vector3(-3, 0, 0)));
                            sequence.AddTask.EnterVehicle(nearbyVehicle, VehicleSeat.Passenger);
                            sequence.Close();
                            ped.Task.PerformSequence(sequence);
                            sequence.Dispose();
                        }
                        else {
                            TaskSequence sequence = new TaskSequence();
                            sequence.AddTask.RunTo(nearbyVehicle.GetOffsetPosition(new Vector3(-3, 0, 0)));
                            sequence.AddTask.EnterVehicle(nearbyVehicle, VehicleSeat.Driver);
                            Vehicle current = Function.Call<Vehicle>(Hash.GET_VEHICLE_PED_IS_USING, ped);
                            sequence.AddTask.Wait(3000);

                            GTA.UI.Notification.Show("found a vehicle, drive to safezone = " + safezonePosition);
                            sequence.Close();
                            ped.Task.PerformSequence(sequence);
                            sequence.Dispose();
                        }
                    }
                }
                else {
                    runToSafezone();
                }
            }
            else {
                if (hasVehiclesNearby) {
                    GTA.UI.Notification.Show("far from player, vehicle nearby");
                    Vehicle nearbyVehicle = World.GetClosestVehicle(ped.Position, 100f);
                    bool doesVehicleHaveDriver = nearbyVehicle.GetPedOnSeat(VehicleSeat.Driver) != null;
                    if (isLeader && !doesVehicleHaveDriver) {
                        ped.SetIntoVehicle(nearbyVehicle, VehicleSeat.Driver);
                    }
                    if (!isLeader && doesVehicleHaveDriver) {
                        bool isDriverFriendly = Function.Call<int>(Hash.GET_RELATIONSHIP_BETWEEN_PEDS, ped, nearbyVehicle.GetPedOnSeat(VehicleSeat.Driver)) < 3;
                        if (isDriverFriendly) {
                            ped.SetIntoVehicle(nearbyVehicle, VehicleSeat.Any);
                        }
                        else {
                            chanceToLive();
                        }
                    }
                    if (!isLeader && !doesVehicleHaveDriver) {
                        ped.SetIntoVehicle(nearbyVehicle, VehicleSeat.Driver);
                    }
                    else {
                        chanceToLive();
                    }
                }
                else {
                    chanceToLive();
                }
            }
        }
        public void chanceToLive() {
            GTA.UI.Notification.Show("has no vehicles nearby");
            bool chanceToLive1 = random.Next(1, 10) < 6;
            int chance2 = random.Next(1, 10);
            bool chanceToLive2 = chance2 < 10 && chance2 >= 6;
            if (chanceToLive1) {
                Vector3 vehiclePosition = World.GetNextPositionOnStreet(new Vector2(ped.Position.X, ped.Position.Y));
                Vehicle motor = null;

                int randomCar = random.Next(1, 4);

                if (randomCar == 1) {

                    if (vehiclePosition == Vector3.Zero) {
                        motor = World.CreateVehicle(VehicleHash.Dune, ped.Position.Around(10));
                    }
                    else {
                        motor = World.CreateVehicle(VehicleHash.Dune, vehiclePosition);
                    }
                }
                if (randomCar == 2) {
                    if (vehiclePosition == Vector3.Zero) {
                        motor = World.CreateVehicle(VehicleHash.Akuma, ped.Position.Around(10));
                    }
                    else {
                        motor = World.CreateVehicle(VehicleHash.Akuma, vehiclePosition);
                    }
                }
                if (randomCar >= 3) {

                    if (vehiclePosition == Vector3.Zero) {
                        motor = World.CreateVehicle(VehicleHash.Retinue, ped.Position.Around(10));
                    }
                    else {
                        motor = World.CreateVehicle(VehicleHash.Retinue, vehiclePosition);
                    }
                }
                ped.SetIntoVehicle(motor, VehicleSeat.Driver);
            }
            if (chanceToLive2) {
                killPed();
            }
        }
        public void driveVehicle() {
            Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, ped, true);
            Function.Call(Hash.TASK_SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, ped, true);
            TaskSequence sequence = new TaskSequence();
            Vehicle current = Function.Call<Vehicle>(Hash.GET_VEHICLE_PED_IS_USING, ped);
            sequence.AddTask.Wait(3000);
            sequence.AddTask.DriveTo(current, safezonePosition.Around(safezoneSize), 0, 300f, DrivingStyle.AvoidTrafficExtremely);

            GTA.UI.Notification.Show("found a vehicle, drive to safezone = " + safezonePosition);
            sequence.Close();
            ped.Task.PerformSequence(sequence);
            sequence.Dispose();
        }
        public void runToSafezone() {
            int randomEvent = random.Next(1, 3);
            Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, ped, true);
            Function.Call(Hash.TASK_SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, ped, true);
            TaskSequence sequence = new TaskSequence();
            if (randomEvent == 1) {
                sequence.AddTask.FleeFrom(Game.Player.Character);
            }
            else {
                sequence.AddTask.ShootAt(Game.Player.Character);
            }
            sequence.Close();
            ped.Task.PerformSequence(sequence);
            sequence.Dispose();
        }
        public void exitVehicle() {
            ped.Task.ClearAllImmediately();
            Function.Call(Hash.SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, ped, false);
            Function.Call(Hash.TASK_SET_BLOCKING_OF_NON_TEMPORARY_EVENTS, ped, false);
            bool isInVehicle = Function.Call<bool>(Hash.IS_PED_IN_ANY_VEHICLE, ped, true);
            GTA.UI.Notification.Show("is inside a vehicle " + isInVehicle);
            if (isInVehicle) {
                Vehicle current = Function.Call<Vehicle>(Hash.GET_VEHICLE_PED_IS_USING, ped);
                TaskSequence sequence = new TaskSequence();
                sequence.AddTask.ParkVehicle(current, ped.Position.Around(10), 0);
                sequence.AddTask.LeaveVehicle(LeaveVehicleFlags.LeaveDoorOpen);
                sequence.Close();
                ped.Task.PerformSequence(sequence);
                sequence.Dispose();
            }
        }

        public Vector3 getSafeCoordForPed(Vector3 pos, bool onGround, int flags) {
            OutputArgument Out = new OutputArgument();
            if (Function.Call<bool>(Hash.GET_SAFE_COORD_FOR_PED, pos.X, pos.Y, pos.Z, onGround, Out, flags)) {
                return Out.GetResult<Vector3>();
            }
            else {
                return Vector3.Zero;
            }
        }
        public void updateSafezone(Vector3 safezonePosition, int safezoneSize) {
            this.safezonePosition = safezonePosition;
            this.safezoneSize = safezoneSize;

        }
        public void setPedInventory() {
        }
        public int getPedHash() {
            return ped.GetHashCode();
        }
        public void delete() {
            ped.Delete();
            if (isLeader && vehicleForEveryone!=null) {
                vehicleForEveryone.Delete();
            }
            Abort();
        }
    }
}
