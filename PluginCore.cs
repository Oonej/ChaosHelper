using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;

using Decal.Adapter;
using Decal.Adapter.Wrappers;
using MyClasses.MetaViewWrappers;
using VirindiViewService.Controls;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Text.RegularExpressions;

using ChaosHelper.VirindiControlExtensions;

/*
 * Created by Mag-nus. 8/19/2011, VVS added by Virindi-Inquisitor.
 * 
 * No license applied, feel free to use as you wish. H4CK TH3 PL4N3T? TR45H1NG 0UR R1GHT5? Y0U D3C1D3!
 * 
 * Notice how I use try/catch on every function that is called or raised by decal (by base events or user initiated events like buttons, etc...).
 * This is very important. Don't crash out your users!
 * 
 * In 2.9.6.4+ Host and Core both have Actions objects in them. They are essentially the same thing.
 * You sould use Host.Actions though so that your code compiles against 2.9.6.0 (even though I reference 2.9.6.5 in this project)
 * 
 * If you add this plugin to decal and then also create another plugin off of this sample, you will need to change the guid in
 * Properties/AssemblyInfo.cs to have both plugins in decal at the same time.
 * 
 * If you have issues compiling, remove the Decal.Adapater and VirindiViewService references and add the ones you have locally.
 * Decal.Adapter should be in C:\Games\Decal 3.0\
 * VirindiViewService should be in C:\Games\VirindiPlugins\VirindiViewService\
*/

namespace ChaosHelper
{
    //Attaches events from core
	[WireUpBaseEvents]

	// FriendlyName is the name that will show up in the plugins list of the decal agent (the one in windows, not in-game)
	// View is the path to the xml file that contains info on how to draw our in-game plugin. The xml contains the name and icon our plugin shows in-game.
	// The view here is SamplePlugin.mainView.xml because our projects default namespace is SamplePlugin, and the file name is mainView.xml.
	// The other key here is that mainView.xml must be included as an embeded resource. If its not, your plugin will not show up in-game.
	[FriendlyName("ChaosHelper")]
	public class PluginCore : PluginBase
	{
        private static PluginCore _Instance = null;
        public static PluginCore Instance { get { return _Instance; } }

        private VirindiViewService.ViewProperties properties;
        private VirindiViewService.ControlGroup controls;
        private VirindiViewService.HudView view;

        private string chatLoc = "";
        public HudTextBox ChatCommand { get; private set; }
        public HudStaticText VersionLbl { get; private set; }
        public HudCombo ConfigChoice { get; private set; }
        
        public HudButton SetChatBtn { get; private set; }
        public HudButton LoadConfigBtn { get; private set; }
        public HudButton SaveDefaultsBtn { get; private set; }

        public HudList popoutList { get; private set; }

        public HudTabView TabView { get; private set; }


        public int currentConfig = 0;
        public int locx = 50, locy = 50;

        public int startingW = 335, startingH = 200;

        //Registered Dictionary of events
        private Dictionary<string, EventHandler> popoutEvents = new Dictionary<string, EventHandler>();

        private ArrayList sizes = new ArrayList();
        private ArrayList locations = new ArrayList();

        private Dictionary<string, PopoutWindow> popoutWindows = new Dictionary<string, PopoutWindow>();
        /// <summary>
        /// This is called when the plugin is started up. This happens only once.
        /// </summary>
        protected override void Startup()
		{
			try
			{
                _Instance = this;

				Globals.Init("Chaos-Helper", Host, Core);

                LoadWindow();
                CommandLineText += new EventHandler<ChatParserInterceptEventArgs>(FilterCore_CommandLineText);

            }
			catch (Exception ex) { Util.LogError(ex); Util.WriteToChat(ex.Message); }
		}

        [BaseEvent("LoginComplete", "CharacterFilter")]
        private void CharacterFilter_LoginComplete(object sender, EventArgs e)
        {
            try
            {
                LoadBaseXML(true, "");
                LoadIni();
            }
            catch (Exception ex) { Util.LogError(ex); }
        }

        /// <summary>
        /// This is called when the plugin is shut down. This happens only once.
        /// </summary>
        protected override void Shutdown()
		{
			try
			{
                CommandLineText -= new EventHandler<ChatParserInterceptEventArgs>(FilterCore_CommandLineText);
            }
			catch (Exception ex) { Util.LogError(ex); Util.WriteToChat(ex.Message); }
		}

