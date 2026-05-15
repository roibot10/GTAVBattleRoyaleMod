/*

GTA V Battle Royale Mod
(C) mrroibot

 */

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Media;
using GTA;
using GTA.Native;
using GTA.Math;
using GTA.UI;
using NativeUI;
using System.Collections.Generic;
using GTAVBattleRoyaleMod;

namespace GTAVBattleRoyaleMain {
    public class GTAVBattleRoyaleStart : Script {

        private bool isMenuScreen = false;
        private bool isGameScreen = false;
        private const string directory = "./scripts/GTAVBattleRoyaleMod/";
        private Random random = new Random();
        // MENU 
        private Camera cam1, cam2, cam3;
        private Vehicle backgroundVehicle;
        private SoundPlayer BgMusic;
        private Ped menuPed1, menuPed2, menuPed3, menuPed4;
        // UI 
        private CustomSprite playNowBtn, gameSettings;


        // SETTINGS
        private CustomSprite menuCursor;
        private int menuIndex = 0; // play, solo tpp
        private int gameModeCount = 0; // 1 - solo, 2- duo, 4 - quad
        private int gameModeStyle = 0; // tpp or fpp

        private readonly SoundPlayer button = new SoundPlayer(directory + "sounds/button.wav");

        //GAME START
        private Vehicle plane;
        private Ped planeDriver;
        private Ped[] squadPed = new Ped[110];
        private int pedIndex = 0;
        private int squadSize = 0;
        private bool isGameStart = false;
        private AITask[] task = new AITask[110];
        private AITask[] playerTask = new AITask[4];
        private AISpawner spawner;

        // SCRIPT SETTINGS
        private int tickSpeed = 5;

        // SAFEZONE
        private Blip safezoneBlip;
        private int safezoneSize = 2000;
        private Vector3 safezonePosition;
        private bool isSafezoneUpdated = false;
        private static ulong timer;

        // WEAPON DROP

        private bool isDropOnGround = false;
        private Vector3 dropZone;

        // TESTING
        private int test = 0;
        private Ped testPed, testPed2;
        private Prop testProp;
        private Vehicle testVehicle;
        private bool testPedSpawned = false;
        private AITask testTask;


        // loot
        private bool locationMarked = false;

        private Vector3 firstPoint = new Vector3();
        private Vector3 secondPoint = new Vector3();
        private Vector3 thirdPoint = new Vector3();
        private Vector3 fourthPoint = new Vector3();

        private Blip blip1, blip2, blip3, blip4;


        Scaleform scaleform = new Scaleform("mp_car_stats_10");
        public GTAVBattleRoyaleStart() {
            Tick += OnTick;
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;

            Interval = tickSpeed; // 5 
            //  UIDraw ui = Script.InstantiateScript<UIDraw>();
        }

