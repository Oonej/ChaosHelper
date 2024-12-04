using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using VirindiViewService;
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


                // synchronize Checked to mirror (ASSUMING NO LOGIC)
                if (MirrorCheckBox != null)
                    MirrorCheckBox.Checked = Checked;
            }
        }

        // custom "toggleable" button..   works like a checkbox but looks like a button.
        // the drawing code was derived from a .NET decompilation of HudButton
        public class ChaosHudToggleButton : HudControl, IChaosHudControl
        {
            public HudControl AsHudControl { get { return this; } }
            public string OnCommand;
            public string OffCommand;

            private string _Text = string.Empty;
            public string Text
            {
                get
                {
                    return _Text;
                }

                set
                {
                    _Text = value;

                    Invalidate();
                }
            }

            private string _TextAlt = null;
            public string TextAlt
            {
                get
                {
                    return _TextAlt;
                }

                set
                {
                    _TextAlt = value;

                    Invalidate();
                }
            }

            private bool _Checked = false;
            public bool Checked
            {
                get
                {
                    return _Checked;
                }

                set
                {
                    _Checked = value;

                    Invalidate();
                }
            }

            public ChaosHudToggleButton MirrorToggleButton = null;
            public IChaosHudControl Mirror { get { return MirrorToggleButton; } }

            public ChaosHudToggleButton(string _Text, string _TextAlt, string _OnCommand, string _OffCommand)
            {
                Text = _Text;
                TextAlt = _TextAlt;
                OnCommand = _OnCommand;
                OffCommand = _OffCommand;

                Hit += ChaosHudToggleButton_Hit;
            }

            private void ChaosHudToggleButton_Hit(object sender, EventArgs e)
            {
                Checked = !Checked;

                if (Checked)
                    PluginCore.DispatchCommand(OnCommand, null);
                else
                    PluginCore.DispatchCommand(OffCommand, null);


                // synchronize Checked to mirror (ASSUMING NO LOGIC)
                if(MirrorToggleButton != null)
                    MirrorToggleButton.Checked = Checked;
            }

            protected bool MouseButtonDown = false;
            protected bool MouseButtonDownInRect = false;

            public override void MouseDown(Point pt)
            {
                base.MouseDown(pt);

                if (!OnScreenNow)
                    return;

                MouseButtonDown = true;
                MouseButtonDownInRect = true;

                Invalidate();
            }

            public override void MouseUp(Point pt, Point orig)
            {
                base.MouseUp(pt, orig);

                if (!OnScreenNow)
                    return;

                MouseButtonDown = false;

                Invalidate();
            }

            public override void MouseMove(Point pt)
            {
                base.MouseMove(pt);

                if (!MouseButtonDown)
                    return;

                bool inside = ClipRegion.Contains(pt);

                if(inside != MouseButtonDownInRect)
                {
                    MouseButtonDownInRect = inside;
                    Invalidate();
                }
            }

            public string FinalText
            {
                get
                {
                    if(TextAlt != null)
                        return $"{(Checked ? TextAlt : Text)}";
                    else
                        return $"{Text}: {(Checked ? "ON" : "OFF")}";
                }
            }

            public override void DrawNow(DxTexture iSavedTarget)
            {
                if (!this.CanDraw || !this.Visible)
                    return;

                base.DrawNow(iSavedTarget);
                
                if (ClipRegion.Size.Width <= 4 || ClipRegion.Size.Height <= 4)
                    return;

                Theme.FloodFill(iSavedTarget, Checked ? "ComboBackground_Selected" : "ComboBackground_Unselected", ClipRegion);

                if (MouseButtonDown && MouseButtonDownInRect)
                {
                    Theme.FloodFill(iSavedTarget, "ButtonShadow", new Rectangle(ClipRegion.Left, ClipRegion.Top, 1, ClipRegion.Height));
                    Theme.FloodFill(iSavedTarget, "ButtonShadow", new Rectangle(ClipRegion.Left, ClipRegion.Top, ClipRegion.Width, 1));
                    Theme.FloodFill(iSavedTarget, "ButtonHighlight", new Rectangle(ClipRegion.Left, ClipRegion.Bottom - 1, ClipRegion.Width, 1));
                    Theme.FloodFill(iSavedTarget, "ButtonHighlight", new Rectangle(ClipRegion.Right - 1, ClipRegion.Top, 1, ClipRegion.Height));
                }
                else
                {
                    Theme.FloodFill(iSavedTarget, "ButtonHighlight", new Rectangle(ClipRegion.Left, ClipRegion.Top, 1, ClipRegion.Height));
                    Theme.FloodFill(iSavedTarget, "ButtonHighlight", new Rectangle(ClipRegion.Left, ClipRegion.Top, ClipRegion.Width, 1));
                    Theme.FloodFill(iSavedTarget, "ButtonShadow", new Rectangle(ClipRegion.Left, ClipRegion.Bottom - 1, ClipRegion.Width, 1));
                    Theme.FloodFill(iSavedTarget, "ButtonShadow", new Rectangle(ClipRegion.Right - 1, ClipRegion.Top, 1, ClipRegion.Height));
                }

                iSavedTarget.BeginText(Theme.GetVal<string>("DefaultTextFontFace"), (float)Theme.GetVal<int>("DefaultTextFontSize"), Theme.GetVal<int>("DefaultTextFontWeight"), false, Theme.GetVal<int>("DefaultTextFontShadowSize"), Theme.GetVal<int>("DefaultTextFontShadowAlpha"));
                iSavedTarget.WriteText(FinalText, Theme.GetColor("ButtonText"), Theme.GetVal<Color>("DefaultTextFontShadowColor"), WriteTextFormats.Center | WriteTextFormats.VerticalCenter, ClipRegion);
                iSavedTarget.EndText();
            }
        
        }

    }
}
