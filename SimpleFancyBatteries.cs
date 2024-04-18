/*
	Simple program to trigger timer blocks when batteries on the grid have 25%,50%,75%,95% of their max stored.
	0.0.1 	* first (crude) run and testing.
	0.0.2 	* Changing tagging to Customdata ... but it does not do that much with it
*/

// version
private const string VERSION = "0.0.1"; // Mja

// Tags
public string ScriptTag = "FBatteries"; // name of this script
public string TimerTag = "FBTimer"; // tag for the four timers
public static IMyProgrammableBlock PBBlock = null;
public static IMyTimerBlock TB25 = null; // to trigger @power > 25%
public string TB25_Tag = "@25";
public static IMyTimerBlock TB50 = null; // to trigger @power > 50%
public string TB50_Tag = "@50";
public static IMyTimerBlock TB75 = null; // to trigger @power > 75%
public string TB75_Tag = "@75";
public static IMyTimerBlock TB100 = null; // to trigger @power > actually 95%
public string TB100_Tag = "@100";
public static IMyTimerBlock TBoff = null; // triggered if power is going down ... so set off everything
public string TBoff_Tag = "off";

//special stuff
public static IMyGridTerminalSystem MyGrid;
public static IMyProgrammableBlock ThatsMe;
public static Program _prog;
bool ThatsMe_Grid(IMyTerminalBlock q) => q.IsSameConstructAs(ThatsMe);
public string EchoChars = "//"; // space gives problems, and most systems will see this as comment

// Lists
List<IMyBatteryBlock> Batteries = new List<IMyBatteryBlock>();

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
    DoScan(); // if grid changes you need to reboot
}
public void Main(string argument, UpdateType updateSource)
{
    Echo(ScriptTag + " ... Running " + VERSION + "\n");
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

    Message = "StorePower: " + (TotalCurrentStoredPower*1000).ToString("###0") + "/" + (TotalMaxStoredPower*1000).ToString("###0") +"\n";
    Message += "Input: " + (TotalCurrentInput*1000).ToString("###0") +"\n";
    Message += "Ouput; " + (TotalCurrentOutput*1000).ToString("###0") +"\n";
    BatPercentage = CalcPercent(TotalCurrentStoredPower,TotalMaxStoredPower);
    Message += "Makes: " + BatPercentage +"%\n";

    if (OldPercentage>BatPercentage) {TBoff.Trigger();} // put everything off
	if(BatPercentage < 2) {TB25.Trigger();Message += "Power Drained !!!\n";}
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
			if (TB25 == null) { TB25=TBBlocks[tidx]; SetupTimer(TB25); continue;}
			if (TB50 == null) { TB50=TBBlocks[tidx]; SetupTimer(TB50); continue; }
			if (TB75 == null) { TB75=TBBlocks[tidx]; SetupTimer(TB75); continue; }
			if (TB100 == null) { TB100=TBBlocks[tidx]; SetupTimer(TB100); continue; }
			if (TBoff == null) { TBoff=TBBlocks[tidx]; SetupTimer(TBoff); continue; }
		}
    }

    if (Timers.Count() < 5) {Message += "I need at least five timers with a tag\n" + TimerTag;return;}
    else { Message += Timers.Count().ToString() + " Timers have been setup\n if grid changes you need to reboot\n";Setupdone = true; }
    return;
}
public void SetupTimer(IMyTimerBlock blok)
{
		switch(blok) {
		case TB25:
			CheckCustomData(TB25, TB25_Tag);
			if (!DoesNameHasTag(TB25.CustomName, TimerTag)) { TB25.CustomName += TimerTag;)
			break;
		case TB50:
			CheckCustomData(TB50, TB50_Tag);
			if (!DoesNameHasTag(TB50.CustomName, TimerTag)) { TB50.CustomName += TimerTag;)
			break;
		case TB75:
			CheckCustomData(TB75, TB75_Tag);
			if (!DoesNameHasTag(TB75.CustomName, TimerTag)) { TB75.CustomName += TimerTag;)
			break;
		case TB100:
			CheckCustomData(TB100, TB100_Tag);
			if (!DoesNameHasTag(TB100.CustomName, TimerTag)) { TB100.CustomName += TimerTag;)
			break;
		case TBoff:
			CheckCustomData(TBoff, TBoff_Tag);
			if (!DoesNameHasTag(TBoff.CustomName, TimerTag)) { TBoff.CustomName += TimerTag;)
			break;
		// default:
		//	Message += "°-O Wich block ?\n";
		//	break;
		}
		return;
}
void CheckCustomData(IMyTerminalBlock _thisblock, string _Var)
{
	// FBTimer = @25,@50, ...
	private string ChckString = GetCustomDataTag(_thisblock, TimerTag);
	// not yet tagged
	if (ChckString != _Var) { _thisblock.CustomData = Echochars + TimerTag + "= " + _Var;}
	return;
}
public bool DoesNameHasTag(string Inthis, string theTag)
{
    bool Hastag = false;
    string[] _nameParts = Inthis.Split(' ');
    for (int i = 0; i < _nameParts.Length; i++)
    {
		if (_nameParts[i].ToLower().Trim() == theTag.ToLower().Trim()){Hastag = true;}	
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
public string GetCustomDataTag(IMyTerminalBlock thisBlock, string _thisTag)
{
	if (thisBlock.CustomData.Trim() == "") return "";
    string _CustomData = thisBlock.CustomData.Trim();

    string[] _cdlines = _CustomData.Split('\n');
    // for each line
    for (int cdidx = 0; cdidx < _cdlines.Length; cdidx++)
	{
		// if it does not start with // it is not mine !
		if (_cdlines[cdidx].StartsWith(EchoChars))
		{
			string _cdline = _cdlines[cdidx].Replace(EchoChars, "");
			string[] _cdwords = _cdline.Split('=');
			if (_cdwords[0].Trim() == _thisTag.Trim()) return _cdwords[1];
		}
	}
    // nothing found
	return "";
}  
