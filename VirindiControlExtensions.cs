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
            public string Param;

            public ChaosHudButton(string _Text, string _Command, string _Param)
            {
                Text = _Text;
                Command = _Command;
                Param = _Param;

                Hit += ChaosHudButton_Hit;
            }

            private void ChaosHudButton_Hit(object sender, EventArgs e)
            {
                PluginCore.DispatchCommand(Command, Param);
            }
        }

        public class ChaosHudImageButton : HudImageButton, IChaosHudControl
        {
            public string Command;
            public string Param;

            public ChaosHudImageButton(string _Command, string _Param)
            {
                Command = _Command;
                Param = _Param;

                Hit += ChaosHudImageButton_Hit;
            }

            // dummy text field
            private string _Text = string.Empty;
            public string Text { get { return _Text; } set { _Text = value; } }

            private void ChaosHudImageButton_Hit(object sender, EventArgs e)
            {
                PluginCore.DispatchCommand(Command, Param);
            }
        }

        public class ChaosHudStaticText : HudStaticText, IChaosHudControl
        {
            public ChaosHudStaticText(string _Text)
            {
                Text = _Text;
            }
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
                    PluginCore.DispatchCommand(OnCommand, null);
                }
                else
                {
                    PluginCore.DispatchCommand(OffCommand, null);
                }
            }
        }


    }
}