        public void OnTick(object sender, EventArgs e) {
            //GTA.UI.Screen.ShowSubtitle((float)Function.Call<int>(Hash.GET_ENTITY_MAX_HEALTH, Game.Player.Character) + " : " + (float)Function.Call<int>(Hash.GET_ENTITY_HEALTH, Game.Player.Character));
            
            if (isMenuScreen) {
                Game.Player.DisableFiringThisFrame();
                Function.Call(Hash.HIDE_HUD_AND_RADAR_THIS_FRAME);
            }
            if (isGameScreen) {
                Function.Call(Hash.SET_PED_DENSITY_MULTIPLIER_THIS_FRAME, 0.0);
                Function.Call(Hash.SET_PED_POPULATION_BUDGET, 0);
                Function.Call(Hash.DELETE_ALL_TRAINS);
                Function.Call(Hash.SET_VEHICLE_POPULATION_BUDGET, 0);
                Function.Call(Hash.SET_ALL_VEHICLE_GENERATORS_ACTIVE_IN_AREA, -10000.0, -10000.0, -200.0, 10000.0, 10000.0, 1000.0, 0, 1);
                Function.Call(Hash.SET_ROADS_IN_AREA, -10000.0, -10000.0, -200.0, 10000.0, 10000.0, 1000.0, 0, 1);
                Function.Call(Hash.CLEAR_AREA_OF_PEDS, Game.Player.Character.Position.X, Game.Player.Character.Position.Y, Game.Player.Character.Position.Z, 1000, 1);
                Function.Call(Hash.CLEAR_AREA_OF_COPS, Game.Player.Character.Position.X, Game.Player.Character.Position.Y, Game.Player.Character.Position.Z, 2000, 0);
                Function.Call(Hash.CLEAR_AREA_OF_PROJECTILES, Game.Player.Character.Position.X, Game.Player.Character.Position.Y, Game.Player.Character.Position.Z, 2000, 0);
                World.DrawMarker(MarkerType.VerticalCylinder, safezonePosition, Vector3.Zero, Vector3.Zero, new Vector3((safezoneSize * 2) - 10, (safezoneSize * 2) - 10, 1000), Color.FromArgb(40, Color.Blue));

            }
            if (safezonePosition != null) {
                World.DrawMarker(MarkerType.VerticalCylinder, safezonePosition, Vector3.Zero, Vector3.Zero, new Vector3((safezoneSize * 2) - 10, (safezoneSize * 2) - 10, 1000), Color.FromArgb(40, Color.Blue));
            }
            if (spawner != null && !isSafezoneUpdated) {
                if (spawner.isSpawnerFinished()) {
                    GTA.UI.Notification.Show("from main class - is spawner finished " + spawner.isSpawnerFinished());
                    if (plane != null) {
                        plane.Delete();
                    }
                    if (planeDriver != null) {
                        planeDriver.Delete();
                    }
                }
            }
            
            if (isGameStart) {
                bool isPlaneInsideSafezone = World.GetDistance(new Vector3(planeDriver.Position.X, planeDriver.Position.Y, 0), safezonePosition) < safezoneSize - 2;
                if (isPlaneInsideSafezone) {
                    spawner = Script.InstantiateScript<AISpawner>();
                    spawner.SetupSquad(squadSize, planeDriver, squadPed, true);
                    plane.OpenBombBay();
                    isGameStart = false;
                }
            }
            if (spawner != null) {
                if (spawner.isSpawnerFinished()) {
                    spawner.updateSafezone(safezonePosition, safezoneSize);
                }
            }
            if (testPedSpawned) {
                /*
                float heliDistance = World.GetDistance(testPed.Position, dropZone);
                if(heliDistance < 100 && !isDropOnGround) {
                    var model = new Model("p_secret_weapon_02");
                    model.Request(250);
                    if (model.IsInCdImage && model.IsValid) {

                        testProp = World.CreateProp(model, new Vector3(dropZone.X, dropZone.Y, World.GetGroundHeight(new Vector2(dropZone.X, dropZone.Y))), false, true);
                        testProp.HasGravity = true;
                        //testPed.AttachTo(testPed);
                        testPed.LodDistance = 1000;
                        testProp.AddBlip();
                    }
                    model.MarkAsNoLongerNeeded();
                    testVehicle.Delete();
                    testPed.Delete();
                    isDropOnGround = true;
                }
                GTA.UI.Screen.ShowSubtitle(heliDistance + "");*/
                testTask.updateSafezone(safezonePosition, safezoneSize);
            }
            if (isMenuScreen && !isGameScreen) {

                //Function.Call((Hash)(0xA0EBB943C300E693), false); // DISPLAY_RADAR
                //Function.Call((Hash)(0xA6294919E56FF02A), false); // DISPLAY_HUD
                playNowBtn.Draw();
                gameSettings.Draw();
                menuCursor.Draw();
                //Function.Call((Hash)(0xAAE7CE1D63167423));
            }/*
                    if ((angle > 310 && angle < 360) || (angle > 0 && angle < 50)) {
                        GTA.UI.Screen.ShowSubtitle("player facing north");
                        y++;
                    }
                    if (angle < 310 && angle > 230) {
                        GTA.UI.Screen.ShowSubtitle("player facing east");
                        x++;
                    }
                    if (angle < 230 && angle > 130) {
                        GTA.UI.Screen.ShowSubtitle("player facing south");
                        y--;

                    }
                    if (angle > 50 && angle < 130) {
                        GTA.UI.Screen.ShowSubtitle("player facing west");
                        x--;
                    }*/
            /*
            bool isPedOnRoad = Function.Call<bool>(Hash.IS_POINT_ON_ROAD, Game.Player.Character.Position.X, Game.Player.Character.Position.Y, Game.Player.Character.Position.Z);

            if (!isPedOnRoad && !locationMarked) {
                firstPoint = Game.Player.Character.Position;
                blip1 = World.CreateBlip(firstPoint);

                float x = 0, y = 0;
                bool isBlipTwoMarked = false;
                blip2 = World.CreateBlip(firstPoint);
                while (!isBlipTwoMarked) {
                    secondPoint = Game.Player.Character.GetOffsetPosition(new Vector3(0, 0 + x++, 0));
                    blip2.Position = secondPoint;
                    if (Function.Call<bool>(Hash.IS_POINT_ON_ROAD, secondPoint.X, secondPoint.Y, secondPoint.Z)) {
                        isBlipTwoMarked = true;
                    }
                }
                float size = firstPoint.DistanceTo(secondPoint);
                float center = firstPoint.DistanceTo(secondPoint) / 2;
                float angle = Game.Player.Character.Heading;
                if (size > 50) {

                    Vector3 pointCenter = new Vector3(((firstPoint.X + secondPoint.X) / 2), ((firstPoint.Y + secondPoint.Y) / 2), World.GetGroundHeight(new Vector2(((firstPoint.X + secondPoint.X) / 2), ((firstPoint.Y + secondPoint.Y) / 2))));
                    blip3 = World.CreateBlip(pointCenter);
                    blip4 = World.CreateBlip(pointCenter);

                    Vector3 aroundCenter = pointCenter.Around(10);
                    /*
                    Function.Call(Hash.CREATE_AMBIENT_PICKUP, 0x0968339D,
                                            aroundCenter.X, aroundCenter.Y, World.GetGroundHeight(new Vector2(aroundCenter.X, aroundCenter.Y)),
                                            0, 1, 0x0968339D, false, false);
                    Function.Call(Hash.CREATE_AMBIENT_PICKUP, 0xE4BD2FC6,
                        aroundCenter.X, aroundCenter.Y, World.GetGroundHeight(new Vector2(aroundCenter.X, aroundCenter.Y)),
                        0, 60, 0xE4BD2FC6, false, false);*/
                       /*
                    bool isBlipThreeMarked = false;
                    bool isBlipFourMarked = false;
                    x = 0;
                    y = 0;
                    while (!isBlipThreeMarked) {
                        x += 1;
                        y = center;
                        thirdPoint = Game.Player.Character.GetOffsetPosition(new Vector3(x, y, World.GetGroundHeight(new Vector2(x, y))));
                        blip3.Position = thirdPoint;
                        Wait(10);
                        if (Function.Call<bool>(Hash.IS_POINT_ON_ROAD, thirdPoint.X, thirdPoint.Y, thirdPoint.Z)) {
                            isBlipThreeMarked = true;
                        }
                    }
                    while (!isBlipFourMarked) {
                        fourthPoint = new Vector3(Game.Player.Character.GetOffsetPosition(new Vector3(0 - y++, 0, 0)).X, pointCenter.Y, pointCenter.Z);
                        blip4.Position = fourthPoint;
                        Wait(10);
                        if (Function.Call<bool>(Hash.IS_POINT_ON_ROAD, fourthPoint.X, fourthPoint.Y, fourthPoint.Z)) {
                            isBlipFourMarked = true;
                        }
                    }
                    GTA.UI.Screen.ShowSubtitle("center " + center);
                }
                locationMarked = true;
            }
            if (isPedOnRoad) {
                if (blip1 != null) {
                    blip1.Delete();
                }
                if (blip2 != null) {
                    blip2.Delete();
                }
                if (blip3 != null) {
                    blip3.Delete();
                }
                if (blip4 != null) {
                    blip4.Delete();
                }
                locationMarked = false;
            }*/
        }
        public void OnKeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.H && !isMenuScreen) {
                StartMainMenu();

                /*
                safezonePosition = new Vector3(0, 0, 0);
                /*
                                random.Next(-1000, 2200),
                                random.Next(-2500, 3500),
                                12f);
                */
            }

