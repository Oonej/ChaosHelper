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
            /// <summary>
            /// Upcasts whatever this control is to a VirindiView HudControl
            /// </summary>
            HudControl AsHudControl { get; }

            /// <summary>
            /// Gets the corresponding "mirrored" copy of this control.
            /// Accessing Mirror on a mainform button will return the one on popout window, and vice-versa.
            /// </summary>
            IChaosHudControl Mirror { get; }


            // common HudControl properties
            bool Visible { get; set; }
            string Text { get; set; }
        }

        public class ChaosHudButton : HudButton, IChaosHudControl
        {
            public HudControl AsHudControl { get { return this; } }
            public string Command;
            public string Param;

            public ChaosHudButton MirrorButton = null;
            public IChaosHudControl Mirror { get { return MirrorButton; } }

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
            public HudControl AsHudControl { get { return this; } }
            public string Command;
            public string Param;

            public ChaosHudImageButton MirrorImageButton = null;
            public IChaosHudControl Mirror { get { return MirrorImageButton; } }

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
            public HudControl AsHudControl { get { return this; } }

            public ChaosHudStaticText MirrorStaticText = null;
            public IChaosHudControl Mirror { get { return MirrorStaticText; } }

            public ChaosHudStaticText(string _Text)
            {
                Text = _Text;
            }
        }

        public class ChaosHudCheckBox : HudCheckBox, IChaosHudControl
        {
            public HudControl AsHudControl { get { return this; } }
            public string OnCommand;
            public string OffCommand;

            public ChaosHudCheckBox MirrorCheckBox = null;
            public IChaosHudControl Mirror { get { return MirrorCheckBox; } }

            public ChaosHudCheckBox(string _Text, string _OnCommand, string _OffCommand)
            {
                Text = _Text;
                OnCommand = _OnCommand;
                OffCommand = _OffCommand;

                Change += ChaosHudCheckBox_Change;
            }

            private bool IgnoreChange = false;
            private void ChaosHudCheckBox_Change(object sender, EventArgs e)
            {
                if (IgnoreChange)
                    return;

                if (Checked)
                {
                    PluginCore.DispatchCommand(OnCommand, null);
                }
                else
                {
                    PluginCore.DispatchCommand(OffCommand, null);
                }

                
                // update our mirror, but without dispatching a duplicate command
                if(MirrorCheckBox != null)
                {
                    MirrorCheckBox.IgnoreChange = true;
                    MirrorCheckBox.Checked = Checked;
                    MirrorCheckBox.IgnoreChange = false;
                }
            }
        }


    }
}
