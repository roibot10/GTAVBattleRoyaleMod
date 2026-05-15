
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

    [ScriptAttributes(NoDefaultInstance = true)]
    class UIDraw : Script {

        private const string directory = "./scripts/GTAVBattleRoyaleMod/";

        // alive
        private ContainerElement alive = new ContainerElement(new PointF(1190, 15), new SizeF(70, 35), Color.FromArgb(200, Color.Black));
        private TextElement aliveText = new TextElement("ALIVE", new PointF(30,5), 0.5f);
        private TextElement aliveCount = new TextElement("99", new PointF(8, 3), 0.6f, Color.DarkOrange);

        // kills
        private ContainerElement killed = new ContainerElement(new PointF(1100, 15), new SizeF(73, 35), Color.FromArgb(200, Color.Black));
        private TextElement killedText = new TextElement("KILLED", new PointF(30, 5), 0.5f);
        private static TextElement killedCount = new TextElement("0", new PointF(8, 3), 0.6f, Color.Orange);
        private int tickSpeed = 1;

        // UI

        // PLAYER INFO
        private ContainerElement hideHealth = new ContainerElement(new PointF(26, 695), new SizeF(180, 10), Color.Black);
        private ContainerElement playerStats = new ContainerElement(new PointF(1010, 645), new SizeF(250, 60), Color.FromArgb(200, Color.Black));
        private TextElement playerRank = new TextElement("PRIVATE", new PointF(240, 5f), 0.26f, Color.DarkOrange);
        private TextElement playerName = new TextElement("MRROIBOT", new PointF(240, 15f), 0.55f, Color.White);
        private ContainerElement playerHealth = new ContainerElement(new PointF(10, 43), new SizeF(230, 7), Color.FromArgb(200, Color.DodgerBlue));
        private ContainerElement playerHealthBg = new ContainerElement(new PointF(10, 43), new SizeF(230, 7), Color.FromArgb(100, Color.Black));

        // AMMO
        private TextElement ammoTitle = new TextElement("AMMO", new PointF(8, 5f), 0.26f, Color.DarkOrange);
        private TextElement ammoCount = new TextElement("0", new PointF(8, 15f), 0.55f, Color.White);
        private TextElement ammoMagazine = new TextElement("0", new PointF(30, 22f), 0.35f, Color.Gray);

        // PLAYER ARMOR & INVENTORY
        private ContainerElement inventoryStatBg = new ContainerElement(new PointF(979, 645), new SizeF(28, 28), Color.FromArgb(200, Color.Black));
        private ContainerElement armorStatBg = new ContainerElement(new PointF(979, 677), new SizeF(28, 28), Color.FromArgb(200, Color.Black));
        private ContainerElement inventoryStat = new ContainerElement(new PointF(0,0), new SizeF(28, 28), Color.FromArgb(200, Color.DarkOrange));
        private ContainerElement armorStat = new ContainerElement(new PointF(0, 0), new SizeF(28, 28), Color.FromArgb(200, Color.DarkOrange));
        private CustomSprite backPackSprite = new CustomSprite(directory + "ui/backpack.png", new System.Drawing.SizeF(15f, 15f), new System.Drawing.PointF(6f, 7f));
        private CustomSprite armorSprite = new CustomSprite(directory + "ui/armor.png", new System.Drawing.SizeF(15f, 15f), new System.Drawing.PointF(6f, 7f));
        // deaths
        private static int killFeedIndex = 0;
        private static int killedByPlayer = 0;
        private static ContainerElement deaths = new ContainerElement(new PointF(1248, 80), new SizeF(0, 0));
        private static System.Timers.Timer timer = new System.Timers.Timer(8000);

        private int playerCurrentWeapon = 0;
        public UIDraw() {

            Tick += OnTick;
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            
            Interval = tickSpeed; // 5 

            // alive ui
            aliveText.Font = GTA.UI.Font.ChaletComprimeCologne;
            aliveCount.Font = GTA.UI.Font.ChaletComprimeCologne;
            playerName.Font = GTA.UI.Font.ChaletComprimeCologne;
            playerName.Alignment = Alignment.Right;

            playerRank.Font = GTA.UI.Font.ChaletComprimeCologne;
            playerRank.Alignment = Alignment.Right;

            ammoTitle.Font = GTA.UI.Font.ChaletComprimeCologne;
            ammoCount.Font = GTA.UI.Font.ChaletComprimeCologne;
            ammoMagazine.Font = GTA.UI.Font.ChaletComprimeCologne;

            ammoTitle.Alignment = Alignment.Left;
            ammoCount.Alignment = Alignment.Left;
            ammoMagazine.Alignment = Alignment.Left;

            alive.Items.Add(aliveText);
            alive.Items.Add(aliveCount);

            //killed ui
            killedText.Font = GTA.UI.Font.ChaletComprimeCologne;
            killedCount.Font = GTA.UI.Font.ChaletComprimeCologne;
            killed.Items.Add(killedText);
            killed.Items.Add(killedCount);

            //deaths

            //player ui
            playerStats.Items.Add(playerHealthBg);
            playerStats.Items.Add(playerHealth);
            playerStats.Items.Add(playerRank);
            playerStats.Items.Add(playerName);
            playerStats.Items.Add(ammoTitle);
            playerStats.Items.Add(ammoCount);
            playerStats.Items.Add(ammoMagazine);

            armorStatBg.Items.Add(armorStat);
            armorStatBg.Items.Add(armorSprite);
            inventoryStatBg.Items.Add(inventoryStat);
            inventoryStatBg.Items.Add(backPackSprite);
        }

        public void OnTick(object sender, EventArgs e) {
            Function.Call(Hash.DISPLAY_AMMO_THIS_FRAME, false); // HIDE ammo overlapping with alive
            Function.Call(Hash.DISPLAY_CASH, false); // HIDE ammo overlapping with alive
            Function.Call(Hash._HIDE_AREA_AND_VEHICLE_NAME_THIS_FRAME);
            alive.ScaledDraw();
            killed.ScaledDraw();
            deaths.ScaledDraw();
            playerStats.ScaledDraw();
            //hideHealth.ScaledDraw();
            armorStatBg.ScaledDraw();
            inventoryStatBg.ScaledDraw();
            setPlayerStats();
        }
        public void setRemainingPlayers(int count) {
            aliveCount.Caption = count + "";
        }
        public static void addKilledPlayers() {
            killedCount.Caption = "" + ++killedByPlayer;
        }
        public void OnKeyDown(object sender, KeyEventArgs e) {
            if(e.KeyCode == Keys.Y) {
                ///updateKillFeed("mrroibot", "teredel", 1);
                Game.Player.Character.Health -= 10;
                //Function.Call(Hash.SET_ENTITY_MAX_HEALTH, Game.Player.Character, 150);
            }
        }

        public void setPlayerStats() {


            float gamePlayerHealth = (float)Function.Call<int>(Hash.GET_ENTITY_HEALTH, Game.Player.Character) - 100;
            float gamePlayerMaxHealth = (float)Function.Call<int>(Hash.GET_ENTITY_MAX_HEALTH, Game.Player.Character) - 100;
            float healthPercent = (gamePlayerHealth / gamePlayerMaxHealth) * 230;
            if(healthPercent >= 100) {
                playerHealth.Size = new SizeF(healthPercent, 7);
            }
            if(gamePlayerHealth > gamePlayerMaxHealth) {
                playerHealth.Size = new SizeF(230, 7);
            }
            float gamePlayerArmor = (float)Function.Call<int>(Hash.GET_PED_ARMOUR, Game.Player.Character);
            float armorPercent = gamePlayerArmor * 0.28f;
            armorStat.Size = new SizeF(28, armorPercent);
            armorStat.Position = new PointF(0,28 - armorPercent);


            int playerCurrentWeapon = 0;
            int currentWeaponAmmo = 0;
            OutputArgument Out = new OutputArgument();
            if (Function.Call<bool>(Hash.GET_CURRENT_PED_WEAPON, Game.Player.Character, Out, true)) {
                playerCurrentWeapon = Out.GetResult<int>();
            }
            int weaponClipSize = Function.Call<int>(Hash.GET_WEAPON_CLIP_SIZE, playerCurrentWeapon);
            int allAmmoCount = Function.Call<int>(Hash.GET_AMMO_IN_PED_WEAPON, Game.Player.Character, playerCurrentWeapon);

            OutputArgument Out2 = new OutputArgument();
            if (Function.Call<bool>(Hash.GET_AMMO_IN_CLIP, Game.Player.Character, playerCurrentWeapon, Out2)) {
                currentWeaponAmmo = Out2.GetResult<int>();
            }
            ammoCount.Caption = currentWeaponAmmo + "";
            if(!(allAmmoCount == 0  && weaponClipSize == 0)) {
                if(currentWeaponAmmo == weaponClipSize)
                    ammoMagazine.Caption = allAmmoCount / weaponClipSize + "";
            }
            else {
                ammoCount.Caption = "0";
                ammoMagazine.Caption = "0";
            }
        }
                        
        public static void updateKillFeed(string killer, string killed, int killSource) {
            if (killFeedIndex >= 8) {
                killFeedIndex = 0;
                deaths.Items.Clear();
            }
            // create new instance of a timer 
            timer.Interval = 8000;
            timer.Start(); // start the timer 
            timer.Elapsed += (Object source, System.Timers.ElapsedEventArgs e) => {

                killFeedIndex = 0;
                deaths.Items.Clear();
                timer.Stop();
            };
            deaths.Items.Add(addKillFeed(killer, killed, killSource, killFeedIndex)); ;
            killFeedIndex++;

            
        }
        public static ContainerElement addKillFeed(string killer, string killed, int killSource, int index) {
            ContainerElement wrapper = new ContainerElement(new PointF(0, index * 30), new SizeF());
            TextElement killerText = new TextElement(killer, new PointF(0, 0), 0.5f);
            TextElement killedText = new TextElement(killed, new PointF(0, 0), 0.5f);

            int text1Width = (killer.Length * 5) + 50;

            CustomSprite killSourceImg = new CustomSprite(directory + "/weapons/AdvancedRifle.png", new System.Drawing.SizeF(35f, 10f), new System.Drawing.PointF(-(text1Width),7));

            killerText.Alignment = Alignment.Right;
            killedText.Position = new PointF(-(text1Width + 10),0);
            killedText.Alignment = Alignment.Right;

            killerText.Font = GTA.UI.Font.ChaletComprimeCologne;
            killedText.Font = GTA.UI.Font.ChaletComprimeCologne;
            wrapper.Items.Add(killedText);
            wrapper.Items.Add(killSourceImg);
            wrapper.Items.Add(killerText);
            return wrapper;
        }
        public void OnKeyUp(object sender, KeyEventArgs e) {
        }

    }
}