            if (e.KeyCode == Keys.K) {
                gameModeCount = 0;
                menuIndex = 0;
                if (task != null) {
                    for (int x = 0; x < task.Length; x++) {
                        if (task[x] != null) {
                            //squadPed[x].Delete();
                            task[x].delete();
                        }
                    }
                }

                ExitMainMenuScreen();
                isGameScreen = false;
                if (plane != null) {
                    Game.Player.Character.Position = planeDriver.GetOffsetPosition(new Vector3(0, 15, -15));
                    plane.Delete();
                    planeDriver.Delete();

                }
            }

            if (e.KeyCode == Keys.J) {
                if (!testPedSpawned) {

                    testPed = World.CreatePed(PedHash.Andreas, Game.Player.Character.Position.Around(5));
                    //testVehicle = World.CreateVehicle(VehicleHash.Dune, Game.Player.Character.Position.Around(10));
                    /*Function.Call(Hash.CREATE_AMBIENT_PICKUP, 0x8F707C18,
                        Game.Player.Character.Position.Around(10).X,
                        Game.Player.Character.Position.Around(10).Y,
                        World.GetGroundHeight(new Vector2(Game.Player.Character.Position.Around(10).X, Game.Player.Character.Position.Around(10).Y)),
                        0, 1, 0x8F707C18, false, false);*/
                    //testVehicle.AddBlip();
                    //testPed.SetIntoVehicle(testVehicle, VehicleSeat.Driver);

                    testTask = Script.InstantiateScript<AITask>();
                    testTask.setPed(testPed, 5, false, false, Vector3.Zero);
                    testPedSpawned = true;
                }
            }
            //p_secret_weapon_02 spawn drop
            if (e.KeyCode == Keys.U) {
                if (testPed != null) {
                    testPed.Delete();
                    //testTask.delete();
                    //testProp.Delete();
                    //testVehicle.Delete();
                    //testPedSpawned = false;
                }

                /*
                bool isPedOnRoad = Function.Call<bool>(Hash.IS_POINT_ON_ROAD, Game.Player.Character.Position.X, Game.Player.Character.Position.Y, Game.Player.Character.Position.Z);
                bool lootChance = random.Next(1, 20) <= 5;
                if (lootChance) {
                    for (int i = 0; i < 40; i++) {
                        if (!isPedOnRoad) {
                            float x = Game.Player.Character.Position.Around(random.Next(10, 30)).X;
                            float y = Game.Player.Character.Position.Around(random.Next(10, 30)).Y;
                            float z = World.GetGroundHeight(new Vector2(x, y));
                            if (!(z > Game.Player.Character.Position.Z + 50)) {
                                bool isVectorOnRoad = Function.Call<bool>(Hash.IS_POINT_ON_ROAD, x, y, z);
                                if (!isVectorOnRoad) {
                                    Function.Call(Hash.CREATE_AMBIENT_PICKUP, 0x0968339D,
                                        x, y, z,
                                        0, 1, 0x0968339D, false, false);
                                    Function.Call(Hash.CREATE_AMBIENT_PICKUP, 0xE4BD2FC6,
                                        x, y, z,
                                        0, 60, 0xE4BD2FC6, false, false);
                                    Blip blip = World.CreateBlip(new Vector3(x, y, z));
                                    GTA.UI.Notification.Show("object spawned in alley");
                                }
                            }
                        }
                    }
                }*/
            }

