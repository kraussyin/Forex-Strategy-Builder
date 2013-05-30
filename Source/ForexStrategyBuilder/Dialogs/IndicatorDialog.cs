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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using ForexStrategyBuilder.Indicators;
using ForexStrategyBuilder.Properties;

namespace ForexStrategyBuilder
{
    //    ###################################################################################
    //    # +-----------+ |--------------------------- Slot Type -------------------(!)(i)| #
    //    # |           |                                                                   #
    //    # |           | |------------------------- lblIndicator ------------------------| #
    //    # |           |                                                                   #
    //    # |           | |----------------- aLblList[0] ----------------------| | Group  | #
    //    # |           | |----------------- aCbxList[0] ----------------------| |cbxGroup| #
    //    # |           |                                                                   #
    //    # |           | |--------- aLblList[1] --------| |--------- aLblList[2] --------| #
    //    # |           | |--------- aCbxList[1] --------| |--------- aCbxList[2] --------| #
    //    # |           |                                                                   #
    //    # |           | |--------- aLblList[3] --------| |--------- aLblList[4] --------| #
    //    # |           | |--------- aCbxList[3] --------| |--------- aCbxList[4] --------| #
    //    # |           |                                                                   #
    //    # |           | |aLblNumeric[0]||aNudNumeric[0]| |aLblNumeric[1]||aNudNumeric[1]| #
    //    # |           |                                                                   #
    //    # |           | |aLblNumeric[2]||aNudNumeric[2]| |aLblNumeric[3]||aNudNumeric[3]| #
    //    # |           |                                                                   #
    //    # |           | |aLblNumeric[4]||aNudNumeric[4]| |aLblNumeric[5]||aNudNumeric[5]| #
    //    # |           |                                                                   #
    //    # |           | |v|------- aChbCheck[0] -------| |v|------- aChbCheck[1] -------| #
    //    # |           |                                                                   #
    //    # |           | +---------------------------------------------------------------+ #
    //    # |           | |                  Balance / Equity Chart                       | #
    //    # |           | |                                                               | #
    //    # |           | |                                                               | #
    //    # |           | +---------------------------------------------------------------| #
    //    # |           |                                                                   #
    //    # +-----------+ [   Accept   ]    [  Default  ]     [   Help   ]   [  Cancel  ]   #
    //    #                                                                                 #
    //    ###################################################################################

    /// <summary>
    ///     Form dialog contains controls for adjusting the indicator's parameters.
    /// </summary>
    public sealed class IndicatorDialog : Form
    {
        private const int Border = 2;
        private readonly List<IndicatorSlot> closingConditions = new List<IndicatorSlot>();
        private readonly OppositeDirSignalAction oppSignalBehaviour;
        private readonly int slot;
        private readonly string slotCation;
        private readonly SlotTypes slotType;
        private readonly ToolTip toolTip = new ToolTip();
        private bool closingSlotsRemoved;
        private string description;
        private string indicatorName;
        private bool isChartRecalculation = true;
        private bool isPaint;
        private bool oppSignalSet;
        private string warningMessage = "";

