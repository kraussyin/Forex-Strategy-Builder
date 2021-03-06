//==============================================================
// Forex Strategy Builder
// Copyright � Miroslav Popov. All rights reserved.
//==============================================================
// THIS CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE.
//==============================================================

using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using ForexStrategyBuilder.CustomControls;
using ForexStrategyBuilder.Indicators;
using ForexStrategyBuilder.Properties;

namespace ForexStrategyBuilder
{
    /// <summary>
    ///     Represents the strategies slots into a readable form
    /// </summary>
    public class StrategyLayout : Panel
    {
        private const int Border = 2;
        private const int Space = 4;
        private readonly Button btnClosingFilterHelp;
        private readonly FlowLayoutPanel pnlStrategy;
        private readonly VScrollBar scrollBar;
        private int slots;
        private Strategy strategy;

        /// <summary>
        ///     Initializes the strategy field
        /// </summary>
        public StrategyLayout(Strategy strategy)
        {
            SlotPropertiesTipText = Language.T("Averaging, Trading size, Protection.");
            SlotToolTipText = Language.T("Long position logic.");
            ShowRemoveSlotButtons = true;
            ShowAddSlotButtons = true;
            this.strategy = strategy;
            slots = strategy.Slots;
            SlotMinMidMax = SlotSizeMinMidMax.mid;
            pnlStrategy = new FlowLayoutPanel();
            scrollBar = new VScrollBar();
            SlotPanelsList = new ContextPanel[slots];
            PanelProperties = new Panel();

            for (int slot = 0; slot < slots; slot++)
                SlotPanelsList[slot] = new ContextPanel();

            // FlowLayoutStrategy
            pnlStrategy.Parent = this;
            pnlStrategy.AutoScroll = false;

            //VScrollBarStrategy
            scrollBar.Parent = this;
            scrollBar.TabStop = true;
            scrollBar.Scroll += VScrollBarStrategyScroll;

            if (ShowAddSlotButtons)
            {
                var toolTip = new ToolTip();
                ButtonAddOpenFilter = new Button
                    {
                        Tag = strategy.OpenSlot,
                        Text = Language.T("Add an Opening Logic Condition"),
                        Margin = new Padding(30, 0, 0, Space),
                        UseVisualStyleBackColor = true
                    };
                toolTip.SetToolTip(ButtonAddOpenFilter, Language.T("Add a new entry logic slot to the strategy."));

                ButtonAddCloseFilter = new Button
                    {
                        Tag = strategy.CloseSlot,
                        Text = Language.T("Add a Closing Logic Condition"),
                        Margin = new Padding(30, 0, 0, Space),
                        UseVisualStyleBackColor = true
                    };
                toolTip.SetToolTip(ButtonAddCloseFilter, Language.T("Add a new exit logic slot to the strategy."));

                btnClosingFilterHelp = new Button
                    {
                        Image = Resources.info,
                        Margin = new Padding(2, 2, 0, Space),
                        TabStop = false
                    };
                btnClosingFilterHelp.Click += BtnClosingFilterHelpClick;
                btnClosingFilterHelp.UseVisualStyleBackColor = true;
            }
        }

        public Button ButtonAddCloseFilter { get; private set; }
        public Button ButtonAddOpenFilter { get; private set; }
        public Panel PanelProperties { get; private set; }

        /// <summary>
        ///     Sets the size of the strategy's slots
        /// </summary>
        public SlotSizeMinMidMax SlotMinMidMax { get; set; }

        /// <summary>
        ///     Show Add Slot Buttons
        /// </summary>
        public bool ShowAddSlotButtons { private get; set; }

        /// <summary>
        ///     Show Remove Slot Buttons
        /// </summary>
        public bool ShowRemoveSlotButtons { private get; set; }

        /// <summary>
        ///     Show padlock image.
        /// </summary>
        public bool ShowPadlockImg { private get; set; }

        /// <summary>
        ///     Sets the tool tip text
        /// </summary>
        public string SlotToolTipText { private get; set; }

        /// <summary>
        ///     Sets the tool tip text
        /// </summary>
        public string SlotPropertiesTipText { private get; set; }

        public ContextPanel[] SlotPanelsList { get; private set; }

