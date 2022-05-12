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

            public ChaosHudToggleButton MirrorToggleButton = null;
            public IChaosHudControl Mirror { get { return MirrorToggleButton; } }

            public ChaosHudToggleButton(string _Text, string _OnCommand, string _OffCommand)
            {
                Text = _Text;
                OnCommand = _OnCommand;
                OffCommand = _OffCommand;

                Hit += ChaosHudToggleButton_Hit;
            }

            private void ChaosHudToggleButton_Hit(object sender, EventArgs e)
            {
                Util.WriteToChat("WEEE I BEEN CLICKED");
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

            public override void DrawNow(DxTexture iSavedTarget)
            {
                if (!this.CanDraw || !this.Visible)
                    return;

                Rectangle rc = ClipRegion;
                HudViewDrawStyle style = Theme;

                base.DrawNow(iSavedTarget);
                
                if (rc.Size.Width <= 4 || rc.Size.Height <= 4)
                    return;

                if (MouseButtonDown && MouseButtonDownInRect)
                {
                    /*if (ImagePressed != null)
                        ImagePressed.Draw(iSavedTarget, rc);
                    else if (this.Image != null)
                        base.DrawNow(iSavedTarget);
                    else*/ if (this.MouseOver)
                        style.FloodFill(iSavedTarget, "ButtonBackground_Down_MouseOver", rc);
                    else
                        style.FloodFill(iSavedTarget, "ButtonBackground_Down", rc);

                    style.FloodFill(iSavedTarget, "ButtonShadow", new Rectangle(rc.Left, rc.Top, 1, rc.Height));
                    style.FloodFill(iSavedTarget, "ButtonShadow", new Rectangle(rc.Left, rc.Top, rc.Width, 1));
                    style.FloodFill(iSavedTarget, "ButtonHighlight", new Rectangle(rc.Left, rc.Bottom - 1, rc.Width, 1));
                    style.FloodFill(iSavedTarget, "ButtonHighlight", new Rectangle(rc.Right - 1, rc.Top, 1, rc.Height));
                }
                else
                {
                    /*if (this.Image != null)
                        base.DrawNow(iSavedTarget);
                    else */if (this.MouseOver)
                        style.FloodFill(iSavedTarget, "ButtonBackground_MouseOver", rc);
                    else
                        style.FloodFill(iSavedTarget, "ButtonBackground", rc);

                    style.FloodFill(iSavedTarget, "ButtonHighlight", new Rectangle(rc.Left, rc.Top, 1, rc.Height));
                    style.FloodFill(iSavedTarget, "ButtonHighlight", new Rectangle(rc.Left, rc.Top, rc.Width, 1));
                    style.FloodFill(iSavedTarget, "ButtonShadow", new Rectangle(rc.Left, rc.Bottom - 1, rc.Width, 1));
                    style.FloodFill(iSavedTarget, "ButtonShadow", new Rectangle(rc.Right - 1, rc.Top, 1, rc.Height));
                }
                /*if (this.b != null)
                {
                    if (this.c.IsEmpty)
                        this.b.Draw(iSavedTarget, rc);
                    else
                        this.b.Draw(iSavedTarget, global::b.b(rc, this.c));
                }*/
                iSavedTarget.BeginText(style.GetVal<string>("DefaultTextFontFace"), (float)style.GetVal<int>("DefaultTextFontSize"), style.GetVal<int>("DefaultTextFontWeight"), false, style.GetVal<int>("DefaultTextFontShadowSize"), style.GetVal<int>("DefaultTextFontShadowAlpha"));
                iSavedTarget.WriteText(Text, style.GetColor("ButtonText"), style.GetVal<Color>("DefaultTextFontShadowColor"), WriteTextFormats.Center | WriteTextFormats.VerticalCenter, rc);
                iSavedTarget.EndText();
            }
        
        }

    }
}