        /// <summary>
        ///     Constructor
        /// </summary>
        public IndicatorDialog(int slotNumb, SlotTypes slotType, bool isDefined)
        {
            slot = slotNumb;
            this.slotType = slotType;

            if (slotType == SlotTypes.Open)
            {
                slotCation = Language.T("Opening Point of the Position");
                PnlParameters = new FancyPanel(slotCation, LayoutColors.ColorSlotCaptionBackOpen);
                PnlTreeViewBase = new FancyPanel(Language.T("Indicators"), LayoutColors.ColorSlotCaptionBackOpen);
            }
            else if (slotType == SlotTypes.OpenFilter)
            {
                slotCation = Language.T("Opening Logic Condition");
                PnlParameters = new FancyPanel(slotCation, LayoutColors.ColorSlotCaptionBackOpenFilter);
                PnlTreeViewBase = new FancyPanel(Language.T("Indicators"), LayoutColors.ColorSlotCaptionBackOpenFilter);
            }
            else if (slotType == SlotTypes.Close)
            {
                slotCation = Language.T("Closing Point of the Position");
                PnlParameters = new FancyPanel(slotCation, LayoutColors.ColorSlotCaptionBackClose);
                PnlTreeViewBase = new FancyPanel(Language.T("Indicators"), LayoutColors.ColorSlotCaptionBackClose);
            }
            else
            {
                slotCation = Language.T("Closing Logic Condition");
                PnlParameters = new FancyPanel(slotCation, LayoutColors.ColorSlotCaptionBackCloseFilter);
                PnlTreeViewBase = new FancyPanel(Language.T("Indicators"), LayoutColors.ColorSlotCaptionBackCloseFilter);
            }

            TrvIndicators = new TreeView();
            BalanceChart = new SmallBalanceChart();
            BtnAccept = new Button();
            BtnHelp = new Button();
            BtnDefault = new Button();
            BtnCancel = new Button();

            LblIndicatorInfo = new Label();
            LblIndicatorWarning = new Label();
            LblIndicator = new Label();
            LblGroup = new Label();
            CbxGroup = new ComboBox();
            ALblList = new Label[5];
            ACbxList = new ComboBox[5];
            ALblNumeric = new Label[6];
            AFancyNudNumeric = new FancyNud[6];
            AChbCheck = new CheckBox[2];

            BackColor = LayoutColors.ColorFormBack;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Icon = Data.Icon;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            AcceptButton = BtnAccept;
            CancelButton = BtnCancel;
            Text = Language.T("Logic and Parameters of the Indicators");

            // Panel TreeViewBase
            PnlTreeViewBase.Parent = this;
            PnlTreeViewBase.Padding = new Padding(Border, (int) PnlTreeViewBase.CaptionHeight, Border, Border);

            // TreeView trvIndicators
            TrvIndicators.Parent = PnlTreeViewBase;
            TrvIndicators.Dock = DockStyle.Fill;
            TrvIndicators.BackColor = LayoutColors.ColorControlBack;
            TrvIndicators.ForeColor = LayoutColors.ColorControlText;
            TrvIndicators.BorderStyle = BorderStyle.None;
            TrvIndicators.NodeMouseDoubleClick += TrvIndicatorsNodeMouseDoubleClick;
            TrvIndicators.KeyPress += TrvIndicatorsKeyPress;

            PnlParameters.Parent = this;

            // pnlSmallBalanceChart
            BalanceChart.Parent = this;

            // lblIndicatorInfo
            LblIndicatorInfo.Parent = PnlParameters;
            LblIndicatorInfo.Size = new Size(16, 16);
            LblIndicatorInfo.BackColor = Color.Transparent;
            LblIndicatorInfo.BackgroundImage = Resources.information;
            LblIndicatorInfo.Click += LblIndicatorInfoClick;
            LblIndicatorInfo.MouseEnter += LabelMouseEnter;
            LblIndicatorInfo.MouseLeave += LabelMouseLeave;

            // lblIndicatorWarning
            LblIndicatorWarning.Parent = PnlParameters;
            LblIndicatorWarning.Size = new Size(16, 16);
            LblIndicatorWarning.BackColor = Color.Transparent;
            LblIndicatorWarning.BackgroundImage = Resources.warning;
            LblIndicatorWarning.Visible = false;
            LblIndicatorWarning.Click += LblIndicatorWarningClick;
            LblIndicatorWarning.MouseEnter += LabelMouseEnter;
            LblIndicatorWarning.MouseLeave += LabelMouseLeave;

            // Label Indicator
            LblIndicator.Parent = PnlParameters;
            LblIndicator.TextAlign = ContentAlignment.MiddleCenter;
            LblIndicator.Font = new Font(Font.FontFamily, 14, FontStyle.Bold);
            LblIndicator.ForeColor = LayoutColors.ColorSlotIndicatorText;
            LblIndicator.BackColor = Color.Transparent;

            // Label ALblList[0]
            ALblList[0] = new Label
                {
                    Parent = PnlParameters,
                    TextAlign = ContentAlignment.BottomCenter,
                    ForeColor = LayoutColors.ColorControlText,
                    BackColor = Color.Transparent
                };

            // ComboBox ACbxList[0]
            ACbxList[0] = new ComboBox {Parent = PnlParameters, DropDownStyle = ComboBoxStyle.DropDownList};
            ACbxList[0].SelectedIndexChanged += ParamChanged;

            // Logical Group
            LblGroup = new Label
                {
                    Parent = PnlParameters,
                    TextAlign = ContentAlignment.BottomCenter,
                    ForeColor = LayoutColors.ColorControlText,
                    BackColor = Color.Transparent,
                    Text = Language.T("Group")
                };

            CbxGroup = new ComboBox {Parent = PnlParameters};
            if (slotType == SlotTypes.OpenFilter)
                CbxGroup.Items.AddRange(new object[] {"A", "B", "C", "D", "E", "F", "G", "H", "All"});
            if (slotType == SlotTypes.CloseFilter)
                CbxGroup.Items.AddRange(new object[] {"a", "b", "c", "d", "e", "f", "g", "h", "all"});
            CbxGroup.SelectedIndexChanged += GroupChanged;
            CbxGroup.DropDownStyle = ComboBoxStyle.DropDownList;
            toolTip.SetToolTip(CbxGroup, Language.T("The logical group of the slot."));

            // ListParams
            for (int i = 1; i < 5; i++)
            {
                ALblList[i] = new Label
                    {
                        Parent = PnlParameters,
                        TextAlign = ContentAlignment.BottomCenter,
                        ForeColor = LayoutColors.ColorControlText,
                        BackColor = Color.Transparent
                    };

                ACbxList[i] = new ComboBox
                    {
                        Parent = PnlParameters,
                        Enabled = false,
                        DropDownStyle = ComboBoxStyle.DropDownList
                    };
                ACbxList[i].SelectedIndexChanged += ParamChanged;
            }

            // NumParams
            for (int i = 0; i < 6; i++)
            {
                ALblNumeric[i] = new Label
                    {
                        Parent = PnlParameters,
                        TextAlign = ContentAlignment.MiddleRight,
                        ForeColor = LayoutColors.ColorControlText,
                        BackColor = Color.Transparent
                    };

                AFancyNudNumeric[i] = new FancyNud
                    {Parent = PnlParameters, TextAlign = HorizontalAlignment.Center, Enabled = false};
                AFancyNudNumeric[i].ValueChanged += ParamChanged;
            }

            // CheckParams
            for (int i = 0; i < 2; i++)
            {
                AChbCheck[i] = new CheckBox
                    {
                        Parent = PnlParameters,
                        CheckAlign = ContentAlignment.MiddleLeft,
                        TextAlign = ContentAlignment.MiddleLeft,
                        ForeColor = LayoutColors.ColorControlText,
                        BackColor = Color.Transparent,
                        Enabled = false
                    };
                AChbCheck[i].CheckedChanged += ParamChanged;
            }

            //Button Accept
            BtnAccept.Parent = this;
            BtnAccept.Text = Language.T("Accept");
            BtnAccept.DialogResult = DialogResult.OK;
            BtnAccept.Click += BtnOkClick;
            BtnAccept.UseVisualStyleBackColor = true;

            //Button Default
            BtnDefault.Parent = this;
            BtnDefault.Text = Language.T("Default");
            BtnDefault.Click += BtnDefaultClick;
            BtnDefault.UseVisualStyleBackColor = true;

            //Button Help
            BtnHelp.Parent = this;
            BtnHelp.Text = Language.T("Help");
            BtnHelp.Click += BtnHelpClick;
            BtnHelp.UseVisualStyleBackColor = true;

            //Button Cancel
            BtnCancel.Parent = this;
            BtnCancel.Text = Language.T("Cancel");
            BtnCancel.DialogResult = DialogResult.Cancel;
            BtnCancel.UseVisualStyleBackColor = true;

            SetTreeViewIndicators();

            // ComboBoxindicator index selection.
            if (isDefined)
            {
                TreeNode[] atrn = TrvIndicators.Nodes.Find(Data.Strategy.Slot[slot].IndParam.IndicatorName, true);
                TrvIndicators.SelectedNode = atrn[0];
                UpdateFromIndicatorParam(Data.Strategy.Slot[slot].IndParam);
                SetLogicalGroup();
                CalculateIndicator(false);
                BalanceChart.SetChartData();
                BalanceChart.InitChart();
                BalanceChart.Invalidate();
            }
            else
            {
                string defaultIndicator;
                if (slotType == SlotTypes.Open)
                    defaultIndicator = "Bar Opening";
                else if (slotType == SlotTypes.OpenFilter)
                    defaultIndicator = "Accelerator Oscillator";
                else if (slotType == SlotTypes.Close)
                    defaultIndicator = "Bar Closing";
                else
                    defaultIndicator = "Accelerator Oscillator";

                TreeNode[] atrn = TrvIndicators.Nodes.Find(defaultIndicator, true);
                TrvIndicators.SelectedNode = atrn[0];
                TrvIndicatorsLoadIndicator();
            }

            oppSignalBehaviour = Data.Strategy.OppSignalAction;

            if (slotType == SlotTypes.Close && Data.Strategy.CloseFilters > 0)
                for (int iSlot = Data.Strategy.CloseSlot + 1; iSlot < Data.Strategy.Slots; iSlot++)
                    closingConditions.Add(Data.Strategy.Slot[iSlot].Clone());
        }

