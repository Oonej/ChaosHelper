﻿using System;
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
        private Dictionary<string, EventHandler> regEvents = new Dictionary<string, EventHandler>();
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

            VersionLbl.Text = "Version 2.2.5.0";
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
                foreach (string line in layout)
                {
                    string[] words = { "windowsize:", "buttonpadding:", "tab:", "cols:", "rows:", "button_" };

                    string temp = "";


                    view.ClientArea = new System.Drawing.Size(startingW, startingH);

                    if (line.Contains("windowposition:"))
                    {
                        temp = line.Remove(0, "windowposition:".Length);
                        string[] split = temp.Split(',');
                        view.Location = new System.Drawing.Point(int.Parse(split[0]), int.Parse(split[1]));
                    }
                    else if(line.Contains("windowsize:"))
                    {
                        temp = line.Remove(0, "windowsize:".Length);
                        string[] split = temp.Split(',');
                        startingW = int.Parse(split[0]);
                        startingH = int.Parse(split[1]);

                        view.ClientArea = new System.Drawing.Size(startingW, startingW);

                    }
                    else if(line.Contains("windowstartopen:"))
                    {
                        temp = line.Remove(0, "windowstartopen:".Length);
                        view.Visible = bool.Parse(temp);
                    }
                    else if (line.Contains("buttonpadding:"))
                    {
                        temp = line.Remove(0, words[1].Length);
                        padding = int.Parse(temp.Trim());
                    }
                    else if (line.Contains("tab:"))
                    {
                        button_count = 1;
                        currentRow = 1;
                        currentCol = 1;
                        cols = 0;
                        rows = 0;
                        temp = line.Remove(0, words[2].Length).Trim();
                        tempLayout = new HudFixedLayout();

                        tempPopoutwindow = new PopoutWindow();

                        tempLayout.InternalName = temp;
                        currentTab = temp;

                        popoutWindows.Add(temp, tempPopoutwindow);
                        TabView.AddTab(tempLayout, temp);
                    }
                    else if (line.Contains("tabvisible:"))
                    {
                        temp = line.Remove(0, "tabvisible:".Length);
                        if(bool.Parse(temp))
                        {
                            tempPopoutwindow.toggleVisibility();
                        }
                    }
                    else if (line.Contains("tabsize:"))
                    {
                        temp = line.Remove(0, "tabsize:".Length);
                        string[] split = temp.Split(',');
                        width = int.Parse(split[0].Trim());
                        height = int.Parse(split[1].Trim());

                        sizes.Add(new System.Drawing.Size(width, height));
                        tempPopoutwindow.SetWindowSize(new System.Drawing.Size(width, height - 25));
                    }
                    else if (line.Contains("tabposition:"))
                    {
                        temp = line.Remove(0, "tabposition:".Length);
                        string[] split = temp.Split(',');
                        int tabx = int.Parse(split[0].Trim());
                        int taby = int.Parse(split[1].Trim());

                        locations.Add(new System.Drawing.Point(tabx, taby));
                        tempPopoutwindow.SetWindowPos(new System.Drawing.Point(tabx, taby));
                    }
                    else if (line.Contains("cols:"))
                    {
                        temp = line.Remove(0, words[3].Length);
                        cols = int.Parse(temp.Trim());
                        buttonWidth = (int)((width - (padding * (1 + cols))) / cols);
                    }
                    else if (line.Contains("rows:"))
                    {
                        temp = line.Remove(0, words[4].Length);
                        rows = int.Parse(temp.Trim());
                        buttonHeight = (int)((height - (padding * (3 + rows))) / rows);
                    }
                    else if (line.Contains("Button"))
                    {
                        int span = int.Parse(line.Remove(0, words[5].Length + 3).Trim());

                        //Creates Button
                        HudButton tempBtn = new HudButton();
                        HudButton tempPopBtn = new HudButton();
                        
                        tempBtn.Text = currentTab + "_" + button_count.ToString("D2");
                        tempBtn.InternalName = currentTab + "_Button_" + button_count.ToString("D2");
                        tempPopBtn.Text = currentTab + "_" + button_count.ToString("D2");
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
                foreach(string line in configInfo)
                {
                    try
                    {
                        if(line.Contains("LAYOUT:"))
                        {                       
                            string layoutTemp = line.Remove(0, "LAYOUT:".Length).Trim();
                            GenerateLayout(layoutTemp);
                        }
                        else
                        {
                            string[] col = line.Split(',');
                            if(view[col[0]].GetType() == typeof(HudButton))
                            {
                                HudButton temp = (HudButton)view[col[0]];

                                string currentTabName = col[0].Substring(0, col[0].IndexOf('_'));
                                //check if button exists
                                if (temp != null)
                                {
                                    //Check if button should be set to visible
                                    if (col[1].Contains("NOTSET"))
                                    {
                                        temp.Visible = false;
                                        popoutWindows[currentTabName].ChangeBtnInfo(col[0], false, "");
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
                                                popoutWindows[currentTabName].SetImage(col[0], tempImage);
                                                popoutWindows[currentTabName].ChangeBtnInfo(col[0], true, "");
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
                                                        popoutWindows[currentTabName].SetImage(col[0], new VirindiViewService.ACImage(iconImage));
                                                        popoutWindows[currentTabName].ChangeBtnInfo(col[0], true, "");

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
                                                        popoutWindows[currentTabName].SetImage(col[0], tempImage);
                                                        popoutWindows[currentTabName].ChangeBtnInfo(col[0], true, imageSettings[1]);
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
                                            popoutWindows[currentTabName].ChangeBtnInfo(col[0], true, col[1]);
                                        }

                                        temp.Visible = true;
                                        
                                        //Creates the event handler for each button

                                        if(col[2].Contains("[player]"))
                                        {
                                            col[2] = col[2].Replace("[player]", Core.CharacterFilter.Name);
                                        }

                                        if(col[2].Contains("[loc]"))
                                        {
                                            col[2] = col[2].Replace("[loc]", Core.WorldFilter.GetByName(Core.CharacterFilter.Name).First.Coordinates().ToString());
                                        }

                                        var regexItem = new Regex("^[a-zA-Z0-9 ]*$");

                                        if (col[2].StartsWith("/") || regexItem.IsMatch(col[2]))
                                        {
                                            //if is a /tell command
                                            if (col.Length == 4 && col[2].StartsWith("/"))
                                            {
                                                EventHandler newEvent = new EventHandler((s, e) => ClickCommand(s, e, col[2] + "," + col[3]));
                                                EventHandler newPopupEvent = new EventHandler((s, e) => ClickCommand(s, e, col[2] + "," + col[3]));

                                                popoutWindows[currentTabName].SetEvent(col[0], newPopupEvent);

                                                if (regEvents.ContainsKey(col[0]))
                                                {
                                                    //Unregister the event handler
                                                    temp.Hit -= regEvents[col[0]];

                                                    //Store the event inside the dictionary so we can unregister it later
                                                    regEvents[col[0]] = newEvent;
                                                }
                                                else
                                                {
                                                    //Replace the event
                                                    regEvents[col[0]] = newEvent;
                                                }

                                                //Register the event

                                                temp.Hit += newEvent;
                                            }
                                            //Handle / commands
                                            else if(col[2].StartsWith("/"))
                                            {
                                                EventHandler newEvent = new EventHandler((s, e) => ClickCommand(s, e, col[2]));
                                                EventHandler newPopupEvent = new EventHandler((s, e) => ClickCommand(s, e, col[2]));

                                                popoutWindows[currentTabName].SetEvent(col[0], newPopupEvent);

                                                if (regEvents.ContainsKey(col[0]))
                                                {
                                                    //Unregister the event handler
                                                    temp.Hit -= regEvents[col[0]];

                                                    //Store the event inside the dictionary so we can unregister it later
                                                    regEvents[col[0]] = newEvent;
                                                }

                                                else
                                                {
                                                    //Replace the event
                                                    regEvents[col[0]] = newEvent;
                                                }

                                                //Register the event
                                                temp.Hit += newEvent;
                                            }
                                            //Handle raw text
                                            else
                                            {
                                                EventHandler newEvent = new EventHandler((s, e) => ClickCommand(s, e, chatLoc + " " + col[2]));
                                                EventHandler newPopupEvent = new EventHandler((s, e) => ClickCommand(s, e, chatLoc + " " + col[2]));

                                                popoutWindows[currentTabName].SetEvent(col[0], newPopupEvent);

                                                if (regEvents.ContainsKey(col[0]))
                                                {
                                                    //Unregister the event handler
                                                    temp.Hit -= regEvents[col[0]];

                                                    //Store the event inside the dictionary so we can unregister it later
                                                    regEvents[col[0]] = newEvent;
                                                }

                                                else
                                                {
                                                    //Replace the event
                                                    regEvents[col[0]] = newEvent;
                                                }

                                                //Register the event
                                                temp.Hit += newEvent;
                                            }

                                            
                                        }
                                        else
                                        {
                                          
                                            EventHandler newEvent = new EventHandler((s, e) => ClickCommand(s, e, chatLoc + " " + col[2]));
                                            EventHandler newPopupEvent = new EventHandler((s, e) => ClickCommand(s, e, chatLoc + " " + col[2]));

                                            popoutWindows[currentTabName].SetEvent(col[0], newPopupEvent);

                                            if (regEvents.ContainsKey(col[0]))
                                            {
                                                //Unregister the event handler
                                                temp.Hit -= regEvents[col[0]];

                                                //Store the event inside the dictionary so we can unregister it later
                                                regEvents[col[0]] = newEvent;
                                            }

                                            else
                                            {
                                                //Replace the event
                                                regEvents[col[0]] = newEvent;
                                            }

                                            //Register the event

                                            temp.Hit += newEvent;
                                        }

                                       

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

        private void ClickCommand(object sender, EventArgs e, string command)
        {
            DispatchChatToBoxWithPluginIntercept(command);
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
