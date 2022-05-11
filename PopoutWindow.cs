using System;
using System.Collections.Generic;
using VirindiViewService.Controls;
using ChaosHelper.VirindiControlExtensions;

namespace ChaosHelper
{
    class PopoutWindow : IDisposable
    {
        private VirindiViewService.ViewProperties popoutproperties;
        private VirindiViewService.ControlGroup popoutcontrols;
        private VirindiViewService.HudView popoutview;

        private HudFixedLayout popoutTempLayout = new HudFixedLayout();
        private HudCheckBox thisCheckBox;

        private System.Drawing.Size windowsize;

        //List<HudB>

        public PopoutWindow()
        {
            VirindiViewService.XMLParsers.Decal3XMLParser parser = new VirindiViewService.XMLParsers.Decal3XMLParser();
            parser.ParseFromResource("ChaosHelper.popoutView.xml", out popoutproperties, out popoutcontrols);

            popoutview = new VirindiViewService.HudView(popoutproperties, popoutcontrols);
            popoutview.ShowInBar = false;
            popoutview.Visible = false;
            popoutview.UserMinimizable = true;

            popoutTempLayout = popoutview != null ? (HudFixedLayout)popoutview["popout"] : new HudFixedLayout();

            //popoutview.VisibleChanged += Popoutview_VisibleChanged;
        }

        private void Popoutview_VisibleChanged(object sender, EventArgs e)
        {
            if (popoutview.Visible)
                thisCheckBox.Checked = true;
            else
                thisCheckBox.Checked = false;
        }

        public void AddControl(IChaosHudControl ctrl, System.Drawing.Rectangle rect)
        {
            popoutTempLayout.AddControl(ctrl.HudControl, rect);
        }

        public void SetImage(string name, VirindiViewService.ACImage image)
        {
            IChaosHudControl ctrl = popoutview[name] as IChaosHudControl;
            if (ctrl == null)
                return;

            if(ctrl is ChaosHudButton)
                (ctrl as ChaosHudButton).Image = image;
        }

        public void ChangeControlInfo(string name, bool visibility, string text)
        {
            IChaosHudControl ctrl = popoutview[name] as IChaosHudControl;
            if (ctrl == null)
                return;

            ctrl.Visible = visibility;

            if (ctrl is ChaosHudButton)
                (ctrl as ChaosHudButton).Text = text;
            else if (ctrl is ChaosHudStaticText)
                (ctrl as ChaosHudStaticText).Text = text;
            else if (ctrl is ChaosHudCheckBox)
                (ctrl as ChaosHudCheckBox).Text = text;
        }

        public void ChangeControlCommand(string name, string command, string param)
        {
            IChaosHudControl ctrl = popoutview[name] as IChaosHudControl;
            if (ctrl == null)
                return;

            if(ctrl is ChaosHudButton)
            {
                (ctrl as ChaosHudButton).Command = command;
                (ctrl as ChaosHudButton).Param = param;
            }
        }

        public void SetWindowSize(System.Drawing.Size windowsize)
        {
            this.windowsize = windowsize;
            popoutview.ClientArea = windowsize;
        }

        public void SetWindowPos(System.Drawing.Point windowloc)
        {
            popoutview.Location = windowloc;
        }

        public bool visible { get; private set; }

        public void toggleVisibility()
        {
            popoutview.Visible = !popoutview.Visible;
            if (popoutview.Visible)
            {
                
                visible = true;

            }
        }

        public void setCheckBox(HudCheckBox checkbox)
        {
            popoutview.VisibleChanged += Popoutview_VisibleChanged;
            this.thisCheckBox = checkbox;
        }

        public VirindiViewService.HudView View { get; private set; }

        public void Dispose()
        {
            if (popoutproperties != null)
            {
                popoutproperties.Dispose();
            }

            if (popoutcontrols != null)
            {
                popoutcontrols.Dispose();
            }

            if (popoutview != null)
            {
                popoutview.Dispose();
            }
        }
    }
}