        private FancyPanel PnlTreeViewBase { get; set; }
        private TreeView TrvIndicators { get; set; }
        private FancyPanel PnlParameters { get; set; }

        private Label LblIndicatorInfo { get; set; }
        private Label LblIndicatorWarning { get; set; }
        private Label LblIndicator { get; set; }
        private Label LblGroup { get; set; }
        private ComboBox CbxGroup { get; set; }
        private Label[] ALblList { get; set; }
        private ComboBox[] ACbxList { get; set; }
        private Label[] ALblNumeric { get; set; }
        private FancyNud[] AFancyNudNumeric { get; set; }
        private CheckBox[] AChbCheck { get; set; }
        private SmallBalanceChart BalanceChart { get; set; }
        private Button BtnAccept { get; set; }
        private Button BtnDefault { get; set; }
        private Button BtnHelp { get; set; }
        private Button BtnCancel { get; set; }

        /// <summary>
        ///     Gets or sets the caption of a ComboBox control.
        /// </summary>
        private Label[] ListLabel
        {
            get { return ALblList; }
        }

        /// <summary>
        ///     Gets or sets the parameters of a ComboBox control.
        /// </summary>
        private ComboBox[] ListParam
        {
            get { return ACbxList; }
        }

        /// <summary>
        ///     Gets or sets the caption of a NumericUpDown control.
        /// </summary>
        private Label[] NumLabel
        {
            get { return ALblNumeric; }
        }

        /// <summary>
        ///     Gets or sets the parameters of a NumericUpDown control.
        /// </summary>
        private FancyNud[] NumParam
        {
            get { return AFancyNudNumeric; }
        }

