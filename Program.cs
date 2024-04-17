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
  
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Save()
        {
 
        }


        List<ITerminalAction> actions = new List<ITerminalAction>();

        public void Main(string argument, UpdateType updateSource)
        {
            printOnPB(Me, "", false);


            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType(blocks, block => block.IsSameConstructAs(Me));
            for (int i = 0; i < blocks.Count; ++i)
            {
                actions.Clear();
                Echo(blocks[i].BlockDefinition.TypeId.ToString());
                Echo(blocks[i].BlockDefinition.SubtypeName);
                Echo(blocks[i].DetailedInfo);
                blocks[i].GetActions(actions);

                string ActionString = "";
                foreach(ITerminalAction action in actions)
                {
                    ActionString += action.Name + "\n";
                }

                MyEntityComponentContainer Components = blocks[i].Components;



                printOnPB(Me, blocks[i].BlockDefinition.TypeId.ToString() + "\n", true);
                printOnPB(Me, blocks[i].BlockDefinition.SubtypeName + "\n", true);
                printOnPB(Me, blocks[i].DetailedInfo + "\n", true);
                printOnPB(Me, ActionString + "\n\n", true);
            }

        }

        public static IMyTextSurface MedrawingSurface;

        public void printOnPB(IMyProgrammableBlock thisPB, string ScreenText, bool _append)
        {
            // ThatsMe = thisPB;
            MedrawingSurface = thisPB.GetSurface(0); // the large one is 0 small one (keyboard) is 1

            MedrawingSurface.ContentType = ContentType.TEXT_AND_IMAGE;
            MedrawingSurface.WriteText(ScreenText, _append);
        }
    }
}