        void LoadBaseXML(bool firstLoad, string configtoload)
        {
            if (firstLoad)
            {
                LoadListOfConfigs();
            }
            else
            {

                locx = view.Location.X;
                locy = view.Location.Y;
                currentConfig = ConfigChoice.Current;
                chatLoc = ChatCommand.Text;

                if (properties != null)
                {
                    properties.Dispose();
                }

                if (controls != null)
                {
                    controls.Dispose();
                }

                if (view != null)
                {
                    view.Dispose();
                }

                foreach (string p in popoutWindows.Keys)
                    popoutWindows[p].Dispose();

                popoutWindows.Clear();
                sizes.Clear();
                locations.Clear();

                LoadWindow();
                LoadListOfConfigs();
                if(configtoload == "")
                {
                    LoadConfig(((HudStaticText)ConfigChoice[currentConfig]).Text);
                }
                else
                {
                    for (int i = 0; i < ConfigChoice.Count; i++)
                    {
                        if (((HudStaticText)ConfigChoice[i]).Text.Trim() == configtoload.Trim())
                        {
                            ConfigChoice.Current = i;
                            LoadConfig(((HudStaticText)ConfigChoice[i]).Text);
                        }
                    }
                }
                
            }

            
        }

        void LoadIni()
        {
            string[] ini = Util.GetIni();
           
            foreach(string line in ini)
            {
                string[] col = line.Split(':');
                if(line.Contains("default config"))
                {
                    for (int i = 0; i < ConfigChoice.Count; i++)
                    {
                        if (((HudStaticText)ConfigChoice[i]).Text.Trim() == col[1].Trim())
                        {
                            ConfigChoice.Current = i;
                            LoadConfig(((HudStaticText)ConfigChoice[i]).Text);
                        }
                    }
                }
                else if(line.Contains("sendchatcommand"))
                {
                    chatLoc = col[1];
                    ChatCommand.Text = col[1];
                }
            } 
        }

        void LoadWindow()
        {
            // Create the view
            VirindiViewService.XMLParsers.Decal3XMLParser parser = new VirindiViewService.XMLParsers.Decal3XMLParser();
            parser.ParseFromResource("ChaosHelper.mainView.xml", out properties, out controls);

            // Display the view
            view = new VirindiViewService.HudView(properties, controls);

            TabView = view != null ? (HudTabView)view["nbMain"] : new HudTabView();

            ChatCommand = view != null ? (HudTextBox)view["ChatCommand"] : new HudTextBox();
            ConfigChoice = view != null ? (HudCombo)view["ConfigFiles"] : new HudCombo(controls);
            VersionLbl = view != null ? (HudStaticText)view["VersionLbl"] : new HudStaticText();

            SetChatBtn = view != null ? (HudButton)view["ChatCommandSet"] : new HudButton();
            LoadConfigBtn = view != null ? (HudButton)view["ReloadConfig"] : new HudButton();
            SaveDefaultsBtn = view != null ? (HudButton)view["SaveIni"] : new HudButton();

            popoutList = view != null ? (HudList)view["PopoutList"] : new HudList();

            SetChatBtn.Hit += new EventHandler(ChatCommandSet_Click);
            LoadConfigBtn.Hit += new EventHandler(ReloadConfig_Click);
            SaveDefaultsBtn.Hit += new EventHandler(SaveIni_Click);

            view.Location = new System.Drawing.Point(locx, locy);

            TabView.OpenTabChange += new EventHandler(TabChanged);

            VersionLbl.Text = "Version 2.2.6.0";
            ChatCommand.Text = chatLoc;
        }

