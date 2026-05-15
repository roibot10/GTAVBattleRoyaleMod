
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Media;
using GTA;
using GTA.Native;
using GTA.Math;
using GTA.UI;
using NativeUI;
using GTAVBattleRoyaleMain;

namespace GTAVBattleRoyaleMod {

    [ScriptAttributes(NoDefaultInstance = true)]
    class Safezone : Script {

        private int tickSpeed = 10;

        private Random random = new Random();

        private Blip safezoneBlip;
        private int safezoneSize = 1400;
        private Vector3 safezonePosition;
        private bool updateSafezone = false;

        private static ulong timer;

        private AISpawner spawner;

        public Safezone() {
            Tick += OnTick;

            Interval = tickSpeed; // 5 

            safezonePosition = new Vector3(
                                random.Next(-1000, 2200),
                                random.Next(-2500, 3500),
                                12f);
            safezoneBlip = GTA.World.CreateBlip(safezonePosition, safezoneSize);
            safezoneBlip.Alpha = 60;
            safezoneBlip.Color = BlipColor.Blue;
        }

        public void OnTick(object sender, EventArgs e) {
            if (updateSafezone) {
                spawner.updateSafezone(safezonePosition, safezoneSize);
            }
        }

        public void startSafezone(AISpawner spawner) {
            this.spawner = spawner;
            updateSafezone = true;
        }

        public Vector3 getSafezonePosition() {
            return safezonePosition;
        }
        public int getSafezoneSize() {
            return safezoneSize;
        }
        public bool isSafezoneUpdated() {
            return updateSafezone;
        }

                /*
                if (safezoneBlip != null) {
                    safezoneSize -= 200;
                    safezoneBlip.Delete();
                    safezoneBlip = GTA.World.CreateBlip(safezonePosition, safezoneSize);
                    safezoneBlip.Alpha = 60;
                    safezoneBlip.Color = BlipColor.Blue;
                }*/
    }
}
