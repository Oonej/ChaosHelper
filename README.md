# ChaosHelper

This is a clone of the released source code for **ChaosHelper** Decal plugin for Asheron's Call; with additional fixes/improvements.

## Overview

### About

#### Original Description:
This is a simple, yet useful (I'm biased), decal plugin that will allow you to customize your commands and make everything a push button instead of using hotkeys and/or chat commands.

#### Original Source:
http://www.immortalbob.com/phpBB3/viewtopic.php?f=25&t=282&sid=76b627477fa5b27afa154276ce9db28e

### License / Build Info _(may be from SamplePlugin?)_
```
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
```

### Credits
Original Plugin: Invisible Fire of MT

Additional Changes _(this repository)_: Strike Athius of RC



## Original Plugin Information _(copied from forum post)_

![sample screenshot](ChaosHelper.png)

To use this plugin, add the files in the zip to the same directory of your choice and add the dll to decal.

To change where you wish the chat text to go, aka /f, /a, etc... modify Chat command: text box and press the 'set' button..

To customize this plugin modify the chaoshelper_config.txt, or any loaded config file. (you can do this while in game, and reload after you save your changes by going to the ? tab and pressing Load button).

to customize the view, modify main.layout or create a new one. Be sure to specify the layout in the top of the config file as it appears in the default file this plugin comes with. Examples Below

A new button was added in v1.2 which allows you to save your defaults. Go to the ? tab and press the save defaults button to save any changes you've made.

New Chat commands were added in v1.2 which allows you to load configs and set chat command through the chat window / metas / etc.
### Chat Commands:
- /ch help
- /ch setprofile profileName.txt
- /ch setchatcommand /f
- /ch settab # NEW

### Button Configuration File (.txt)
First Line : This is the layout that will be associated with the button layout. Every config needs a layout!

(columns represented by commas ',')

First Column : this is how the script knows which buttons to set to what. <TabName>\_Button\_\#\#
  
Second column : This is the text of the button. Set it how ever you wish! Example : Loot Rares
  
-- If you wish for the button to be not visible, set the text to be NOTSET and it will hide from the screen.
  
-- If you want a button to have an icon, add the icon value (Decimal! NOT Hex!) in brackets Example: [12345]
  
Third column : This is the command that will be sent to the chat. !loot rares
  
--You can now use any command prefix, aka ! or # or % or & etc etc...
  
--If you want this to run a /command, just add the /command here. Be sure not to add more than one comma as it may bug it out. For instance: /tell Invisible Fire, boom! will work, but /tell invisible fire, hi, there! will not!
  
--If you want this meta to only apply to the player calling the command, you can now use [player] to plug the player name into the command. EXAMPLE: /tell [player], I'm talking to myself... OUTPUT: "You think, "I'm talking to myself..."
  
--If you want to display the coords of the player who calls the command, you can now use [loc] to plug the coordinates of the player into the command. Example: I'm located at [loc]! OUTPUT: "I'm located at 34.5s, 54.2w!

##### Example:
```
LAYOUT: main.layout
Adv_Button_01,Loot Rares,!loot rares
Adv_Button_02,Loot All,!loot all
Adv_Button_03,Jiggle On,!jiggle on
Adv_Button_04,Jiggle Off,!jiggle off
Adv_Button_05,Peace On,!peace on
```

##### Example of [player]:
```
Adv_Button_01,Talk to Yourself,/tell [player], Hi, how are you?
```

##### Example of [loc]:
```
Adv_Button_02,My Loc,[loc]
```

##### Example of Icons:
```
Ports_Button_02,[9561],!prim
Ports_Button_03,[9562],!sec
Ports_Button_04,[10733],!pr
Ports_Button_05,[10725],!aphus
Ports_Button_05,[10725|Show Text],!aphus < Will also display text!
```

Some nice button backgrounds include:
6956, 6957, 6958, 6959
19518, 19519, 19532, 19533, 19534, 19535
etc..

Use Digero's Ac Icon Browser to view all possible icons - http://decal.acasylum.com/icon_browser.php
Right-click an icon to copy the Decimal value

### Modifying/Creating the layout: (.layout)
```
windowposition: 50, 50 < Default load location of window
windowsize: 360, 200 < Sets the main tab size (if not set will default to 360, 200
windowstartopen: true < Sets the layout to default open
buttonpadding: 10 < Padding size (how much space between buttons)
tab: Basic < Tab Name. Buttons in the config text file will need to labeled by TabName_Button_## (example: Basic_Button_01)
tabvisible: false < Sets if the tab should start visible or not (true or false)
tabsize: 340, 350 < Size of the tab and the popout window
tabposition: 390, 50 < Default location of the tab popout
cols: 2 < Number of columns you want
rows: 7 < Number of Rows
Button_01: 2 < Span value for button 1 (make it span 2 columns)
Button_02: 1 < Span value for button 2 (make it only span 1 column)
Button_03: 1 < MORE BUTTONS!!! ARGH
Button_04: 1
tab: Adv < Tab Name for the second tab
tabvisible: false < Sets if the tab should start visible or not (true or false)
tabsize: 340, 350 < Size of the tab and the popout window
tabposition: 390, 50 < Default location of the tab popout
cols: 2 < Number of Columns for this tab
rows: 7 < Number of Rows for this tab
Button_01: 1 < Span of 1
Button_02: 1
etc.
```

### Known Issues:
None at the moment. Please leave feed back so I can properly fix known issues! Thanks :D

### Upcoming Features:
Possibly allow the ability to load custom images instead of AC Icons... if I get a big enough request, I will do this.

### Change Notes:
```
V2.2.5 NEW
Added new chat command /ch settab # to auto switch to a tab via command.
V2.2.4
Added "windowsize:" option to layout files. This will adjust the main tab (?) size to prevent large layouts from breaking.
V2.2.3
Added the ability to set commands with coordinates of the player who issues the command. EX: Misc_Button_04,My LOC ,[loc]
Added functionality so any prefix symbol will work! #, $, %, ^ etc etc.. everything should work now!
V2.2.2
Added the ability to set commands with player names. EX : Misc_Button_04,Self Command, /tell [player], !Command
V2.2.1
Made it so /ch setprofile profile.txt will not auto-popup main window unless it was already visible. -As requested by snail
Added correct rynthid recall mask icon to the ports tab
V2.2.0
New version 'should not' affect old configs/layouts. If it does, Sorry!
Added Icon-able buttons
Added Ability to send /commands (full control of vtank commands if you like!)
Added closable popup windows.
V2.1.0
Added Popout tabs
V2.0.0
Added customizable windows!!
V1.2.1
Fixed command bug causing players to not be able to chat.
V1.2
NOTE: Old config files will need to be modified (Remove the first line that says "SETCHATCOMMAND" completely)
Ini file now contains defaults for profile and chat command
Drop down with hot-swappable config files
Chat commands!.
- /ch help
- /ch setprofile profileName.txt
- /ch setchatcommand /f
V1.1
Added customization txt file to modify button scripts to anything you'd like.
```