        void GenerateLayout(string layoutStyle)
        {
            string[] layout = Util.LoadLayout(layoutStyle);

            try
            {
                int currentRow = 1;
                int currentCol = 1;
                int padding = 0;
                int width = 0;
                int height = 0;
                int cols = 0;
                int rows = 0;
                HudFixedLayout tempLayout = new HudFixedLayout();
                PopoutWindow tempPopoutwindow = new PopoutWindow();

                int buttonWidth = 0;
                int buttonHeight = 0;
                string currentTab = "";
                int button_count = 1;
                startingW = 335;
                startingH = 200;
                foreach (string _line in layout)
                {
                    string line = _line.Trim();

                    // replace first colon (if exist) with whitespace (make it optional)
                    int colonIndex = line.IndexOf(':');
                    if (colonIndex != -1)
                    {
                        line = line.Remove(colonIndex, 1);
                        line = line.Insert(colonIndex, " ");
                    }

                    // split directive out based on first whitespace
                    int sepIndex = line.IndexOfAny(new char[] { ' ', '\t' });
                    if (sepIndex == -1)
                        continue;

                    // isolate directive and content
                    string directive = line.Substring(0, sepIndex).Trim();
                    string content = line.Substring(sepIndex+1).Trim();

                    view.ClientArea = new System.Drawing.Size(startingW, startingH);

                    // for simple  [directive] [value]  lines  just compare a pre-lowercased version `simpleDirective`
                    // for complex directives where values are introduced in the first string (eg. Button_01) then parse using `directive`
                    string simpleDirective = directive.ToLowerInvariant();
                    if (simpleDirective == "windowposition")
                    {
                        string[] split = content.Split(',');
                        view.Location = new System.Drawing.Point(int.Parse(split[0]), int.Parse(split[1]));
                    }
                    else if (simpleDirective == "windowsize")
                    {
                        string[] split = content.Split(',');
                        view.ClientArea = new System.Drawing.Size(int.Parse(split[0]), int.Parse(split[1]));

                    }
                    else if (simpleDirective == "windowstartopen")
                    {
                        view.Visible = bool.Parse(content);
                    }
                    else if (simpleDirective == "buttonpadding")
                    {
                        padding = int.Parse(content);
                    }
                    else if (simpleDirective == "tab")
                    {
                        button_count = 1;
                        currentRow = 1;
                        currentCol = 1;
                        cols = 0;
                        rows = 0;
                        tempLayout = new HudFixedLayout();

                        tempPopoutwindow = new PopoutWindow();

                        tempLayout.InternalName = content;
                        currentTab = content;

                        popoutWindows.Add(content, tempPopoutwindow);
                        TabView.AddTab(tempLayout, content);
                    }
                    else if (simpleDirective == "tabvisible")
                    {
                        if (bool.Parse(content))
                        {
                            tempPopoutwindow.toggleVisibility();
                        }
                    }
                    else if (simpleDirective == "tabsize")
                    {
                        string[] split = content.Split(',');
                        width = int.Parse(split[0].Trim());
                        height = int.Parse(split[1].Trim());

                        sizes.Add(new System.Drawing.Size(width, height));
                        tempPopoutwindow.SetWindowSize(new System.Drawing.Size(width, height - 25));
                    }
                    else if (simpleDirective == "tabposition")
                    {
                        string[] split = content.Split(',');
                        int tabx = int.Parse(split[0].Trim());
                        int taby = int.Parse(split[1].Trim());

                        locations.Add(new System.Drawing.Point(tabx, taby));
                        tempPopoutwindow.SetWindowPos(new System.Drawing.Point(tabx, taby));
                    }
                    else if (simpleDirective == "cols")
                    {
                        cols = int.Parse(content.Trim());
                        buttonWidth = (int)((width - (padding * (1 + cols))) / cols);
                    }
                    else if (simpleDirective == "rows")
                    {
                        rows = int.Parse(content);
                        buttonHeight = (int)((height - (padding * (3 + rows))) / rows);
                    }
                    else
                    {
                        // OK doesnt seem like simple directive.. so either its totally bad, or its one we have to interpret

                        int span;// we will still have a span value.. so the following (optional) comma-seperated columns will just try to take place of .txt

                        // we may have columns of data
                        List<string> datCols = new List<string>(content.Split(','));

                        // if not, then make sure to put original value in here
                        if (datCols.Count == 0)
                            span = int.Parse(content);
                        else
                        {
                            // we have to put 1st entry into span,  then condense the rest of the arguments list to be zero-based
                            span = int.Parse(datCols[0]);
                            datCols.RemoveAt(0);
                        }

                        // lets see what we have...

                        if (directive.IndexOf("Button", StringComparison.InvariantCultureIgnoreCase) != -1)
                        {
                            string defText = null;
                            string defCommand = null;
                            string defParam = null;

                            if (datCols.Count > 0)
                                defText = datCols[0];

                            if (datCols.Count > 1)
                                defCommand = datCols[1];

                            if (datCols.Count > 2)
                                defCommand = datCols[2];

                            if (string.IsNullOrEmpty(defText))
                                defText = currentTab + "_" + button_count.ToString("D2");

                            //Creates Button
                            ChaosHudButton tempBtn = new ChaosHudButton(defText, defCommand, defParam);
                            ChaosHudButton tempPopBtn = new ChaosHudButton(defText, defCommand, defParam);

                            tempBtn.InternalName = currentTab + "_Button_" + button_count.ToString("D2");
                            tempPopBtn.InternalName = currentTab + "_Button_" + button_count.ToString("D2");

                            int x = (padding * (currentCol)) + (buttonWidth * (currentCol - 1));
                            int y = (padding * (currentRow)) + (buttonHeight * (currentRow - 1));
                            int btnW = (buttonWidth * span) + (padding * (span - 1));
                            int btnH = buttonHeight;


                            tempLayout.AddControl(tempBtn, new System.Drawing.Rectangle(x, y, btnW, btnH));
                            popoutWindows[currentTab].AddButton(tempPopBtn, new System.Drawing.Rectangle(x, y, btnW, btnH));

                            currentCol += span;
                            if (currentCol > cols)
                            {
                                currentCol = 1;
                                currentRow++;
                            }

                            button_count++;
                        }
                        else if (directive.IndexOf("StaticText", StringComparison.InvariantCultureIgnoreCase) != -1)
                        {
                            string defText = null;

                            if (datCols.Count > 0)
                                defText = datCols[0];

                            if (string.IsNullOrEmpty(defText))
                                defText = currentTab + "_" + button_count.ToString("D2");

                            //Creates Button
                            ChaosHudStaticText tempBtn = new ChaosHudStaticText(defText);
                            ChaosHudStaticText tempPopBtn = new ChaosHudStaticText(defText);

                            tempBtn.TextAlignment = VirindiViewService.WriteTextFormats.Center | VirindiViewService.WriteTextFormats.VerticalCenter;
                            tempPopBtn.TextAlignment = VirindiViewService.WriteTextFormats.Center | VirindiViewService.WriteTextFormats.VerticalCenter;

                            tempBtn.InternalName = currentTab + "_StaticText_" + button_count.ToString("D2");
                            tempPopBtn.InternalName = currentTab + "_StaticText_" + button_count.ToString("D2");

                            int x = (padding * (currentCol)) + (buttonWidth * (currentCol - 1));
                            int y = (padding * (currentRow)) + (buttonHeight * (currentRow - 1));
                            int btnW = (buttonWidth * span) + (padding * (span - 1));
                            int btnH = buttonHeight;

                            tempLayout.AddControl(tempBtn, new System.Drawing.Rectangle(x, y, btnW, btnH));
                            popoutWindows[currentTab].AddStaticText(tempPopBtn, new System.Drawing.Rectangle(x, y, btnW, btnH));

                            currentCol += span;
                            if (currentCol > cols)
                            {
                                currentCol = 1;
                                currentRow++;
                            }

                            button_count++;
                        }


                    }
                }

                CreatePopoutList();
            }
            catch(Exception ex)
            {
                Util.WriteToChat("Error Loading Layout: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        void LoadListOfConfigs()
        {
            string[] configs = Util.GetListofConfigs();

            if(ConfigChoice.Count > 0)
                ConfigChoice.Clear();

            for(int i = 0; i < configs.Length; i++)
            {
                HudStaticText temp = new HudStaticText();
                temp.Text = configs[i];
                ConfigChoice.AddItem(temp, configs[i]);
            }
        }

        /// <summary>
        /// Dynamically generates the final command string from provided values and dispatches message to game.
        /// </summary>
        public static void DispatchCommand(string command, string param)
        {
            PluginCore.DispatchChatToBoxWithPluginIntercept(Instance.GenerateFinalCommandString(command, param));
        }

        private string GenerateFinalCommandString(string command, string param)
        {
            if (string.IsNullOrEmpty(command))
                return null;

            if (command.Contains("[player]"))
            {
                command = command.Replace("[player]", Core.CharacterFilter.Name);
            }

            if (command.Contains("[loc]"))
            {
                command = command.Replace("[loc]", Core.WorldFilter.GetByName(Core.CharacterFilter.Name).First.Coordinates().ToString());
            }

            var regexItem = new Regex("^[a-zA-Z0-9 ]*$");

            if (command.StartsWith("/") || regexItem.IsMatch(command))
            {
                //if is a /tell command
                if (!string.IsNullOrEmpty(param) && command.StartsWith("/"))
                {
                    return command + "," + param;
                }
                //Handle / commands
                else if (command.StartsWith("/"))
                {
                    return command;
                }
                //Handle raw text
                else
                {
                    return chatLoc + " " + command;
                }


            }
            else
            {

                return chatLoc + " " + command;
            }
        }

        void LoadConfig(string configName)
        {
            for (int i = 0; i < ConfigChoice.Count; i++)
            {
                if (((HudStaticText)ConfigChoice[i]).Text == configName.Trim())
                {
                    ConfigChoice.Current = i;
                }
            }

            string[] configInfo = Util.GetConfig(configName.Trim());
            if(configInfo != null)
            {
                foreach(string _line in configInfo)
                {
                    string line = _line.Trim();

                    try
                    {
                        if(line.StartsWith("LAYOUT:", StringComparison.InvariantCultureIgnoreCase))
                        {
                            string[] parts = line.Split(':');
                            if (parts.Length < 2)
                                continue;

                            GenerateLayout(parts[1].Trim());
                        }
                        else
                        {
                            string[] col = line.Split(',');
                            
                            // perhaps we have a single-column control declaration like a break/spacer;  so make a "fake" array with just the control name
                            if (col.Length == 0)
                                col = new string[] { line };


                            // isolate control name and control object
                            string ctrlName = col[0];
                            if (string.IsNullOrEmpty(ctrlName))// might be a blank line.. skip
                                continue;

                            IChaosHudControl ctrl;
                            try
                            {
                                ctrl = view[ctrlName] as IChaosHudControl;
                            }
                            catch
                            {
                                Util.WriteToChat($"cant find {ctrlName} when parsing {configName.Trim()}; check your .layout file!");
                                continue;
                            }


                            if(ctrl is ChaosHudButton)
                            {
                                ChaosHudButton temp = (ChaosHudButton)ctrl;

                                string currentTabName = ctrlName.Substring(0, ctrlName.IndexOf('_'));
                                //check if button exists
                                if (temp != null)
                                {
                                    //Check if button should be set to visible
                                    if (col[1].Contains("NOTSET"))
                                    {
                                        temp.Visible = false;
                                        popoutWindows[currentTabName].ChangeControlInfo(ctrlName, false, "");
                                    }
                                    //If button is an image button
                                    
                                    // Register the button event handler and make visible
                                    else
                                    {
                                        if (col[1].Contains("[") && col[1].Contains("]"))
                                        {
                                            //Remove button, replace with image button and register bindings
                                            //temp.Visible = false;

                                            string clean = col[1].Replace("[", "");
                                            clean = clean.Replace("]", "");

                                            //Format : [<icon id>|<text>]
                                            int iconImage = 0;

                                            if(!clean.Contains("|"))
                                            {
                                                iconImage = int.Parse(clean);

                                                VirindiViewService.ACImage tempImage = new VirindiViewService.ACImage(iconImage);
                                                popoutWindows[currentTabName].SetImage(ctrlName, tempImage);
                                                popoutWindows[currentTabName].ChangeControlInfo(ctrlName, true, "");
                                                temp.Image = tempImage;
                                                temp.Text = "";
                                            }
                                            else
                                            {
                                                string[] imageSettings = clean.Split('|');
                                                iconImage = int.Parse(imageSettings[0]);
                                                int iconBG = 0;
                                                if(imageSettings.Length > 1)
                                                {
                                                    if(int.TryParse(imageSettings[1], out iconBG))
                                                    {
                                                        popoutWindows[currentTabName].SetImage(ctrlName, new VirindiViewService.ACImage(iconImage));
                                                        popoutWindows[currentTabName].ChangeControlInfo(ctrlName, true, "");

                                                        temp.Image = new VirindiViewService.ACImage(iconImage); 
                                                        temp.Image = new VirindiViewService.ACImage(iconBG);
                                                        Util.WriteToChat("Adding Double Image");
                                                        if (imageSettings.Length == 3)
                                                        {
                                                            temp.Text = imageSettings[2];
                                                        }

                                                    }
                                                    else
                                                    {
                                                        VirindiViewService.ACImage tempImage = new VirindiViewService.ACImage(iconImage);
                                                        popoutWindows[currentTabName].SetImage(ctrlName, tempImage);
                                                        popoutWindows[currentTabName].ChangeControlInfo(ctrlName, true, imageSettings[1]);
                                                        temp.Image = tempImage;
                                                        if(imageSettings.Length == 2)
                                                        {
                                                            temp.Text = imageSettings[1];
                                                        }
                                                        
                                                    }
                                                }
                                            }
                                            
                                        }
                                        else
                                        {
                                            temp.Text = col[1];
                                            popoutWindows[currentTabName].ChangeControlInfo(ctrlName, true, col[1]);
                                        }

                                        temp.Visible = true;

                                        //Creates the event handler for each button

                                        string strParam = null;
                                        if (col.Length >= 4)
                                            strParam = col[3];


                                        // override command for main form
                                        temp.Command = col[2];
                                        temp.Param = strParam;

                                        // override command for popup form
                                        popoutWindows[currentTabName].ChangeControlCommand(ctrlName, col[2], strParam);

                                    }
                                }
                            } else if (ctrl is HudStaticText)
                            {
                                HudStaticText temp = (HudStaticText)ctrl;

                                string currentTabName = ctrlName.Substring(0, ctrlName.IndexOf('_'));
                                //check if button exists
                                if (temp != null)
                                {
                                    //Check if button should be set to visible
                                    if (col[1].Contains("NOTSET"))
                                    {
                                        temp.Visible = false;
                                        popoutWindows[currentTabName].ChangeControlInfo(ctrlName, false, "");
                                    }
                                    //If button is an image button

                                    // Register the button event handler and make visible
                                    else
                                    {
                                        temp.Text = col[1];
                                        popoutWindows[currentTabName].ChangeControlInfo(ctrlName, true, col[1]);

                                        temp.Visible = true;
                                    }
                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                      Util.WriteToChat("Error Loading Config at :" + line + "\n : Error: " + ex.Message + "\n" + ex.StackTrace);
                    } 
                }
            }
            else
            {
               Util.WriteToChat("Error Loading Config!");
            }
        }


        //
        //  ?  Tab
        //
        private void ChatCommandSet_Click(object sender, EventArgs e)
        {
            //Set the specified chat command text
            chatLoc = ChatCommand.Text;
            LoadBaseXML(false, "");
            view.Visible = true;
            Util.WriteToChat("Changing chat command to : " + chatLoc);
        }

        private void ReloadConfig_Click(object sender, EventArgs e)
        {
            //Load the specified config file
            LoadBaseXML(false, "");
            view.Visible = true;
            Util.WriteToChat("Reloading Config...");
        }

        private void SaveIni_Click(object sender, EventArgs e)
        {
            //Save the ini so it can be used later
            Util.SaveIni(ChatCommand.Text, ((HudStaticText)ConfigChoice[ConfigChoice.Current]).Text);
            Util.WriteToChat("Saving Defaults...");
        }

        private void TabChanged(object sender, EventArgs e)
        {
            if(TabView.CurrentTab == 0)
            {
                view.ClientArea = new System.Drawing.Size(startingW, startingH);
            }
            else
            {
                view.Width = ((System.Drawing.Size)sizes[TabView.CurrentTab - 1]).Width;
                view.Height = ((System.Drawing.Size)sizes[TabView.CurrentTab - 1]).Height;
            }
        }

        private void FilterCore_CommandLineText(object sender, ChatParserInterceptEventArgs e)
        {
            if(e.Text.ToLower().StartsWith("/ch"))
            {
                string setprofile = "/ch setprofile";
                string setchatcommand = "/ch setchatcommand";
                string sethelpcommand = "/ch help";
                string settabcommand = "/ch settab";

                //If set profile command is issued
                if (e.Text.ToLower().Contains(setprofile))
                {
                    bool setvis = view.Visible;
                    string temp = e.Text;
                    temp = temp.Remove(0, setprofile.Length).Trim();
                    LoadBaseXML(false, temp);
                    if(setvis)
                    {
                        view.Visible = true;
                    }
                    else
                    {
                        view.Visible = false;
                    }
                    
                }
                //If set chat command is issued
                else if (e.Text.ToLower().Contains(setchatcommand))
                {
                    bool setvis = view.Visible;
                    string temp = e.Text;
                    temp = temp.Remove(0, setchatcommand.Length);
                    ChatCommand.Text = chatLoc = temp.Trim();
                    LoadBaseXML(false, "");
                    if (setvis)
                    {
                        view.Visible = true;
                    }
                    else
                    {
                        view.Visible = false;
                    }

                }
                //set tab command
                else if (e.Text.ToLower().Contains(settabcommand))
                {
                    bool setvis = view.Visible;
                    string temp = e.Text;
                    temp = temp.Remove(0, settabcommand.Length);
                    int tab = int.Parse(temp);

                    if(!(tab > TabView.TabCount - 1) && !(tab < 0))
                    {
                        TabView.CurrentTab = tab;
                    }
                }
                //Show help
                else if (e.Text.ToLower().Contains(sethelpcommand))
                {
                    Globals.Host.Actions.AddChatText("setprofile profilename.txt : sets the profile", 5);
                    Globals.Host.Actions.AddChatText("setchatcommand /c : sets chat command to /c", 5);
                    Globals.Host.Actions.AddChatText("settab # : sets tab to tab #", 5);
                }

                //Do not execute as AC command.
                e.Eat = true;
            }          
        }

        private void TogglePopout(object sender, EventArgs e, string name)
        {
            if(popoutWindows.ContainsKey(name))
            {
                popoutWindows[name].toggleVisibility();
            }
        }

        private void CreatePopoutList()
        {
            popoutList.ClearColumnsAndRows();
            popoutEvents.Clear();
            popoutList.AddColumn(typeof(HudStaticText), 70, "name");
            popoutList.AddColumn(typeof(HudCheckBox), 30, "?");

            for(int i = 1; i < TabView.TabCount; i++)
            {
                string tabName = TabView.GetTabName(i);
                HudList.HudListRowAccessor newRow = popoutList.AddRow();
                ((HudStaticText)newRow[0]).Text = tabName;
                HudCheckBox temp = new HudCheckBox();

                //Check if tab starts popped out, and check it.
                if (popoutWindows[tabName].visible)
                {
                    temp.Checked = true;
                }

                popoutWindows[tabName].setCheckBox(temp);

                EventHandler tempEH = new EventHandler((s, e) => TogglePopout(s, e, tabName));
                popoutEvents.Add(tabName, tempEH);
                temp.Change += tempEH;
                newRow[1] = temp;
            }
        }

        [DllImport("Decal.dll")]
        static extern int DispatchOnChatCommand(ref IntPtr str, [MarshalAs(UnmanagedType.U4)] int target);

        static bool Decal_DispatchOnChatCommand(string cmd)
        {
            IntPtr bstr = Marshal.StringToBSTR(cmd);

            try
            {
                bool eaten = (DispatchOnChatCommand(ref bstr, 1) & 0x1) > 0;

                return eaten;
            }
            finally
            {
                Marshal.FreeBSTR(bstr);
            }
        }

        /// <summary>
        /// This will first attempt to send the messages to all plugins. If no plugins set e.Eat to true on the message, it will then simply call InvokeChatParser.
        /// </summary>
        /// <param name="cmd"></param>
        public static void DispatchChatToBoxWithPluginIntercept(string cmd)
        {
            if (string.IsNullOrEmpty(cmd))
                return;

            if (!Decal_DispatchOnChatCommand(cmd))
                CoreManager.Current.Actions.InvokeChatParser(cmd);
        }

        public static Bitmap MergeTwoImages(Image firstImage, Image secondImage)
        {
            if (firstImage == null)
            {
                throw new ArgumentNullException("firstImage");
            }

            if (secondImage == null)
            {
                throw new ArgumentNullException("secondImage");
            }

            int outputImageWidth = firstImage.Width > secondImage.Width ? firstImage.Width : secondImage.Width;

            int outputImageHeight = firstImage.Height + secondImage.Height + 1;

            Bitmap outputImage = new Bitmap(outputImageWidth, outputImageHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics graphics = Graphics.FromImage(outputImage))
            {
                graphics.DrawImage(firstImage, new Rectangle(new Point(), firstImage.Size),
                    new Rectangle(new Point(), firstImage.Size), GraphicsUnit.Pixel);
                graphics.DrawImage(secondImage, new Rectangle(new Point(0, firstImage.Height + 1), secondImage.Size),
                    new Rectangle(new Point(), secondImage.Size), GraphicsUnit.Pixel);
            }

            return outputImage;
        }
    }
}