        /// <summary>
        ///     Initializes the strategy slots
        /// </summary>
        private void InitializeStrategySlots()
        {
            SlotPanelsList = new ContextPanel[slots];
            var toolTip = new ToolTip();

            // Strategy properties panel
            PanelProperties = new Panel {Cursor = Cursors.Hand, Tag = 100, Margin = new Padding(0, 0, 0, Space)};
            PanelProperties.Paint += PnlPropertiesPaint;
            PanelProperties.Resize += PnlSlotResize;
            toolTip.SetToolTip(PanelProperties, SlotPropertiesTipText);


            // Slot panels settings
            for (int slot = 0; slot < slots; slot++)
            {
                SlotPanelsList[slot] = new ContextPanel
                    {
                        Cursor = Cursors.Hand,
                        Tag = slot,
                        Margin = new Padding(0, 0, 0, Space)
                    };
                SlotPanelsList[slot].Paint += PnlSlotPaint;
                SlotPanelsList[slot].Resize += PnlSlotResize;
                toolTip.SetToolTip(SlotPanelsList[slot], SlotToolTipText);

                if (ShowRemoveSlotButtons && slot != strategy.OpenSlot && slot != strategy.CloseSlot)
                {
                    SlotPanelsList[slot].CloseButton.Tag = slot;
                    toolTip.SetToolTip(SlotPanelsList[slot].CloseButton, Language.T("Discard the logic condition."));
                }
            }

            // Ads the controls to the flow layout
            pnlStrategy.Controls.Add(PanelProperties);
            for (int slot = 0; slot < slots; slot++)
            {
                if (ShowAddSlotButtons && slot == strategy.CloseSlot)
                    pnlStrategy.Controls.Add(ButtonAddOpenFilter);
                pnlStrategy.Controls.Add(SlotPanelsList[slot]);
            }
            if (ShowAddSlotButtons)
            {
                pnlStrategy.Controls.Add(ButtonAddCloseFilter);
                pnlStrategy.Controls.Add(btnClosingFilterHelp);
            }
        }

        /// <summary>
        ///     Calculates the position of the controls
        /// </summary>
        private void ArrangeStrategyControls()
        {
            int width = ClientSize.Width;
            int height = ClientSize.Height;
            int totalHeight = PnlSlotCalculateTotalHeight(width);
            if (totalHeight < height)
            {
                scrollBar.Enabled = false;
                scrollBar.Visible = false;
            }
            else
            {
                width = ClientSize.Width - scrollBar.Width;
                totalHeight = PnlSlotCalculateTotalHeight(width);
                scrollBar.Enabled = true;
                scrollBar.Visible = true;
                scrollBar.Value = 0;
                scrollBar.SmallChange = 100;
                scrollBar.LargeChange = 200;
                scrollBar.Maximum = Math.Max(totalHeight - height + 220, 0);
                scrollBar.Location = new Point(width, 0);
                scrollBar.Height = height;
            }

            pnlStrategy.Location = Point.Empty;
            pnlStrategy.Width = width;
            pnlStrategy.Height = totalHeight;

            // Strategy properties panel size
            int pnlPropertiesWidth = pnlStrategy.ClientSize.Width;
            int pnlPropertiesHeight = PnlPropertiesCalculateHeight();
            PanelProperties.Size = new Size(pnlPropertiesWidth, pnlPropertiesHeight);

            // Sets the strategy slots size
            for (int slot = 0; slot < slots; slot++)
            {
                int slotWidth = pnlStrategy.ClientSize.Width;
                int slotHeight = PnlSlotCalculateHeight(slot, slotWidth);
                SlotPanelsList[slot].Size = new Size(slotWidth, slotHeight);
            }

            if (ShowAddSlotButtons)
            {
                int buttonWidth = pnlStrategy.ClientSize.Width - 60;
                var buttonHeight = (int) (Font.Height*1.7);
                ButtonAddOpenFilter.Size = new Size(buttonWidth, buttonHeight);

                btnClosingFilterHelp.Size = new Size(buttonHeight - 4, buttonHeight - 4);
                ButtonAddCloseFilter.Size = new Size(buttonWidth, buttonHeight);
            }
        }

        /// <summary>
        ///     Sets add new slot buttons
        /// </summary>
        private void SetAddSlotButtons()
        {
            // Shows or not btnAddOpenFilter
            ButtonAddOpenFilter.Enabled = strategy.OpenFilters < Strategy.MaxOpenFilters;

            bool isClosingFiltersAllowed =
                IndicatorManager.ClosingIndicatorsWithClosingFilters.Contains(
                    strategy.Slot[strategy.CloseSlot].IndicatorName);

            // Shows or not btnAddCloseFilter
            ButtonAddCloseFilter.Enabled = (strategy.CloseFilters < Strategy.MaxCloseFilters && isClosingFiltersAllowed);

            // Shows or not btnClosingFilterHelp
            btnClosingFilterHelp.Visible = !isClosingFiltersAllowed;
        }

        /// <summary>
        ///     The Scrolling moves the flowLayout
        /// </summary>
        private void VScrollBarStrategyScroll(object sender, ScrollEventArgs e)
        {
            var scroll = (VScrollBar) sender;
            pnlStrategy.Location = new Point(0, -scroll.Value);
        }

        /// <summary>
        ///     Rebuilds all the controls in panel Strategy
        /// </summary>
        public void RebuildStrategyControls(Strategy newStrategy)
        {
            strategy = newStrategy;
            slots = newStrategy.Slots;
            pnlStrategy.SuspendLayout();
            pnlStrategy.Controls.Clear();
            InitializeStrategySlots();
            ArrangeStrategyControls();
            if (ShowAddSlotButtons) SetAddSlotButtons();
            pnlStrategy.ResumeLayout();
        }