        /// <summary>
        ///     Gets or sets the parameters of a CheckBox control.
        /// </summary>
        private CheckBox[] CheckParam
        {
            get { return AChbCheck; }
        }

// ---------------------------------------------------------------------------

        /// <summary>
        ///     OnLoad
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var height = (int) (570*Data.VDpiScale);
            var width = (int) (670*Data.HDpiScale);

            Width = width;
            Height = height;
            MinimumSize = new Size(width, height);
        }

        /// <summary>
        ///     Recalculates the sizes and positions of the controls after resizing.
        /// </summary>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            var buttonHeight = (int) (Data.VerticalDlu*15.5);
            var buttonWidth = (int) (Data.HorizontalDlu*60);
            var btnVertSpace = (int) (Data.VerticalDlu*5.5);
            var btnHrzSpace = (int) (Data.HorizontalDlu*3);
            int space = btnHrzSpace;
            int textHeight = Font.Height;
            int controlHeight = Font.Height + 4;

            int rightColumnWight = 4*buttonWidth + 3*btnHrzSpace;
            int pnlTreeViewWidth = ClientSize.Width - rightColumnWight - 3*space;

            // Panel pnlTreeViewBase
            PnlTreeViewBase.Size = new Size(pnlTreeViewWidth, ClientSize.Height - 2*space);
            PnlTreeViewBase.Location = new Point(space, space);

            int iRightColumnLeft = pnlTreeViewWidth + 2*space;
            const int nudWidth = 65;

            // pnlParameterBase
            PnlParameters.Width = rightColumnWight;
            PnlParameters.Location = new Point(iRightColumnLeft, space);

            //Button Accept
            BtnAccept.Size = new Size(buttonWidth, buttonHeight);
            BtnAccept.Location = new Point(iRightColumnLeft, ClientSize.Height - btnVertSpace - buttonHeight);

            //Button Default
            BtnDefault.Size = BtnAccept.Size;
            BtnDefault.Location = new Point(BtnAccept.Right + btnHrzSpace, BtnAccept.Top);

            //Button Help
            BtnHelp.Size = BtnAccept.Size;
            BtnHelp.Location = new Point(BtnDefault.Right + btnHrzSpace, BtnAccept.Top);

            //Button Cancel
            BtnCancel.Size = BtnAccept.Size;
            BtnCancel.Location = new Point(BtnHelp.Right + btnHrzSpace, BtnAccept.Top);

            // lblIndicatorInfo
            LblIndicatorInfo.Location = new Point(PnlParameters.Width - LblIndicatorInfo.Width - 1, 1);

            // lblIndicatorWarning
            LblIndicatorWarning.Location = new Point(LblIndicatorInfo.Left - LblIndicatorWarning.Width - 1, 1);

            // lblIndicator
            LblIndicator.Size = new Size(PnlParameters.ClientSize.Width - 2*Border - 2*space,
                                         3*LblIndicator.Font.Height/2);
            LblIndicator.Location = new Point(Border + space, (int) PnlParameters.CaptionHeight + space);

            // Logical Group
            Graphics g = CreateGraphics();
            LblGroup.Width = (int) g.MeasureString(Language.T("Group"), Font).Width + 10;
            g.Dispose();
            LblGroup.Height = textHeight;
            LblGroup.Location = new Point(PnlParameters.ClientSize.Width - Border - space - LblGroup.Width,
                                          LblIndicator.Bottom + space);

            // ComboBox Groups
            CbxGroup.Size = new Size(LblGroup.Width, controlHeight);
            CbxGroup.Location = new Point(LblGroup.Left, ALblList[0].Bottom + space);

            int rightShift = Configs.UseLogicalGroups &&
                             (slotType == SlotTypes.OpenFilter || slotType == SlotTypes.CloseFilter)
                                 ? (LblGroup.Width + space)
                                 : 0;

            // Label Logic
            ALblList[0].Size = new Size(PnlParameters.ClientSize.Width - 2*Border - 2*space - rightShift, textHeight);
            ALblList[0].Location = new Point(Border + space, LblIndicator.Bottom + space);

            // ComboBox Logic
            ACbxList[0].Size = new Size(PnlParameters.ClientSize.Width - 2*Border - 2*space - rightShift, controlHeight);
            ACbxList[0].Location = new Point(Border + space, ALblList[0].Bottom + space);

            // ListParams
            for (int m = 0; m < 2; m++)
                for (int n = 0; n < 2; n++)
                {
                    int i = 2*m + n + 1;
                    int x = (ACbxList[1].Width + space)*n + space + Border;
                    int y = (textHeight + controlHeight + 3*space)*m + ACbxList[0].Bottom + 2*space;

                    ALblList[i].Size = new Size((PnlParameters.ClientSize.Width - 3*space - 2*Border)/2, textHeight);
                    ALblList[i].Location = new Point(x, y);

                    ACbxList[i].Size = new Size((PnlParameters.ClientSize.Width - 3*space - 2*Border)/2, controlHeight);
                    ACbxList[i].Location = new Point(x, y + textHeight + space);
                }