            if (e.KeyCode == Keys.L) {
                safezoneSize -= 200;
                testTask.clearPedTask = true;
                //spawner.clearPedTasks();
            }
            if (e.KeyCode == Keys.U && isGameScreen) {
                safezoneSize -= 200;
                spawner.clearPedTasks();
            }
            if (e.KeyCode == Keys.L && isGameScreen) {
                GTA.UI.Notification.Show("Squad size " + squadSize);
                SetupPlayerSquad(squadSize); 
            }
            if (isMenuScreen && !isGameScreen) {
                MainMenuController(e);
            }
        }
        
        public void OnKeyUp(object sender, KeyEventArgs e) {
        }

        public void Pause() {
            if (BgMusic != null) {
                BgMusic.Stop();
            }

        }
        public void Resume() {
            if (BgMusic != null) {
                BgMusic.Play();
            }
        }
        public void safeZoneStates() {

        }
        public void StartMainMenu() {

            // START FADE OUT
            //GTA.UI.Screen.FadeOut(300);
            Wait(300);

            Function.Call(Hash.DESTROY_MOBILE_PHONE);
            
            playNowBtn = new CustomSprite(directory + "ui/play_now.png", new System.Drawing.SizeF(400f, 105f), new System.Drawing.PointF(880f, 0f));
            gameSettings = new CustomSprite(directory + "ui/tpp_solo.png", new System.Drawing.SizeF(350f, 150f), new System.Drawing.PointF(940f, 47f));
            menuCursor = new CustomSprite(directory + "ui/cursor.png", new System.Drawing.SizeF(30f, 25f), new System.Drawing.PointF(890f, 30f));
            isMenuScreen = true;
            playNowBtn.Enabled = true;
            gameSettings.Enabled = true;

            cam1 = World.CreateCamera(new Vector3(-1596.4f, 719.568f, 191.46f), new Vector3(0, 0, 180), 50);
            cam2 = World.CreateCamera(new Vector3(-1596.6f, 719.968f, 191.46f), new Vector3(0, 0, 175), 50);
            cam3 = World.CreateCamera(new Vector3(-1596.7f, 719.968f, 191.46f), new Vector3(0, 0, 173), 50);

            //GTA.UI.
            cam1.Shake(CameraShake.Hand, 0.6f);
            cam2.Shake(CameraShake.Hand, 0.7f);
            cam3.Shake(CameraShake.Hand, 0.8f);

            backgroundVehicle = World.CreateVehicle(VehicleHash.Retinue, new Vector3(-1591.84f, 714.869f, 191.769f), 80f);
            backgroundVehicle.AreBrakeLightsOn = true;
            backgroundVehicle.AreLightsOn = true;
            backgroundVehicle.AreHighBeamsOn = true;

            menuPed1 = World.CreatePed(PedHash.Bevhills02AMY, new Vector3(-1595.84f, 717.169f, 191.424f), 0f);
            menuPed1.Weapons.Give(WeaponHash.AssaultRifle, 999, true, true);
            menuPed1.BlockPermanentEvents = true;

            menuPed2 = World.CreatePed(PedHash.Hipster01AMY, new Vector3(-1596.84f, 716.169f, 191.424f), 0f);
            menuPed2.Weapons.Give(WeaponHash.Bat, 999, true, true);
            menuPed2.BlockPermanentEvents = true;

            menuPed3 = World.CreatePed(PedHash.Hipster02AFY, new Vector3(-1597.84f, 716.969f, 191.424f), 0f);
            menuPed3.Weapons.Give(WeaponHash.Revolver, 999, true, true);
            menuPed3.BlockPermanentEvents = true;

            menuPed4 = World.CreatePed(PedHash.Hipster03AMY, new Vector3(-1598.84f, 716.269f, 191.424f), 0f);
            menuPed4.Weapons.Give(WeaponHash.SawnOffShotgun, 999, true, true);
            menuPed4.BlockPermanentEvents = true;

            menuPed1.Opacity = 1000;
            menuPed2.Opacity = 0;
            menuPed3.Opacity = 0;
            menuPed4.Opacity = 0;

            menuPed2.Weapons.Give(WeaponHash.Unarmed, 999, true, true);
            menuPed3.Weapons.Give(WeaponHash.Unarmed, 999, true, true);
            menuPed4.Weapons.Give(WeaponHash.Unarmed, 999, true, true);

            // SET PLAYER SETTINGS
            Game.Player.Character.Position = new Vector3(-1592.83f, 785.779f, 189.94f);
            Game.Player.IgnoredByPolice = true;
            Game.Player.Character.BlockPermanentEvents = true;
            Function.Call(Hash.SET_ENTITY_LOD_DIST, Game.Player.Character, 10000);
            while (!Function.Call<bool>(Hash.HAS_COLLISION_LOADED_AROUND_ENTITY, Game.Player.Character)) {
                Wait(1000);
            }
            // RENDER CAMERA
            Function.Call(Hash.RENDER_SCRIPT_CAMS, 1, 1, cam1.Handle, 0, 0);

            // SET WORLD
            World.RenderingCamera = cam1;
            World.Weather = GTA.Weather.Clouds;
            World.PauseClock(true);

            Function.Call((Hash)(0x47C3B5848C3E45D8), 9, 30, 0);

            // SET MUSIC
            //BgMusic = new SoundPlayer(directory + "sounds/bg.wav");
            //BgMusic.PlayLooping();
            // END FADE IN
            //GTA.UI.Screen.FadeIn(300);
            Wait(300);
            isMenuScreen = true;
        }

        public void SetupPlane(int squadSize) {
            //Function.Call(Hash.DO_SCREEN_FADE_OUT, 300);
            Wait(1000);
            ExitMainMenuScreen();
            /*
            for (int i = 0; i < 200; i++) {
                float x = random.Next(-1000, 1200);
                float y = random.Next(-1800, 200);
                Vector3 temp = World.GetNextPositionOnStreet(new Vector2(x, y));
                Vehicle vehicleForEveryone = World.CreateVehicle(VehicleHash.Dubsta, temp);
            }*/
            Function.Call((Hash)(0x47C3B5848C3E45D8), 12, 30, 0);

            Game.Player.Character.BlockPermanentEvents = false;
            plane = World.CreateVehicle(VehicleHash.CargoPlane, new Vector3(-2264f, -1618f,
                                    600f), safezonePosition.ToHeading() - 50);
            while (plane == null) {
                Wait(50);
            }
            planeDriver = World.CreatePed(PedHash.Pilot01SMM, plane.Position.Around(10));
            while (planeDriver == null) {
                Wait(50);
            }
            planeDriver.SetIntoVehicle(plane, VehicleSeat.Driver);
            Game.Player.Character.SetIntoVehicle(plane, VehicleSeat.Passenger);
            Function.Call(Hash.TASK_PLANE_MISSION, planeDriver, plane, 0, 0, safezonePosition.X, safezonePosition.Y, 600f, 4, 100f, 0f, 90f, 0, 200f);
            Wait(7000);
            plane.Position = new Vector3(-2264f, -1618f, 600f);
            SetupSafezone();
            //Function.Call(Hash.DO_SCREEN_FADE_IN, 300);
            // SPAWN ENEMIES
            isGameStart = true;
        }

        public void SetupSafezone() {
            /* states
             * 1 - 8
             * 
             * 
             */

            /*
            safezonePosition = new Vector3(
                                random.Next(-1000, 2200),
                                random.Next(-2500, 3500),
                                12f);
            */

            safezonePosition = new Vector3(0,0,12f);
            safezoneBlip = GTA.World.CreateBlip(safezonePosition, safezoneSize);
            safezoneBlip.Alpha = 60;
            safezoneBlip.Color = BlipColor.White;


            isGameScreen = true;
        }

        public void SetupPlayerSquad(int squadSize) {
            GTA.UI.Notification.Show("set player squad");
            pedIndex = 0;
            for (int j = 0; j < squadSize; j++) {
                playerTask[j] = Script.InstantiateScript<AITask>();
                if (j == 0) {
                    squadPed[pedIndex] = World.CreatePed(PedHash.Bevhills01AMY, Game.Player.Character.GetOffsetPosition(new Vector3(0, -48, -15)));
                    playerTask[j].setPed(squadPed[pedIndex], 0, true, true, Vector3.Zero);
                }
                else {
                    squadPed[pedIndex] = World.CreatePed(PedHash.Bevhills01AMY, Game.Player.Character.Position.Around(15));
                    playerTask[j].setPed(squadPed[pedIndex], 0, false, true, Vector3.Zero);
                }
                pedIndex++;
            }
            spawner.SetupPlayerSquad(playerTask);
        }

        public void ExitMainMenuScreen() {
            isMenuScreen = false;
            // SET DEFAULT CAMERA
            Function.Call(Hash.RENDER_SCRIPT_CAMS, 0, 1, 0, 0, 0); // go back to player camera
            if(cam1 != null) {
                World.PauseClock(false);
                cam1.Delete();
                cam2.Delete();
                cam3.Delete();
                backgroundVehicle.Delete();
                menuPed1.Delete();
                menuPed2.Delete();
                menuPed3.Delete();
                menuPed4.Delete();
                Game.Player.ChangeModel(PedHash.Michael);
                Game.Player.Character.Position = new Vector3(-1592.83f, 785.779f, 189.94f);
                Game.Player.CanControlCharacter = true;
                Game.Player.Character.Weapons.Give(WeaponHash.Parachute, 0, true, false);
            }
            // SET PLAYER SETTINGS
            Game.Player.CanControlCharacter = true;
        }

        public void MainMenuController(KeyEventArgs e) {

            if (e.KeyCode == Keys.Enter && isMenuScreen) {
                if (menuIndex == 0) {
                    isMenuScreen = false;
                    isGameScreen = true;
                    if (gameModeCount == 0) {
                        SetupPlane(1);
                        squadSize = 1;
                    }
                    if (gameModeCount == 1) {
                        SetupPlane(2);
                        squadSize = 2;
                    }
                    if (gameModeCount == 2) {
                        SetupPlane(4);
                        squadSize = 4;
                    };
                }
            }
            if (e.KeyCode == Keys.Down && isMenuScreen) {
                if (menuIndex < 2) {
                    menuIndex++;
                }
                if (menuIndex == 0) {
                    menuCursor.Position = new System.Drawing.PointF(890f, 30f);
                }
                if (menuIndex == 1) {
                    menuCursor.Position = new System.Drawing.PointF(960f, 95f);
                }
                if (menuIndex == 2) {
                    menuCursor.Position = new System.Drawing.PointF(1000f, 135f);
                }
                button.Play();
            }
            if (e.KeyCode == Keys.Up && isMenuScreen) {
                if (menuIndex > 0) {
                    menuIndex--;
                }
                if (menuIndex == 0) {
                    menuCursor.Position = new System.Drawing.PointF(890f, 30f);
                }
                if (menuIndex == 1) {
                    menuCursor.Position = new System.Drawing.PointF(960f, 95f);
                }
                if (menuIndex == 2) {
                    menuCursor.Position = new System.Drawing.PointF(1000f, 135f);
                }
                button.Play();
            }
            if (e.KeyCode == Keys.Left && isMenuScreen) {
                if (menuIndex == 1) {
                    if (gameModeCount > 0) {
                        gameModeCount--;
                    }
                    if (gameModeCount == 0 && gameModeStyle == 0) {
                        gameSettings = new CustomSprite(directory + "ui/tpp_solo.png", new System.Drawing.SizeF(350f, 150f), new System.Drawing.PointF(940f, 47f));
                        cam2.InterpTo(cam1, 1000, 0, 0);
                        MenuPedView(1);
                    }
                    if (gameModeCount == 1 && gameModeStyle == 0) {
                        gameSettings = new CustomSprite(directory + "ui/tpp_duo.png", new System.Drawing.SizeF(350f, 150f), new System.Drawing.PointF(940f, 47f));
                        cam3.InterpTo(cam2, 1000, 0, 0);
                        MenuPedView(2);
                    }
                    if (gameModeCount == 2 && gameModeStyle == 0) {
                        gameSettings = new CustomSprite(directory + "ui/tpp_quad.png", new System.Drawing.SizeF(350f, 150f), new System.Drawing.PointF(940f, 47f));
                        MenuPedView(3);
                    }
                    if (gameModeCount == 0 && gameModeStyle == 1) {
                        gameSettings = new CustomSprite(directory + "ui/fpp_solo.png", new System.Drawing.SizeF(350f, 150f), new System.Drawing.PointF(940f, 47f));
                        cam2.InterpTo(cam1, 1000, 0, 0);
                        MenuPedView(1);
                    }
                    if (gameModeCount == 1 && gameModeStyle == 1) {
                        gameSettings = new CustomSprite(directory + "ui/fpp_duo.png", new System.Drawing.SizeF(350f, 150f), new System.Drawing.PointF(940f, 47f));
                        cam3.InterpTo(cam2, 1000, 0, 0);
                        MenuPedView(2);
                    }
                    if (gameModeCount == 2 && gameModeStyle == 1) {
                        gameSettings = new CustomSprite(directory + "ui/fpp_quad.png", new System.Drawing.SizeF(350f, 150f), new System.Drawing.PointF(940f, 47f));
                        MenuPedView(3);
                    }
                }
                if (menuIndex == 2) {
                    if (gameModeStyle > 0) {
                        gameModeStyle--;
                    }
                    if (gameModeCount == 0 && gameModeStyle == 0) {
                        gameSettings = new CustomSprite(directory + "ui/tpp_solo.png", new System.Drawing.SizeF(350f, 150f), new System.Drawing.PointF(940f, 47f));
                        MenuPedView(1);
                    }
                    if (gameModeCount == 1 && gameModeStyle == 0) {
                        gameSettings = new CustomSprite(directory + "ui/tpp_duo.png", new System.Drawing.SizeF(350f, 150f), new System.Drawing.PointF(940f, 47f));
                        MenuPedView(2);
                    }
                    if (gameModeCount == 2 && gameModeStyle == 0) {
                        gameSettings = new CustomSprite(directory + "ui/tpp_quad.png", new System.Drawing.SizeF(350f, 150f), new System.Drawing.PointF(940f, 47f));
                        MenuPedView(3);
                    }
                    if (gameModeCount == 0 && gameModeStyle == 1) {
                        gameSettings = new CustomSprite(directory + "ui/fpp_solo.png", new System.Drawing.SizeF(350f, 150f), new System.Drawing.PointF(940f, 47f));
                        MenuPedView(1);
                    }
                    if (gameModeCount == 1 && gameModeStyle == 1) {
                        gameSettings = new CustomSprite(directory + "ui/fpp_duo.png", new System.Drawing.SizeF(350f, 150f), new System.Drawing.PointF(940f, 47f));
                        MenuPedView(2);
                    }
                    if (gameModeCount == 2 && gameModeStyle == 1) {
                        gameSettings = new CustomSprite(directory + "ui/fpp_quad.png", new System.Drawing.SizeF(350f, 150f), new System.Drawing.PointF(940f, 47f));
                        MenuPedView(3);
                    }
                }
                button.Play();
            }
            if (e.KeyCode == Keys.Right && isMenuScreen) {
                if (menuIndex == 1) { // solo, duo, quad


                    if (gameModeCount < 2) {
                        gameModeCount++;
                    }
                    if (gameModeCount == 0 && gameModeStyle == 0) {
                        gameSettings = new CustomSprite(directory + "ui/tpp_solo.png", new System.Drawing.SizeF(350f, 150f), new System.Drawing.PointF(940f, 47f));
                        //cam2.InterpTo(cam1, 1000, 0, 0);
                        MenuPedView(1);
                    }
                    if (gameModeCount == 1 && gameModeStyle == 0) {
                        gameSettings = new CustomSprite(directory + "ui/tpp_duo.png", new System.Drawing.SizeF(350f, 150f), new System.Drawing.PointF(940f, 47f));
                        cam1.InterpTo(cam2, 1000, 0, 0);
                        MenuPedView(2);
                    }
                    if (gameModeCount == 2 && gameModeStyle == 0) {
                        gameSettings = new CustomSprite(directory + "ui/tpp_quad.png", new System.Drawing.SizeF(350f, 150f), new System.Drawing.PointF(940f, 47f));
                        cam2.InterpTo(cam3, 1000, 0, 0);
                        MenuPedView(3);
                    }
                    if (gameModeCount == 0 && gameModeStyle == 1) {
                        gameSettings = new CustomSprite(directory + "ui/fpp_solo.png", new System.Drawing.SizeF(350f, 150f), new System.Drawing.PointF(940f, 47f));
                        MenuPedView(1);
                    }
                    if (gameModeCount == 1 && gameModeStyle == 1) {
                        gameSettings = new CustomSprite(directory + "ui/fpp_duo.png", new System.Drawing.SizeF(350f, 150f), new System.Drawing.PointF(940f, 47f));
                        cam1.InterpTo(cam2, 1000, 0, 0);
                        MenuPedView(2);
                    }
                    if (gameModeCount == 2 && gameModeStyle == 1) {
                        gameSettings = new CustomSprite(directory + "ui/fpp_quad.png", new System.Drawing.SizeF(350f, 150f), new System.Drawing.PointF(940f, 47f));
                        cam2.InterpTo(cam3, 1000, 0, 0);
                        MenuPedView(3);
                    }
                }
                if (menuIndex == 2) {
                    if (gameModeStyle < 1) {
                        gameModeStyle++;
                    }
                    if (gameModeCount == 0 && gameModeStyle == 0) {
                        gameSettings = new CustomSprite(directory + "ui/tpp_solo.png", new System.Drawing.SizeF(350f, 150f), new System.Drawing.PointF(940f, 47f));
                        MenuPedView(1);
                    }
                    if (gameModeCount == 1 && gameModeStyle == 0) {
                        gameSettings = new CustomSprite(directory + "ui/tpp_duo.png", new System.Drawing.SizeF(350f, 150f), new System.Drawing.PointF(940f, 47f));
                        MenuPedView(2);
                    }
                    if (gameModeCount == 2 && gameModeStyle == 0) {
                        gameSettings = new CustomSprite(directory + "ui/tpp_quad.png", new System.Drawing.SizeF(350f, 150f), new System.Drawing.PointF(940f, 47f));
                        MenuPedView(3);
                    }
                    if (gameModeCount == 0 && gameModeStyle == 1) {
                        gameSettings = new CustomSprite(directory + "ui/fpp_solo.png", new System.Drawing.SizeF(350f, 150f), new System.Drawing.PointF(940f, 47f));
                        MenuPedView(1);
                    }
                    if (gameModeCount == 1 && gameModeStyle == 1) {
                        gameSettings = new CustomSprite(directory + "ui/fpp_duo.png", new System.Drawing.SizeF(350f, 150f), new System.Drawing.PointF(940f, 47f));
                        MenuPedView(2);
                    }
                    if (gameModeCount == 2 && gameModeStyle == 1) {
                        gameSettings = new CustomSprite(directory + "ui/fpp_quad.png", new System.Drawing.SizeF(350f, 150f), new System.Drawing.PointF(940f, 47f));
                        MenuPedView(3);
                    }
                }
                button.Play();
            }
        }

        public void MenuPedView(int index) {
            if (index == 1) {
                menuPed1.Opacity = 1000;
                menuPed2.Opacity = 0;
                menuPed3.Opacity = 0;
                menuPed4.Opacity = 0;

                menuPed2.Weapons.Give(WeaponHash.Unarmed, 999, true, true);
                menuPed3.Weapons.Give(WeaponHash.Unarmed, 999, true, true);
                menuPed4.Weapons.Give(WeaponHash.Unarmed, 999, true, true);
            }
            if (index == 2) {
                menuPed1.Opacity = 1000;
                menuPed2.Opacity = 1000;
                menuPed3.Opacity = 0;
                menuPed4.Opacity = 0;
                menuPed2.Weapons.Give(WeaponHash.Bat, 999, true, true);
                menuPed3.Weapons.Give(WeaponHash.Unarmed, 999, true, true);
                menuPed4.Weapons.Give(WeaponHash.Unarmed, 999, true, true);
            }
            if (index == 3) {
                menuPed1.Opacity = 1000;
                menuPed2.Opacity = 1000;
                menuPed3.Opacity = 1000;
                menuPed4.Opacity = 1000;
                menuPed2.Weapons.Give(WeaponHash.Bat, 999, true, true);
                menuPed3.Weapons.Give(WeaponHash.Revolver, 999, true, true);
                menuPed4.Weapons.Give(WeaponHash.SawnOffShotgun, 999, true, true);
            }
        }
    }
}