        /// <summary>
        ///     Rearranges all controls in panel Strategy
        /// </summary>
        public void RearrangeStrategyControls()
        {
            pnlStrategy.SuspendLayout();
            ArrangeStrategyControls();
            pnlStrategy.ResumeLayout();
        }

        /// <summary>
        ///     Repaints the strategy slots
        /// </summary>
        public void RepaintStrategyControls(Strategy newStrategy)
        {
            strategy = newStrategy;
            slots = strategy.Slots;
            pnlStrategy.SuspendLayout();
            foreach (ContextPanel pnl in SlotPanelsList)
                pnl.Invalidate();
            PanelProperties.Invalidate();
            pnlStrategy.ResumeLayout();
        }

        /// <summary>
        ///     Panel Strategy Resize
        /// </summary>
        private void PnlSlotResize(object sender, EventArgs e)
        {
            var pnl = (Panel) sender;
            pnl.Invalidate();
        }

        /// <summary>
        ///     Calculates the height of the Panel Slot
        /// </summary>
        /// <param name="slot">The slot number</param>
        /// <param name="width">The panel width</param>
        /// <returns>The required height</returns>
        private int PnlSlotCalculateHeight(int slot, int width)
        {
            var fontCaption = new Font(Font.FontFamily, 9f);
            int vPosition = Math.Max(fontCaption.Height, 18) + 3;

            var fontIndicator = new Font(Font.FontFamily, 11f);
            vPosition += fontIndicator.Height;

            if (SlotMinMidMax == SlotSizeMinMidMax.min)
                return vPosition + 5;

            // Calculate the height of Logic string
            if (strategy.Slot[slot].IndParam.ListParam[0].Enabled)
            {
                Graphics g = CreateGraphics();
                const float padding = Space;

                var stringFormat = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center,
                        Trimming = StringTrimming.EllipsisCharacter,
                        FormatFlags = StringFormatFlags.NoClip
                    };

                string value = strategy.Slot[slot].IndParam.ListParam[0].Text;
                var fontLogic = new Font(Font.FontFamily, 10.5f, FontStyle.Regular);
                SizeF sizeValue = g.MeasureString(value, fontLogic, (int) (width - 2*padding), stringFormat);
                vPosition += (int) sizeValue.Height;
                g.Dispose();
            }

            if (SlotMinMidMax == SlotSizeMinMidMax.mid)
                return vPosition + 6;

            var fontParam = new Font(Font.FontFamily, 9f, FontStyle.Regular);

            // List Parameters
            for (int i = 1; i < 5; i++)
                vPosition += strategy.Slot[slot].IndParam.ListParam[i].Enabled ? fontParam.Height : 0;

            // NumericParam
            foreach (NumericParam nump in strategy.Slot[slot].IndParam.NumParam)
                vPosition += nump.Enabled ? fontParam.Height : 0;

            // CheckParam
            foreach (CheckParam check in strategy.Slot[slot].IndParam.CheckParam)
                vPosition += check.Enabled ? fontParam.Height : 0;

            vPosition += 10;

            return vPosition;
        }

        /// <summary>
        ///     Calculates the height of the Averaging Panel
        /// </summary>
        /// <returns>The required height</returns>
        private int PnlPropertiesCalculateHeight()
        {
            var fontCaption = new Font(Font.FontFamily, 9f);
            int vPosition = Math.Max(fontCaption.Height, 18) + 3;

            var fontAveraging = new Font(Font.FontFamily, 9f);

            if (SlotMinMidMax == SlotSizeMinMidMax.min)
                vPosition += fontAveraging.Height;
            else
                vPosition += 5*fontAveraging.Height + 5;

            return vPosition + 8;
        }

        /// <summary>
        ///     Calculates the total height of the Panel Slot
        /// </summary>
        private int PnlSlotCalculateTotalHeight(int width)
        {
            int totalHeight = 0;

            for (int slot = 0; slot < slots; slot++)
                totalHeight += Space + PnlSlotCalculateHeight(slot, width);

            if (ShowAddSlotButtons)
                totalHeight += 2*ButtonAddCloseFilter.Height + Space;

            totalHeight += Space + PnlPropertiesCalculateHeight();

            return totalHeight;
        }

