using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        // version
        private const string VERSION = "0.0.1"; // Mja

        // Tags
        public string ScriptTag = "FBatteries"; // name of this script
        public string TimerTag = "FBTimer"; // tag for the four timers
        public static IMyProgrammableBlock PBBlock = null;
        public static IMyTimerBlock TB25 = null; // to trigger @power > 25%
        public static IMyTimerBlock TB50 = null; // to trigger @power > 50%
        public static IMyTimerBlock TB75 = null; // to trigger @power > 75%
        public static IMyTimerBlock TB100 = null; //actuaally 95%
        public static IMyTimerBlock TBoff = null; // triggered if something changed

        //special stuff
        public static IMyGridTerminalSystem MyGrid;
        public static IMyProgrammableBlock ThatsMe;
        public static Program _prog;
        bool ThatsMe_Grid(IMyTerminalBlock q) => q.IsSameConstructAs(ThatsMe);

        // Lists
        List<IMyBatteryBlock> Batteries = new List<IMyBatteryBlock>();
        List<IMyTimerBlock> Timers = new List<IMyTimerBlock>();

        // Messaging
        public string Message = "";
        public static IMyTextSurface MedrawingSurface;

        //Data
        float TotalCurrentStoredPower=0; 
        float TotalMaxStoredPower=0; 
 
        float TotalCurrentInput=0; 
        float TotalCurrentOutput=0;
        double BatPercentage=0;
        double OldPercentage=0; 
 
        // bools
        public bool Setupdone = false;
 
        public Program()
        {
            MyGrid = GridTerminalSystem;
            ThatsMe = Me;
            _prog = this;
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            Echo(":-> Booting\n");
 
            DoScan();
        }

        public void Main(string argument, UpdateType updateSource)
        {

            Echo(" ... Running " + VERSION + "\n");
            Echo(Message);

            if (Setupdone) {Message=""; DoLoop();}
            printOnPB(ThatsMe,Message);

        }
        
        public void DoLoop()
        {
            Message="";

            TotalCurrentStoredPower=0; 
            TotalMaxStoredPower=0; 
 
            TotalCurrentInput=0; 
            TotalCurrentOutput=0;
            BatPercentage=0; 

            // Get the combined Powers 
            for (int i = 0; i < Batteries.Count; i++)  
            {    
	            TotalCurrentStoredPower  += Batteries[i].CurrentStoredPower; 
                TotalMaxStoredPower += Batteries[i].MaxStoredPower; 
                TotalCurrentInput +=  Batteries[i].CurrentInput; 
                TotalCurrentOutput += Batteries[i].CurrentOutput; 
            }  

            Message = "StorePower: " + TotalCurrentStoredPower*1000 + "/" + TotalMaxStoredPower*1000 +"\n";
            Message += "Input: " + TotalCurrentInput*1000 +"\n";
            Message += "Ouput; " + TotalCurrentOutput*1000 +"\n";

            BatPercentage = CalcPercent(TotalCurrentStoredPower,TotalMaxStoredPower);
            Message += "Makes: " + BatPercentage +"%\n";

            if (OldPercentage>BatPercentage) {TBoff.Trigger();} // put everything off

            if(BatPercentage > 25) {TB25.Trigger();Message += "@25 Triggered\n";}
            if(BatPercentage > 50) {TB50.Trigger();Message += "@50 Triggered\n";}
            if(BatPercentage > 75) {TB75.Trigger();Message += "@75 Triggered\n";}
            if(BatPercentage > 95) {TB100.Trigger();Message += "@95/100 Triggered\n";}

            OldPercentage = BatPercentage;

            return;

        }


        public void DoScan()
        {
            // Batteries
            Batteries.Clear();
            List<IMyBatteryBlock> BatBlocks = new List<IMyBatteryBlock>();
            // MyGrid.GetBlocksOfType<IMyBatteryBlock>(BatBlocks, Bblock => Bblock.IsSameConstructAs(ThatsMe));
            MyGrid.GetBlocksOfType(BatBlocks, ThatsMe_Grid);

            for (int bidx = 0; bidx < BatBlocks.Count(); bidx++)
            {
                Batteries.Add(BatBlocks[bidx]);
            }

            if (Batteries.Count() == 0) { Message = "No batteries ? That will not do at all !\n"; return; }
            else { Message = Batteries.Count().ToString() + " batteries found\n"; }

            //TBs
            Timers.Clear();
            List<IMyTimerBlock> TBBlocks = new List<IMyTimerBlock>();
            // MyGrid.GetBlocksOfType<IMyBatteryBlock>(BatBlocks, Bblock => Bblock.IsSameConstructAs(ThatsMe));
            MyGrid.GetBlocksOfType(TBBlocks, ThatsMe_Grid);

            for (int tidx = 0; tidx < TBBlocks.Count(); tidx++)
            {
                if (DoesNameHasTag(TimerTag,TBBlocks[tidx].CustomName))
                {
                    Timers.Add(TBBlocks[tidx]);
                    if (TB25 == null) { TB25=TBBlocks[tidx]; TB25.CustomName += " @25"; continue;}
                    if (TB50 == null) { TB50=TBBlocks[tidx]; TB50.CustomName += " @50"; continue; }
                    if (TB75 == null) { TB75=TBBlocks[tidx]; TB75.CustomName += " @75"; continue; }
                    if (TB100 == null) { TB100=TBBlocks[tidx]; TB100.CustomName += " @100"; continue; }
                    if (TBoff == null) { TBoff=TBBlocks[tidx]; TBoff.CustomName += " off"; continue; }
                }
            }

            if (Timers.Count() < 5) 
            { 
                Message += "I need at least five timers with a tag\n" + TimerTag;
                return; 
            }
            else 
            { 
                Message += Timers.Count().ToString() + " Timers have been setup\n";
                Setupdone = true; 
            }
        
            return;
        }

        public bool DoesNameHasTag(string theTag, string Inthis)
        {
            bool Hastag = false;

            string[] _nameParts = Inthis.Split(' ');
            for (int i = 0; i < _nameParts.Length; i++)
            {
                if (_nameParts[i].ToLower().Trim() == theTag.ToLower().Trim())
                {
                        Hastag = true;
                }
            }

            return Hastag;
        }

        public double CalcPercent(double numerator, double denominator)
        {
            if (denominator == 0) return 0;
                double percentage = Math.Round(numerator / denominator * 100, 1);
                return percentage;
        }

        public void printOnPB(IMyProgrammableBlock thisPB, string ScreenText, int surface = 0)
        {
            MedrawingSurface = thisPB.GetSurface(surface); // the large one is 0 small one (keyboard) is 1
            MedrawingSurface.ContentType = ContentType.TEXT_AND_IMAGE;
            MedrawingSurface.WriteText("", false);
            MedrawingSurface.WriteText(ScreenText, false);
        }          
    }
}