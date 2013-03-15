﻿//==============================================================
// Forex Strategy Builder
// Copyright © Miroslav Popov. All rights reserved.
//==============================================================
// THIS CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE.
//==============================================================

using System;
using System.Drawing;
using System.Windows.Forms;

namespace ForexStrategyBuilder
{
    internal class FancyMessageBox : Form
    {
        /// <summary>
        ///     Public Constructor
        /// </summary>
        public FancyMessageBox(string text, string title)
        {
            BoxHeight = 230;
            BoxWidth = 380;
            PanelBase = new FancyPanel();
            PanelControl = new Panel();
            HtmlViewer = new WebBrowser();
            ButtonClose = new Button();

            Text = title;
            Icon = Data.Icon;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            TopMost = true;
            AcceptButton = ButtonClose;

            PanelBase.Parent = this;

            HtmlViewer.Parent = PanelBase;
            HtmlViewer.AllowNavigation = false;
            HtmlViewer.AllowWebBrowserDrop = false;
            HtmlViewer.DocumentText = GetHTMLContent(text, title);
            HtmlViewer.Dock = DockStyle.Fill;
            HtmlViewer.TabStop = false;
            HtmlViewer.IsWebBrowserContextMenuEnabled = false;
            HtmlViewer.WebBrowserShortcutsEnabled = true;

            PanelControl.Parent = this;
            PanelControl.Dock = DockStyle.Bottom;
            PanelControl.BackColor = Color.Transparent;

            ButtonClose.Parent = PanelControl;
            ButtonClose.Text = Language.T("Close");
            ButtonClose.Name = "Close";
            ButtonClose.Click += BtnCloseClick;
            ButtonClose.UseVisualStyleBackColor = true;
        }

        private WebBrowser HtmlViewer { get; set; }
        private Button ButtonClose { get; set; }
        private FancyPanel PanelBase { get; set; }
        private Panel PanelControl { get; set; }

        public int BoxWidth { private get; set; }
        public int BoxHeight { private get; set; }

        public override sealed string Text
        {
            get { return base.Text; }
            set { base.Text = value; }
        }

        private string GetHTMLContent(string text, string title)
        {
            // Header
            string header =
                "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">";
            header += "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"en\">";
            header += "<head><meta http-equiv=\"content-type\" content=\"text/html; charset=UTF-8\" />";
            header += "<title>" + title + "</title><style>";
            header += "body {margin: 0px; font-size: 14px; background-color: #fffffd}";
            header += ".content {padding: 5px;}";
            header += ".content h1 {margin: 0.5em 0 0.2em 0; font-weight: bold; font-size: 1.1em; color: #000033;}";
            header += ".content h2 {margin: 0.5em 0 0.2em 0; font-weight: bold; font-size: 1.0em; color: #000033;}";
            header += ".content p {margin-left: 5px; color: #000033;}";
            header += "</style></head>";
            header += "<body>";
            header += "<div class=\"content\">";

            // Footer
            const string footer = "</div></body></html>";

            return header + text + footer;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Width = BoxWidth;
            Height = BoxHeight;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            var buttonHeight = (int) (Data.VerticalDlu*15.5);
            var buttonWidth = (int) (Data.HorizontalDlu*60);
            var btnVertSpace = (int) (Data.VerticalDlu*5.5);
            var btnHrzSpace = (int) (Data.HorizontalDlu*3);
            int border = btnHrzSpace;

            PanelControl.Height = buttonHeight + 2*btnVertSpace;

            PanelBase.Size = new Size(ClientSize.Width - 2*border, PanelControl.Top - border);
            PanelBase.Location = new Point(border, border);

            ButtonClose.Size = new Size(buttonWidth, buttonHeight);
            ButtonClose.Location = new Point(ClientSize.Width - ButtonClose.Width - btnHrzSpace, btnVertSpace);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Data.GradientPaint(e.Graphics, ClientRectangle, LayoutColors.ColorFormBack, LayoutColors.DepthControl);
        }

        private void BtnCloseClick(object sender, EventArgs e)
        {
            Close();
        }
    }
}