        /// <summary>
        ///     Panel Slot Paint
        /// </summary>
        private void PnlSlotPaint(object sender, PaintEventArgs e)
        {
            var pnl = (ContextPanel) sender;
            Graphics g = e.Graphics;
            var slot = (int) pnl.Tag;
            int width = pnl.ClientSize.Width;
            SlotTypes slotType = strategy.GetSlotType(slot);

            Color colorBackground = LayoutColors.ColorSlotBackground;
            Color colorCaptionText = LayoutColors.ColorSlotCaptionText;
            Color colorCaptionBackOpen = LayoutColors.ColorSlotCaptionBackOpen;
            Color colorCaptionBackOpenFilter = LayoutColors.ColorSlotCaptionBackOpenFilter;
            Color colorCaptionBackClose = LayoutColors.ColorSlotCaptionBackClose;
            Color colorCaptionBackCloseFilter = LayoutColors.ColorSlotCaptionBackCloseFilter;
            Color colorIndicatorNameText = LayoutColors.ColorSlotIndicatorText;
            Color colorLogicText = LayoutColors.ColorSlotLogicText;
            Color colorParamText = LayoutColors.ColorSlotParamText;
            Color colorValueText = LayoutColors.ColorSlotValueText;
            Color colorDash = LayoutColors.ColorSlotDash;

            // Caption
            string stringCaptionText = string.Empty;
            Color colorCaptionBack = LayoutColors.ColorSignalRed;

            switch (slotType)
            {
                case SlotTypes.Open:
                    stringCaptionText = Language.T("Opening Point of the Position");
                    colorCaptionBack = colorCaptionBackOpen;
                    break;
                case SlotTypes.OpenFilter:
                    stringCaptionText = Language.T("Opening Logic Condition");
                    colorCaptionBack = colorCaptionBackOpenFilter;
                    break;
                case SlotTypes.Close:
                    stringCaptionText = Language.T("Closing Point of the Position");
                    colorCaptionBack = colorCaptionBackClose;
                    break;
                case SlotTypes.CloseFilter:
                    stringCaptionText = Language.T("Closing Logic Condition");
                    colorCaptionBack = colorCaptionBackCloseFilter;
                    break;
            }

            var penBorder = new Pen(Data.GetGradientColor(colorCaptionBack, -LayoutColors.DepthCaption), Border);

            var fontCaptionText = new Font(Font.FontFamily, 9);
            float captionHeight = Math.Max(fontCaptionText.Height, 18);
            float captionWidth = width;
            Brush brushCaptionText = new SolidBrush(colorCaptionText);
            var stringFormatCaption = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoWrap,
                    Alignment = StringAlignment.Center
                };

            var rectCaption = new RectangleF(0, 0, captionWidth, captionHeight);
            Data.GradientPaint(g, rectCaption, colorCaptionBack, LayoutColors.DepthCaption);

            if (ShowRemoveSlotButtons && slot != strategy.OpenSlot && slot != strategy.CloseSlot)
            {
                int buttonDimentions = (int) captionHeight - 2;
                int buttonX = width - buttonDimentions - 1;
                float captionTextWidth = g.MeasureString(stringCaptionText, fontCaptionText).Width;
                float captionTextX = Math.Max((captionWidth - captionTextWidth)/2f, 0);
                var pfCaptionText = new PointF(captionTextX, 0);
                var sfCaptionText = new SizeF(buttonX - captionTextX, captionHeight);
                rectCaption = new RectangleF(pfCaptionText, sfCaptionText);
                stringFormatCaption.Alignment = StringAlignment.Near;
            }
            g.DrawString(stringCaptionText, fontCaptionText, brushCaptionText, rectCaption, stringFormatCaption);

            // Border
            g.DrawLine(penBorder, 1, captionHeight, 1, pnl.Height);
            g.DrawLine(penBorder, pnl.Width - Border + 1, captionHeight, pnl.Width - Border + 1, pnl.Height);
            g.DrawLine(penBorder, 0, pnl.Height - Border + 1, pnl.Width, pnl.Height - Border + 1);

            // Paints the panel
            var rectPanel = new RectangleF(Border, captionHeight, pnl.Width - 2*Border,
                                           pnl.Height - captionHeight - Border);
            Data.GradientPaint(g, rectPanel, colorBackground, LayoutColors.DepthControl);

            int vPosition = (int) captionHeight + 3;

            // Padlock image
            if (ShowPadlockImg)
            {
                if (strategy.Slot[slot].SlotStatus == StrategySlotStatus.Locked)
                    g.DrawImage(Resources.padlock_img, 1, 1, 16, 16);
                else if (strategy.Slot[slot].SlotStatus == StrategySlotStatus.Open)
                    g.DrawImage(Resources.open_padlock, 1, 1, 16, 16);
                else if (strategy.Slot[slot].SlotStatus == StrategySlotStatus.Linked)
                    g.DrawImage(Resources.linked, 1, 1, 16, 16);
            }