            // NumParams
            for (int m = 0; m < 3; m++)
                for (int n = 0; n < 2; n++)
                {
                    int i = 2*m + n;
                    int iLblWidth = (PnlParameters.ClientSize.Width - 5*space - 2*nudWidth - 2*Border)/2;
                    ALblNumeric[i].Size = new Size(iLblWidth, controlHeight);
                    ALblNumeric[i].Location = new Point((iLblWidth + nudWidth + 2*space)*n + space + Border,
                                                        (controlHeight + 2*space)*m + 2*space + ACbxList[3].Bottom);

                    AFancyNudNumeric[i].Size = new Size(nudWidth, controlHeight);
                    AFancyNudNumeric[i].Location = new Point(ALblNumeric[i].Right + space, ALblNumeric[i].Top);
                }

            // CheckParams
            for (int i = 0; i < 2; i++)
            {
                int iChbWidth = (PnlParameters.ClientSize.Width - 3*space - 2*Border)/2;
                AChbCheck[i].Size = new Size(iChbWidth, controlHeight);
                AChbCheck[i].Location = new Point((space + iChbWidth)*i + space + Border,
                                                  AFancyNudNumeric[4].Bottom + space);
            }

            PnlParameters.ClientSize = new Size(PnlParameters.ClientSize.Width, AChbCheck[0].Bottom + space + Border);

