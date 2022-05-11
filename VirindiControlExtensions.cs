using System;
using System.Collections.Generic;
using System.Text;
using VirindiViewService.Controls;

namespace ChaosHelper
{
    namespace VirindiControlExtensions
    {
        public interface IChaosHudControl
        {
            bool Visible { get; set; }
            string Text { get; set; }
        }

        public class ChaosHudButton : HudButton, IChaosHudControl
        {
            public string Command;

            public ChaosHudButton(string _Command)
            {
                Command = _Command;

                Hit += ChaosHudButton_Hit;
            }

            private void ChaosHudButton_Hit(object sender, EventArgs e)
            {
                if (string.IsNullOrEmpty(Command))
                    return;

                PluginCore.DispatchChatToBoxWithPluginIntercept(Command);
            }
        }

        public class ChaosHudImageButton : HudImageButton, IChaosHudControl
        {
            public string Command;

            public ChaosHudImageButton(string _Command)
            {
                Command = _Command;

                Hit += ChaosHudButton_Hit;
            }

            // dummy text field
            private string _Text = string.Empty;
            public string Text { get { return _Text; } set { _Text = value; } }

            private void ChaosHudButton_Hit(object sender, EventArgs e)
            {
                if (string.IsNullOrEmpty(Command))
                    return;

                PluginCore.DispatchChatToBoxWithPluginIntercept(Command);
            }
        }

        public class ChaosHudStaticText : HudStaticText, IChaosHudControl
        {

        }

        public class ChaosHudCheckBox : HudCheckBox, IChaosHudControl
        {
            public string OnCommand;
            public string OffCommand;

            public ChaosHudCheckBox(string _OnCommand, string _OffCommand)
            {
                OnCommand = _OnCommand;
                OffCommand = _OffCommand;

                Change += ChaosHudCheckBox_Change;
            }

            private void ChaosHudCheckBox_Change(object sender, EventArgs e)
            {
                if (Checked)
                {
                    if(!string.IsNullOrEmpty(OnCommand))
                        PluginCore.DispatchChatToBoxWithPluginIntercept(OnCommand);
                }
                else
                {
                    if(!string.IsNullOrEmpty(OffCommand))
                        PluginCore.DispatchChatToBoxWithPluginIntercept(OffCommand);
                }
            }
        }


    }
}