            // Indicator name
            var stringFormatIndicatorName = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    FormatFlags = StringFormatFlags.NoWrap
                };

            var fontIndicator = new Font(Font.FontFamily, 11f, FontStyle.Regular);
            Brush brushIndName = new SolidBrush(colorIndicatorNameText);
            float indNameHeight = fontIndicator.Height;
            float fGroupWidth = 0;
            if (Configs.UseLogicalGroups && (slotType == SlotTypes.OpenFilter || slotType == SlotTypes.CloseFilter))
            {
                string sLogicalGroup = "[" + strategy.Slot[slot].LogicalGroup + "]";
                fGroupWidth = g.MeasureString(sLogicalGroup, fontIndicator).Width;
                var rectGroup = new RectangleF(0, vPosition, fGroupWidth, indNameHeight);
                g.DrawString(sLogicalGroup, fontIndicator, brushIndName, rectGroup, stringFormatIndicatorName);
            }
            stringFormatIndicatorName.Trimming = StringTrimming.EllipsisCharacter;
            string indicatorName = strategy.Slot[slot].IndicatorName;
            float nameWidth = g.MeasureString(indicatorName, fontIndicator).Width;

            RectangleF rectIndName = width >= 2*fGroupWidth + nameWidth
                                         ? new RectangleF(0, vPosition, width, indNameHeight)
                                         : new RectangleF(fGroupWidth, vPosition, width - fGroupWidth, indNameHeight);

            g.DrawString(indicatorName, fontIndicator, brushIndName, rectIndName, stringFormatIndicatorName);
            vPosition += (int) indNameHeight;


            if (slotType == SlotTypes.OpenFilter || slotType == SlotTypes.CloseFilter)
            {
                pnl.CloseButton.ColorBack = colorCaptionBack;
                pnl.CloseButton.ColorFore = colorCaptionText;
                pnl.CloseButton.Visible = ShowRemoveSlotButtons;
            }

            if (SlotMinMidMax == SlotSizeMinMidMax.min)
                return;

            // Logic
            var stringFormatLogic = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoClip
                };
            float padding = Space;

            if (strategy.Slot[slot].IndParam.ListParam[0].Enabled)
            {
                string value = strategy.Slot[slot].IndParam.ListParam[0].Text;
                var fontLogic = new Font(Font.FontFamily, 10.5f, FontStyle.Regular);
                SizeF sizeValue = g.MeasureString(value, fontLogic, (int) (width - 2*padding), stringFormatLogic);
                var rectValue = new RectangleF(padding, vPosition, width - 2*padding, sizeValue.Height);
                Brush brushLogic = new SolidBrush(colorLogicText);

                g.DrawString(value, fontLogic, brushLogic, rectValue, stringFormatLogic);
                vPosition += (int) sizeValue.Height;
            }

            if (SlotMinMidMax == SlotSizeMinMidMax.mid)
                return;

            // Parameters
            var stringFormat = new StringFormat
                {
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoWrap
                };
            var fontParam = new Font(Font.FontFamily, 9f, FontStyle.Regular);
            var fontValue = new Font(Font.FontFamily, 9f, FontStyle.Regular);
            Brush brushParam = new SolidBrush(colorParamText);
            Brush brushValue = new SolidBrush(colorValueText);
            var penDash = new Pen(colorDash);

            // Find Maximum width of the strings
            float maxParamWidth = 0;
            float maxValueWidth = 0;

            for (int i = 1; i < 5; i++)
            {
                if (!strategy.Slot[slot].IndParam.ListParam[i].Enabled)
                    continue;

                string caption = strategy.Slot[slot].IndParam.ListParam[i].Caption;
                string value = strategy.Slot[slot].IndParam.ListParam[i].Text;
                SizeF sizeParam = g.MeasureString(caption, fontParam);
                SizeF sizeValue = g.MeasureString(value, fontValue);

                if (maxParamWidth < sizeParam.Width)
                    maxParamWidth = sizeParam.Width;

                if (maxValueWidth < sizeValue.Width)
                    maxValueWidth = sizeValue.Width;
            }

            foreach (NumericParam numericParam in strategy.Slot[slot].IndParam.NumParam)
            {
                if (!numericParam.Enabled) continue;

                string caption = numericParam.Caption;
                string value = numericParam.ValueToString;
                SizeF sizeParam = g.MeasureString(caption, fontParam);
                SizeF sizeValue = g.MeasureString(value, fontValue);

                if (maxParamWidth < sizeParam.Width)
                    maxParamWidth = sizeParam.Width;

                if (maxValueWidth < sizeValue.Width)
                    maxValueWidth = sizeValue.Width;
            }

            foreach (CheckParam checkParam in strategy.Slot[slot].IndParam.CheckParam)
            {
                if (!checkParam.Enabled) continue;

                string param = checkParam.Caption;
                string value = checkParam.Checked ? "Yes" : "No";
                SizeF sizeParam = g.MeasureString(param, fontParam);
                SizeF sizeValue = g.MeasureString(value, fontValue);

                if (maxParamWidth < sizeParam.Width)
                    maxParamWidth = sizeParam.Width;

                if (maxValueWidth < sizeValue.Width)
                    maxValueWidth = sizeValue.Width;
            }

            // Padding Parameter Padding Dash Padding Value Padding
            const float dashWidth = 5;
            float necessaryWidth = 4*padding + maxParamWidth + maxValueWidth + dashWidth;

            padding = width > necessaryWidth
                          ? Math.Max((pnl.ClientSize.Width - maxParamWidth - maxValueWidth - dashWidth)/6, padding)
                          : 2;

            float tabParam = 2*padding;
            float tabDash = tabParam + maxParamWidth + padding;
            float tabValue = tabDash + dashWidth + padding;

            // List Parameters
            for (int i = 1; i < 5; i++)
            {
                if (!strategy.Slot[slot].IndParam.ListParam[i].Enabled)
                    continue;

                string caption = strategy.Slot[slot].IndParam.ListParam[i].Caption;
                string value = strategy.Slot[slot].IndParam.ListParam[i].Text;
                var pointParam = new PointF(tabParam, vPosition);
                var pointDash1 = new PointF(tabDash, vPosition + fontParam.Height/2 + 2);
                var pointDash2 = new PointF(tabDash + dashWidth, vPosition + fontParam.Height/2 + 2);
                var pointValue = new PointF(tabValue, vPosition);
                var sizeValue = new SizeF(Math.Max(width - tabValue, 0), fontValue.Height + 2);
                var rectValue = new RectangleF(pointValue, sizeValue);

                g.DrawString(caption, fontParam, brushParam, pointParam);
                g.DrawLine(penDash, pointDash1, pointDash2);
                g.DrawString(value, fontValue, brushValue, rectValue, stringFormat);
                vPosition += fontValue.Height;
            }

            // Num Parameters
            foreach (NumericParam numericParam in strategy.Slot[slot].IndParam.NumParam)
            {
                if (!numericParam.Enabled) continue;

                string caption = numericParam.Caption;
                string value = numericParam.ValueToString;
                var pointParam = new PointF(tabParam, vPosition);
                var pointDash1 = new PointF(tabDash, vPosition + fontParam.Height/2 + 2);
                var pointDash2 = new PointF(tabDash + dashWidth, vPosition + fontParam.Height/2 + 2);
                var pointValue = new PointF(tabValue, vPosition);
                var sizeValue = new SizeF(Math.Max(width - tabValue, 0), fontValue.Height + 2);
                var rectValue = new RectangleF(pointValue, sizeValue);

                g.DrawString(caption, fontParam, brushParam, pointParam);
                g.DrawLine(penDash, pointDash1, pointDash2);
                g.DrawString(value, fontValue, brushValue, rectValue, stringFormat);
                vPosition += fontValue.Height;
            }

            // Check Parameters
            foreach (CheckParam checkParam in strategy.Slot[slot].IndParam.CheckParam)
            {
                if (!checkParam.Enabled) continue;

                string param = checkParam.Caption;
                string value = checkParam.Checked ? "Yes" : "No";
                var pointParam = new PointF(tabParam, vPosition);
                var pointDash1 = new PointF(tabDash, vPosition + fontParam.Height/2 + 2);
                var pointDash2 = new PointF(tabDash + dashWidth, vPosition + fontParam.Height/2 + 2);
                var pointValue = new PointF(tabValue, vPosition);
                var sizeValue = new SizeF(Math.Max(width - tabValue, 0), fontValue.Height + 2);
                var rectValue = new RectangleF(pointValue, sizeValue);

                g.DrawString(param, fontParam, brushParam, pointParam);
                g.DrawLine(penDash, pointDash1, pointDash2);
                g.DrawString(value, fontValue, brushValue, rectValue, stringFormat);
                vPosition += fontValue.Height;
            }
        }

        /// <summary>
        ///     Panel properties Paint
        /// </summary>
        private void PnlPropertiesPaint(object sender, PaintEventArgs e)
        {
            var pnl = (Panel) sender;
            Graphics g = e.Graphics;
            int width = pnl.ClientSize.Width;

            Color colorCaptionBack = LayoutColors.ColorSlotCaptionBackAveraging;
            Color colorCaptionText = LayoutColors.ColorSlotCaptionText;
            Color colorBackground = LayoutColors.ColorSlotBackground;
            Color colorLogicText = LayoutColors.ColorSlotLogicText;
            Color colorDash = LayoutColors.ColorSlotDash;

            // Caption
            string stringCaptionText = Language.T("Strategy Properties");
            var fontCaptionText = new Font(Font.FontFamily, 9);
            float captionHeight = Math.Max(fontCaptionText.Height, 18);
            float captionWidth = width;
            Brush brushCaptionText = new SolidBrush(colorCaptionText);
            var stringFormatCaption = new StringFormat
                {
                    LineAlignment = StringAlignment.Center,
                    Trimming = StringTrimming.EllipsisCharacter,
                    FormatFlags = StringFormatFlags.NoWrap,
                    Alignment = StringAlignment.Center
                };

            var rectCaption = new RectangleF(0, 0, captionWidth, captionHeight);
            Data.GradientPaint(g, rectCaption, colorCaptionBack, LayoutColors.DepthCaption);
            g.DrawString(stringCaptionText, fontCaptionText, brushCaptionText, rectCaption, stringFormatCaption);

            // Border
            var penBorder = new Pen(Data.GetGradientColor(colorCaptionBack, -LayoutColors.DepthCaption), Border);
            g.DrawLine(penBorder, 1, captionHeight, 1, pnl.Height);
            g.DrawLine(penBorder, pnl.Width - Border + 1, captionHeight, pnl.Width - Border + 1, pnl.Height);
            g.DrawLine(penBorder, 0, pnl.Height - Border + 1, pnl.Width, pnl.Height - Border + 1);

            // Paint the panel's background
            var rectPanel = new RectangleF(Border, captionHeight, pnl.Width - 2*Border,
                                           pnl.Height - captionHeight - Border);
            Data.GradientPaint(g, rectPanel, colorBackground, LayoutColors.DepthControl);

            int vPosition = (int) captionHeight + 2;

            // Padlock image
            if (ShowPadlockImg)
            {
                if (strategy.PropertiesStatus == StrategySlotStatus.Locked)
                    g.DrawImage(Resources.padlock_img, 1, 1, 16, 16);
                else if (strategy.PropertiesStatus == StrategySlotStatus.Open)
                    g.DrawImage(Resources.open_padlock, 1, 1, 16, 16);
                else if (strategy.PropertiesStatus == StrategySlotStatus.Linked)
                    g.DrawImage(Resources.linked, 1, 1, 16, 16);
            }

            var stringFormat = new StringFormat
                {Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap};
            var fontParam = new Font(Font.FontFamily, 9f, FontStyle.Regular);
            var fontValue = new Font(Font.FontFamily, 9f, FontStyle.Regular);
            Brush brushParam = new SolidBrush(colorLogicText);
            Brush brushValue = new SolidBrush(colorLogicText);
            var penDash = new Pen(colorDash);

            string strPermaSL = strategy.UsePermanentSL
                                    ? (Data.Strategy.PermanentSLType == PermanentProtectionType.Absolute ? "(Abs) " : "") +
                                      strategy.PermanentSL.ToString(CultureInfo.InvariantCulture)
                                    : Language.T("None");
            string strPermaTP = strategy.UsePermanentTP
                                    ? (Data.Strategy.PermanentTPType == PermanentProtectionType.Absolute ? "(Abs) " : "") +
                                      strategy.PermanentTP.ToString(CultureInfo.InvariantCulture)
                                    : Language.T("None");
            string strBreakEven = strategy.UseBreakEven
                                      ? strategy.BreakEven.ToString(CultureInfo.InvariantCulture)
                                      : Language.T("None");

            if (SlotMinMidMax == SlotSizeMinMidMax.min)
            {
                string param = Language.T(strategy.SameSignalAction.ToString()) + "; " +
                               Language.T(strategy.OppSignalAction.ToString()) + "; " +
                               "SL-" + strPermaSL + "; " +
                               "TP-" + strPermaTP + "; " +
                               "BE-" + strBreakEven;

                SizeF sizeParam = g.MeasureString(param, fontParam);
                float maxParamWidth = sizeParam.Width;

                // Padding Param Padding Dash Padding Value Padding
                float padding = Space;
                float necessaryWidth = 2*padding + maxParamWidth;
                padding = width > necessaryWidth ? Math.Max((pnl.ClientSize.Width - maxParamWidth)/2, padding) : 2;
                float tabParam = padding;

                var pointParam = new PointF(tabParam, vPosition);
                g.DrawString(param, fontParam, brushParam, pointParam);
            }
            else
            {
                // Find Maximum width of the strings
                var asParams = new[]
                    {
                        Language.T("Same direction signal"),
                        Language.T("Opposite direction signal"),
                        Language.T("Permanent Stop Loss"),
                        Language.T("Permanent Take Profit"),
                        Language.T("Break Even")
                    };

                var asValues = new[]
                    {
                        Language.T(strategy.SameSignalAction.ToString()),
                        Language.T(strategy.OppSignalAction.ToString()),
                        strPermaSL,
                        strPermaTP,
                        strBreakEven
                    };

                float maxParamWidth = 0;
                foreach (string param in asParams)
                {
                    if (g.MeasureString(param, fontParam).Width > maxParamWidth)
                        maxParamWidth = g.MeasureString(param, fontParam).Width;
                }

                float maxValueWidth = 0;
                foreach (string value in asValues)
                {
                    if (g.MeasureString(value, fontParam).Width > maxValueWidth)
                        maxValueWidth = g.MeasureString(value, fontParam).Width;
                }

                // Padding Param Padding Dash Padding Value Padding
                float padding = Space;
                const float dashWidth = 5;
                float necessaryWidth = 4*padding + maxParamWidth + maxValueWidth + dashWidth;
                padding = width > necessaryWidth
                              ? Math.Max((pnl.ClientSize.Width - maxParamWidth - maxValueWidth - dashWidth)/6, padding)
                              : 2;
                float tabParam = 2*padding;
                float tabDash = tabParam + maxParamWidth + padding;
                float tabValue = tabDash + dashWidth + padding;

                // Same direction
                string parameter = Language.T("Same direction signal");
                string text = Language.T(strategy.SameSignalAction.ToString());
                var pointParam = new PointF(tabParam, vPosition);
                var pointDash1 = new PointF(tabDash, vPosition + fontParam.Height/2 + 2);
                var pointDash2 = new PointF(tabDash + dashWidth, vPosition + fontParam.Height/2 + 2);
                var pointValue = new PointF(tabValue, vPosition);
                var sizeValue = new SizeF(Math.Max(width - tabValue, 0), fontValue.Height + 2);
                var rectValue = new RectangleF(pointValue, sizeValue);
                g.DrawString(parameter, fontParam, brushParam, pointParam);
                g.DrawLine(penDash, pointDash1, pointDash2);
                g.DrawString(text, fontValue, brushValue, rectValue, stringFormat);
                vPosition += fontValue.Height + 2;

                // Opposite direction
                parameter = Language.T("Opposite direction signal");
                text = Language.T(strategy.OppSignalAction.ToString());
                pointParam = new PointF(tabParam, vPosition);
                pointDash1 = new PointF(tabDash, vPosition + fontParam.Height/2 + 2);
                pointDash2 = new PointF(tabDash + dashWidth, vPosition + fontParam.Height/2 + 2);
                pointValue = new PointF(tabValue, vPosition);
                sizeValue = new SizeF(Math.Max(width - tabValue, 0), fontValue.Height + 2);
                rectValue = new RectangleF(pointValue, sizeValue);
                g.DrawString(parameter, fontParam, brushParam, pointParam);
                g.DrawLine(penDash, pointDash1, pointDash2);
                g.DrawString(text, fontValue, brushValue, rectValue, stringFormat);
                vPosition += fontValue.Height + 2;

                // Permanent Stop Loss
                parameter = Language.T("Permanent Stop Loss");
                text = strPermaSL;
                pointParam = new PointF(tabParam, vPosition);
                pointDash1 = new PointF(tabDash, vPosition + fontParam.Height/2 + 2);
                pointDash2 = new PointF(tabDash + dashWidth, vPosition + fontParam.Height/2 + 2);
                pointValue = new PointF(tabValue, vPosition);
                sizeValue = new SizeF(Math.Max(width - tabValue, 0), fontValue.Height + 2);
                rectValue = new RectangleF(pointValue, sizeValue);
                g.DrawString(parameter, fontParam, brushParam, pointParam);
                g.DrawLine(penDash, pointDash1, pointDash2);
                g.DrawString(text, fontValue, brushValue, rectValue, stringFormat);
                vPosition += fontValue.Height + 2;

                // Permanent Take Profit
                parameter = Language.T("Permanent Take Profit");
                text = strPermaTP;
                pointParam = new PointF(tabParam, vPosition);
                pointDash1 = new PointF(tabDash, vPosition + fontParam.Height/2 + 2);
                pointDash2 = new PointF(tabDash + dashWidth, vPosition + fontParam.Height/2 + 2);
                pointValue = new PointF(tabValue, vPosition);
                sizeValue = new SizeF(Math.Max(width - tabValue, 0), fontValue.Height + 2);
                rectValue = new RectangleF(pointValue, sizeValue);
                g.DrawString(parameter, fontParam, brushParam, pointParam);
                g.DrawLine(penDash, pointDash1, pointDash2);
                g.DrawString(text, fontValue, brushValue, rectValue, stringFormat);
                vPosition += fontValue.Height;

                // Break Even
                parameter = Language.T("Break Even");
                text = strBreakEven;
                pointParam = new PointF(tabParam, vPosition);
                pointDash1 = new PointF(tabDash, vPosition + fontParam.Height/2 + 2);
                pointDash2 = new PointF(tabDash + dashWidth, vPosition + fontParam.Height/2 + 2);
                pointValue = new PointF(tabValue, vPosition);
                sizeValue = new SizeF(Math.Max(width - tabValue, 0), fontValue.Height + 2);
                rectValue = new RectangleF(pointValue, sizeValue);
                g.DrawString(parameter, fontParam, brushParam, pointParam);
                g.DrawLine(penDash, pointDash1, pointDash2);
                g.DrawString(text, fontValue, brushValue, rectValue, stringFormat);
            }
        }

        /// <summary>
        ///     Shows Closing Filter Help.
        /// </summary>
        private void BtnClosingFilterHelpClick(object sender, EventArgs e)
        {
            const string text =
                "You can use Closing Logic Conditions only if the Closing Point of the Position slot contains one of the following indicators:";
            string indicators = Environment.NewLine;
            foreach (string indicator in IndicatorManager.ClosingIndicatorsWithClosingFilters)
                indicators += " - " + indicator + Environment.NewLine;
            MessageBox.Show(Language.T(text) + indicators, Language.T("Closing Logic Condition"), MessageBoxButtons.OK,
                            MessageBoxIcon.Asterisk);
        }

        /// <summary>
        ///     Arranges the controls after resizing
        /// </summary>
        protected override void OnResize(EventArgs eventargs)
        {
            pnlStrategy.SuspendLayout();
            ArrangeStrategyControls();
            pnlStrategy.ResumeLayout();
        }
    }
}