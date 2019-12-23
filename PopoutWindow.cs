using System;
using System.Collections.Generic;
using VirindiViewService.Controls;

namespace ChaosHelper
{
    class PopoutWindow : IDisposable
    {
        private VirindiViewService.ViewProperties popoutproperties;
        private VirindiViewService.ControlGroup popoutcontrols;
        private VirindiViewService.HudView popoutview;

        private HudFixedLayout popoutTempLayout = new HudFixedLayout();
        private HudCheckBox thisCheckBox;
        
        private Dictionary<string, EventHandler> regEvents = new Dictionary<string, EventHandler>();

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

        public void AddButton(HudButton button, System.Drawing.Rectangle rect)
        {
            popoutTempLayout.AddControl(button, rect);
        }

        public void AddButton(HudImageButton button, System.Drawing.Rectangle rect)
        {
            popoutTempLayout.AddControl(button, rect);
        }

        public void SetEvent(string name, EventHandler e)
        {
                HudButton temp = (HudButton)popoutview[name];

                if (regEvents.ContainsKey(name))
                {
                    //Unregister the event handler
                    temp.Hit -= regEvents[name];
                    //Store the event inside the dictionary so we can unregister it later
                    regEvents[name] = e;
                }
                else
                {
                    //Replace the event
                    regEvents[name] = e;
                }

                temp.Hit += e;            
        }

        public void SetImage(string name, VirindiViewService.ACImage image)
        {
            HudButton temp = (HudButton)popoutview[name];
            temp.Image = image;
        }

        public void ChangeBtnInfo(string name, bool visibility, string btnText)
        {
            HudButton temp = (HudButton)popoutview[name];

            temp.Visible = visibility;
            temp.Text = btnText;
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