            // Panel pnlSmallBalanceChart
            BalanceChart.Size = new Size(rightColumnWight, BtnAccept.Top - PnlParameters.Bottom - space - btnVertSpace);
            BalanceChart.Location = new Point(iRightColumnLeft, PnlParameters.Bottom + space);
        }

        /// <summary>
        ///     Sets the controls' parameters.
        /// </summary>
        private void UpdateFromIndicatorParam(IndicatorParam ip)
        {
            indicatorName = ip.IndicatorName;
            LblIndicator.Text = indicatorName;

            isPaint = false;
            isChartRecalculation = false;

            // List parameters
            for (int i = 0; i < 5; i++)
            {
                ListParam[i].Items.Clear();
                foreach (string item in ip.ListParam[i].ItemList)
                    ListParam[i].Items.Add(item);
                ListLabel[i].Text = ip.ListParam[i].Caption;
                ListParam[i].SelectedIndex = ip.ListParam[i].Index;
                ListParam[i].Enabled = ip.ListParam[i].Enabled;
                toolTip.SetToolTip(ListParam[i], ip.ListParam[i].ToolTip);
            }

            // Numeric parameters
            for (int i = 0; i < 6; i++)
            {
                NumParam[i].BeginInit();
                NumLabel[i].Text = ip.NumParam[i].Caption;
                NumParam[i].Minimum = (decimal) ip.NumParam[i].Min;
                NumParam[i].Maximum = (decimal) ip.NumParam[i].Max;
                NumParam[i].Value = (decimal) ip.NumParam[i].Value;
                NumParam[i].DecimalPlaces = ip.NumParam[i].Point;
                NumParam[i].Increment = (decimal) Math.Pow(10, -ip.NumParam[i].Point);
                NumParam[i].Enabled = ip.NumParam[i].Enabled;
                NumParam[i].EndInit();
                toolTip.SetToolTip(NumParam[i],
                                   ip.NumParam[i].ToolTip + Environment.NewLine + "Minimum: " + NumParam[i].Minimum +
                                   " Maximum: " + NumParam[i].Maximum);
            }

            // Check parameters
            for (int i = 0; i < 2; i++)
            {
                CheckParam[i].Text = ip.CheckParam[i].Caption;
                CheckParam[i].Checked = ip.CheckParam[i].Checked;
                toolTip.SetToolTip(CheckParam[i], ip.CheckParam[i].ToolTip);

                if (Data.AutoUsePrvBarValue && ip.CheckParam[i].Caption == "Use previous bar value")
                    CheckParam[i].Enabled = false;
                else
                    CheckParam[i].Enabled = ip.CheckParam[i].Enabled;
            }

            isPaint = true;
            isChartRecalculation = true;
        }

        /// <summary>
        ///     Sets the logical group of the slot.
        /// </summary>
        private void SetLogicalGroup()
        {
            if (slotType != SlotTypes.OpenFilter && slotType != SlotTypes.CloseFilter) return;
            string group = Data.Strategy.Slot[slot].LogicalGroup;
            if (string.IsNullOrEmpty(@group))
                SetDefaultGroup();
            else
            {
                if (@group.ToLower() == "all")
                    CbxGroup.SelectedIndex = CbxGroup.Items.Count - 1;
                else
                    CbxGroup.SelectedIndex = char.ConvertToUtf32(@group.ToLower(), 0) - char.ConvertToUtf32("a", 0);
            }
        }

        /// <summary>
        ///     Sets the default logical group of the slot.
        /// </summary>
        private void SetDefaultGroup()
        {
            if (slotType == SlotTypes.OpenFilter)
            {
                if (indicatorName == "Data Bars Filter" ||
                    indicatorName == "Date Filter" ||
                    indicatorName == "Day of Month" ||
                    indicatorName == "Enter Once" ||
                    indicatorName == "Entry Time" ||
                    indicatorName == "Long or Short" ||
                    indicatorName == "Lot Limiter" ||
                    indicatorName == "Random Filter")
                    CbxGroup.SelectedIndex = CbxGroup.Items.Count - 1; // "All" group.
                else
                    CbxGroup.SelectedIndex = 0;
            }

            if (slotType == SlotTypes.CloseFilter)
            {
                int index = slot - Data.Strategy.CloseSlot - 1;
                CbxGroup.SelectedIndex = index;
            }
        }

        /// <summary>
        ///     Sets the trvIndicators nodes
        /// </summary>
        private void SetTreeViewIndicators()
        {
            var trnAll = new TreeNode {Name = "trnAll", Text = Language.T("All"), Tag = false};
            var trnIndicators = new TreeNode {Name = "trnIndicators", Text = Language.T("Indicators"), Tag = false};
            var trnAdditional = new TreeNode {Name = "trnAdditional", Text = Language.T("Additional"), Tag = false};

            var trnOscillatorOfIndicators = new TreeNode
                {
                    Name = "trnOscillatorOfIndicators",
                    Text = Language.T("Oscillator of Indicators"),
                    Tag = false
                };

            var trnIndicatorsMaOscillator = new TreeNode
                {
                    Name = "trnIndicatorMA",
                    Text = Language.T("Indicator's MA Oscillator"),
                    Tag = false
                };

            var trnDateTime = new TreeNode {Name = "trnDateTime", Text = Language.T("Date/Time Functions"), Tag = false};

            var trnCustomIndicators = new TreeNode
                {
                    Name = "trnCustomIndicators",
                    Text = Language.T("Custom Indicators"),
                    Tag = false
                };

            TrvIndicators.Nodes.AddRange(new[]
                {
                    trnAll, trnIndicators, trnAdditional, trnOscillatorOfIndicators,
                    trnIndicatorsMaOscillator, trnDateTime, trnCustomIndicators
                });

            foreach (string name in IndicatorManager.GetIndicatorNames(slotType))
            {
                Indicator indicator = IndicatorManager.ConstructIndicator(name);
                indicator.Initialize(slotType);
                TypeOfIndicator type = indicator.IndParam.IndicatorType;

                Color nodeColor = LayoutColors.ColorControlText;
                if (indicator.OverrideMainIndicator)
                    nodeColor = Color.Brown;

                var trn = new TreeNode {Tag = true, Name = name, Text = name, ForeColor = nodeColor};
                trnAll.Nodes.Add(trn);

                if (indicator.CustomIndicator)
                {
                    var trnCustom = new TreeNode {Tag = true, Name = name, Text = name, ForeColor = nodeColor};
                    trnCustomIndicators.Nodes.Add(trnCustom);
                }

                var trnGroups = new TreeNode {Tag = true, Name = name, Text = name, ForeColor = nodeColor};

                switch (type)
                {
                    case TypeOfIndicator.Indicator:
                        trnIndicators.Nodes.Add(trnGroups);
                        break;
                    case TypeOfIndicator.Additional:
                        trnAdditional.Nodes.Add(trnGroups);
                        break;
                    case TypeOfIndicator.OscillatorOfIndicators:
                        trnOscillatorOfIndicators.Nodes.Add(trnGroups);
                        break;
                    case TypeOfIndicator.IndicatorsMA:
                        trnIndicatorsMaOscillator.Nodes.Add(trnGroups);
                        break;
                    case TypeOfIndicator.DateTime:
                        trnDateTime.Nodes.Add(trnGroups);
                        break;
                }
            }
        }

        /// <summary>
        ///     Loads the default parameters for the chosen indicator.
        /// </summary>
        private void TrvIndicatorsNodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (!(bool) TrvIndicators.SelectedNode.Tag)
                return;

            TrvIndicatorsLoadIndicator();
        }

        /// <summary>
        ///     Loads the default parameters for the chosen indicator.
        /// </summary>
        private void TrvIndicatorsKeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != ' ')
                return;

            if (!(bool) TrvIndicators.SelectedNode.Tag)
                return;

            TrvIndicatorsLoadIndicator();
        }

        /// <summary>
        ///     Loads an Indicator
        /// </summary>
        private void TrvIndicatorsLoadIndicator()
        {
            Indicator indicator = IndicatorManager.ConstructIndicator(TrvIndicators.SelectedNode.Text);
            indicator.Initialize(slotType);
            UpdateFromIndicatorParam(indicator.IndParam);
            SetDefaultGroup();
            CalculateIndicator(true);
            UpdateBalanceChart();
        }

        /// <summary>
        ///     Loads the default parameters for the selected indicator.
        /// </summary>
        private void BtnDefaultClick(object sender, EventArgs e)
        {
            Indicator indicator = IndicatorManager.ConstructIndicator(indicatorName);
            indicator.Initialize(slotType);
            UpdateFromIndicatorParam(indicator.IndParam);
            SetDefaultGroup();
            CalculateIndicator(true);
            UpdateBalanceChart();
        }

        /// <summary>
        ///     Shows help for the selected indicator.
        /// </summary>
        private void BtnHelpClick(object sender, EventArgs e)
        {
            try
            {
                Process.Start("http://forexsb.com/wiki/indicators/start");
            }
            catch (Exception exception)
            {
                Console.WriteLine("BtnHelpClick: " + exception.Message);
            }
        }

        /// <summary>
        ///     Closes the dialog.
        /// </summary>
        private void BtnOkClick(object sender, EventArgs e)
        {
        }

        /// <summary>
        ///     Sets the slot group.
        /// </summary>
        private void GroupChanged(object sender, EventArgs e)
        {
            if (slotType == SlotTypes.OpenFilter || slotType == SlotTypes.CloseFilter)
                Data.Strategy.Slot[slot].LogicalGroup = CbxGroup.Text;

            ParamChanged(sender, e);
        }

        /// <summary>
        ///     Calculates strategy when a parameter was changed.
        /// </summary>
        private void ParamChanged(object sender, EventArgs e)
        {
            CalculateIndicator(true);
            UpdateBalanceChart();
        }

        // Updates the balance chart.
        private void UpdateBalanceChart()
        {
            if (!isChartRecalculation)
                return;

            BalanceChart.SetChartData();
            BalanceChart.InitChart();
            BalanceChart.Invalidate();
        }

        /// <summary>
        ///     Calculates the selected indicator.
        /// </summary>
        private void CalculateIndicator(bool bCalculateStrategy)
        {
            if (!Data.IsData || !Data.IsResult || !isPaint) return;

            SetOppositeSignalBehaviour();
            SetClosingLogicConditions();

            Indicator indicator = IndicatorManager.ConstructIndicator(indicatorName);
            indicator.Initialize(slotType);

            // List parameters
            for (int i = 0; i < 5; i++)
            {
                indicator.IndParam.ListParam[i].Index = ListParam[i].SelectedIndex;
                indicator.IndParam.ListParam[i].Text = ListParam[i].Text;
                indicator.IndParam.ListParam[i].Enabled = ListParam[i].Enabled;
            }

            // Numeric parameters
            for (int i = 0; i < 6; i++)
            {
                indicator.IndParam.NumParam[i].Value = (double) NumParam[i].Value;
                indicator.IndParam.NumParam[i].Enabled = NumParam[i].Enabled;
            }

            // Check parameters
            for (int i = 0; i < 2; i++)
            {
                indicator.IndParam.CheckParam[i].Checked = CheckParam[i].Checked;
                indicator.IndParam.CheckParam[i].Enabled = CheckParam[i].Enabled;
                indicator.IndParam.CheckParam[i].Enabled = CheckParam[i].Text == "Use previous bar value" ||
                                                           CheckParam[i].Enabled;
            }

            if (!CalculateIndicator(slotType, indicator))
                return;

            if (bCalculateStrategy)
            {
                //Sets Data.Strategy
                Data.Strategy.Slot[slot].IndicatorName = indicator.IndicatorName;
                Data.Strategy.Slot[slot].IndParam = indicator.IndParam;
                Data.Strategy.Slot[slot].Component = indicator.Component;
                Data.Strategy.Slot[slot].SeparatedChart = indicator.SeparatedChart;
                Data.Strategy.Slot[slot].SpecValue = indicator.SpecialValues;
                Data.Strategy.Slot[slot].MinValue = indicator.SeparatedChartMinValue;
                Data.Strategy.Slot[slot].MaxValue = indicator.SeparatedChartMaxValue;
                Data.Strategy.Slot[slot].IsDefined = true;

                // Search the indicators' components to determine Data.FirstBar
                Data.FirstBar = Data.Strategy.SetFirstBar();

                // Check "Use previous bar value"
                if (Data.Strategy.AdjustUsePreviousBarValue())
                {
                    for (int i = 0; i < 2; i++)
                        if (indicator.IndParam.CheckParam[i].Caption == "Use previous bar value")
                            AChbCheck[i].Checked = Data.Strategy.Slot[slot].IndParam.CheckParam[i].Checked;
                }

                Backtester.Calculate();
                Backtester.CalculateAccountStats();
            }

            SetIndicatorNotification(indicator);

            Data.IsResult = true;
        }

        /// <summary>
        ///     Calculates an indicator and returns OK status.
        /// </summary>
        private bool CalculateIndicator(SlotTypes type, Indicator indicator)
        {
            bool okStatus;

            try
            {
                indicator.Calculate(Data.DataSet);

                okStatus = true;
            }
            catch (Exception exception)
            {
                string request = "Please report this error in the support forum!";
                if (indicator.CustomIndicator)
                    request = "Please report this error to the author of the indicator!<br />" +
                              "You may remove this indicator from the Custom Indicators folder.";

                string text =
                    "<h1>Error: " + exception.Message + "</h1>" +
                    "<p>Slot type: <strong>" + type + "</strong><br />" +
                    "Indicator: <strong>" + indicator + "</strong></p>" +
                    "<p>" + request + "</p>";

                const string caption = "Indicator Calculation Error";
                var msgBox = new FancyMessageBox(text, caption) {BoxWidth = 450, BoxHeight = 250};
                msgBox.Show();

                okStatus = false;
            }

            return okStatus;
        }

        /// <summary>
        ///     Sets the indicator overview.
        /// </summary>
        private void SetIndicatorNotification(Indicator indicator)
        {
            // Warning message.
            warningMessage = indicator.WarningMessage;
            LblIndicatorWarning.Visible = !string.IsNullOrEmpty(warningMessage);

            // Set description.
            indicator.SetDescription();
            description = "Long position:" + Environment.NewLine;
            if (slotType == SlotTypes.Open)
            {
                description += "   Open a long position " + indicator.EntryPointLongDescription + "." +
                               Environment.NewLine + Environment.NewLine;
                description += "Short position:" + Environment.NewLine;
                description += "   Open a short position " + indicator.EntryPointShortDescription + ".";
            }
            else if (slotType == SlotTypes.OpenFilter)
            {
                description += "   Open a long position when " + indicator.EntryFilterLongDescription + "." +
                               Environment.NewLine + Environment.NewLine;
                description += "Short position:" + Environment.NewLine;
                description += "   Open a short position when " + indicator.EntryFilterShortDescription + ".";
            }
            else if (slotType == SlotTypes.Close)
            {
                description += "   Close a long position " + indicator.ExitPointLongDescription + "." +
                               Environment.NewLine + Environment.NewLine;
                description += "Short position:" + Environment.NewLine;
                description += "   Close a short position " + indicator.ExitPointShortDescription + ".";
            }
            else
            {
                description += "   Close a long position when " + indicator.ExitFilterLongDescription + "." +
                               Environment.NewLine + Environment.NewLine;
                description += "Short position:" + Environment.NewLine;
                description += "   Close a short position when " + indicator.ExitFilterShortDescription + ".";
            }

            for (int i = 0; i < 2; i++)
                if (indicator.IndParam.CheckParam[i].Caption == "Use previous bar value")
                    description += Environment.NewLine + "-------------" + Environment.NewLine + "* Use the value of " +
                                   indicator.IndicatorName + " from the previous bar.";

            toolTip.SetToolTip(LblIndicatorInfo, description);
        }

        /// <summary>
        ///     Sets or restores the closing logic conditions.
        /// </summary>
        private void SetClosingLogicConditions()
        {
            bool isClosingFiltersAllowed = IndicatorManager.ClosingIndicatorsWithClosingFilters.Contains(indicatorName);

            // Removes or recovers closing logic slots.
            if (slotType == SlotTypes.Close && !isClosingFiltersAllowed && Data.Strategy.CloseFilters > 0)
            {
                // Removes all the closing logic conditions.
                Data.Strategy.RemoveAllCloseFilters();
                closingSlotsRemoved = true;
            }
            else if (slotType == SlotTypes.Close && isClosingFiltersAllowed && closingSlotsRemoved)
            {
                foreach (IndicatorSlot inslot in closingConditions)
                {
                    // Recovers all the closing logic conditions.
                    Data.Strategy.AddCloseFilter();
                    Data.Strategy.Slot[Data.Strategy.Slots - 1] = inslot.Clone();
                }
                closingSlotsRemoved = false;
            }
        }

        /// <summary>
        ///     Sets or restores the opposite signal behavior.
        /// </summary>
        private void SetOppositeSignalBehaviour()
        {
            // Changes opposite signal behavior.
            if (slotType == SlotTypes.Close && indicatorName == "Close and Reverse" &&
                oppSignalBehaviour != OppositeDirSignalAction.Reverse)
            {
                // Sets the strategy opposite signal to Reverse.
                Data.Strategy.OppSignalAction = OppositeDirSignalAction.Reverse;
                oppSignalSet = true;
            }
            else if (slotType == SlotTypes.Close && indicatorName != "Close and Reverse" && oppSignalSet)
            {
                // Recovers the original opposite signal.
                Data.Strategy.OppSignalAction = oppSignalBehaviour;
                oppSignalSet = false;
            }
        }

        /// <summary>
        ///     Shows the indicator description
        /// </summary>
        private void LblIndicatorInfoClick(object sender, EventArgs e)
        {
            MessageBox.Show(description, slotCation, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        ///     Shows the indicator warning
        /// </summary>
        private void LblIndicatorWarningClick(object sender, EventArgs e)
        {
            MessageBox.Show(warningMessage, indicatorName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        ///     Changes the background color of a label when the mouse leaves.
        /// </summary>
        private void LabelMouseLeave(object sender, EventArgs e)
        {
            var lbl = (Label) sender;
            lbl.BackColor = Color.Transparent;
        }

        /// <summary>
        ///     Changes the background color of a label when the mouse enters.
        /// </summary>
        private void LabelMouseEnter(object sender, EventArgs e)
        {
            var lbl = (Label) sender;
            lbl.BackColor = Color.Orange;
        }
    }
}