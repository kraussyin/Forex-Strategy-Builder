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
using ForexStrategyBuilder.Infrastructure.Entities;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Indicators.Store
{
    /// <summary>
    ///     Oscillator of BBP Indicator
    /// </summary>
    public class OscillatorOfBbp : Indicator
    {
        public OscillatorOfBbp()
        {
            // General properties
            IndicatorName = "Oscillator of BBP";
            PossibleSlots = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
            SeparatedChart = true;
        }

        /// <summary>
        ///     Sets the default indicator parameters for the designated slot type.
        /// </summary>
        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            // Setting up the indicator parameters
            IndParam.IndicatorType = TypeOfIndicator.OscillatorOfIndicators;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[]
                {
                    "Oscillator rises",
                    "Oscillator falls",
                    "Oscillator is higher than the zero line",
                    "Oscillator is lower than the zero line",
                    "Oscillator crosses the zero line upward",
                    "Oscillator crosses the zero line downward",
                    "Oscillator changes its direction upward",
                    "Oscillator changes its direction downward"
                };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            IndParam.ListParam[1].Caption = "Smoothing method";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof (MAMethod));
            IndParam.ListParam[1].Index = (int) MAMethod.Simple;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "The Moving Average method used for smoothing the BBP value.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "First BBP period";
            IndParam.NumParam[0].Value = 13;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period of first BBP.";

            IndParam.NumParam[1].Caption = "Second BBP period";
            IndParam.NumParam[1].Value = 21;
            IndParam.NumParam[1].Min = 1;
            IndParam.NumParam[1].Max = 200;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "The period of second BBP.";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        /// <summary>
        ///     Calculates the indicator's components
        /// </summary>
        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            var period1 = (int) IndParam.NumParam[0].Value;
            var period2 = (int) IndParam.NumParam[1].Value;
            int prvs = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            int firstBar = period1 + period2 + 2;
            var adOscillator = new double[Bars];

// ---------------------------------------------------------
            var bbp1 = new BullsBearsPower();
            bbp1.Initialize(SlotType);

            bbp1.IndParam.ListParam[1].Index = IndParam.ListParam[1].Index;
            bbp1.IndParam.NumParam[0].Value = IndParam.NumParam[0].Value;
            bbp1.IndParam.CheckParam[0].Checked = IndParam.CheckParam[0].Checked;
            bbp1.Calculate(DataSet);

            var bbp2 = new BullsBearsPower();
            bbp2.Initialize(SlotType);

            bbp2.IndParam.ListParam[1].Index = IndParam.ListParam[1].Index;
            bbp2.IndParam.NumParam[0].Value = IndParam.NumParam[1].Value;
            bbp2.IndParam.CheckParam[0].Checked = IndParam.CheckParam[0].Checked;
            bbp2.Calculate(DataSet);

            double[] adIndicator1 = bbp1.Component[0].Value;
            double[] adIndicator2 = bbp2.Component[0].Value;
// ----------------------------------------------------------

            for (int bar = firstBar; bar < Bars; bar++)
            {
                adOscillator[bar] = adIndicator1[bar] - adIndicator2[bar];
            }

            // Saving the components
            Component = new IndicatorComp[3];

            Component[0] = new IndicatorComp
                {
                    CompName = "Histogram",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Histogram,
                    FirstBar = firstBar,
                    Value = adOscillator
                };

            Component[1] = new IndicatorComp
                {
                    ChartType = IndChartType.NoChart,
                    FirstBar = firstBar,
                    Value = new double[Bars]
                };

            Component[2] = new IndicatorComp
                {
                    ChartType = IndChartType.NoChart,
                    FirstBar = firstBar,
                    Value = new double[Bars]
                };

            // Sets the Component's type
            if (SlotType == SlotTypes.OpenFilter)
            {
                Component[1].DataType = IndComponentType.AllowOpenLong;
                Component[1].CompName = "Is long entry allowed";
                Component[2].DataType = IndComponentType.AllowOpenShort;
                Component[2].CompName = "Is short entry allowed";
            }
            else if (SlotType == SlotTypes.CloseFilter)
            {
                Component[1].DataType = IndComponentType.ForceCloseLong;
                Component[1].CompName = "Close out long position";
                Component[2].DataType = IndComponentType.ForceCloseShort;
                Component[2].CompName = "Close out short position";
            }

            // Calculation of the logic
            var indLogic = IndicatorLogic.It_does_not_act_as_a_filter;

            switch (IndParam.ListParam[0].Text)
            {
                case "Oscillator rises":
                    indLogic = IndicatorLogic.The_indicator_rises;
                    break;

                case "Oscillator falls":
                    indLogic = IndicatorLogic.The_indicator_falls;
                    break;

                case "Oscillator is higher than the zero line":
                    indLogic = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                    break;

                case "Oscillator is lower than the zero line":
                    indLogic = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                    break;

                case "Oscillator crosses the zero line upward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                    break;

                case "Oscillator crosses the zero line downward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                    break;

                case "Oscillator changes its direction upward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_upward;
                    break;

                case "Oscillator changes its direction downward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_downward;
                    break;
            }

            OscillatorLogic(firstBar, prvs, adOscillator, 0, 0, ref Component[1], ref Component[2], indLogic);
        }

        /// <summary>
        ///     Sets the indicator logic description
        /// </summary>
        public override void SetDescription()
        {
            EntryFilterLongDescription = "the " + ToString() + " ";
            EntryFilterShortDescription = "the " + ToString() + " ";
            ExitFilterLongDescription = "the " + ToString() + " ";
            ExitFilterShortDescription = "the " + ToString() + " ";

            switch (IndParam.ListParam[0].Text)
            {
                case "Oscillator rises":
                    EntryFilterLongDescription += "rises";
                    EntryFilterShortDescription += "falls";
                    ExitFilterLongDescription += "rises";
                    ExitFilterShortDescription += "falls";
                    break;

                case "Oscillator falls":
                    EntryFilterLongDescription += "falls";
                    EntryFilterShortDescription += "rises";
                    ExitFilterLongDescription += "falls";
                    ExitFilterShortDescription += "rises";
                    break;

                case "Oscillator is higher than the zero line":
                    EntryFilterLongDescription += "is higher than the zero line";
                    EntryFilterShortDescription += "is lower than the zero line";
                    ExitFilterLongDescription += "is higher than the zero line";
                    ExitFilterShortDescription += "is lower than the zero line";
                    break;

                case "Oscillator is lower than the zero line":
                    EntryFilterLongDescription += "is lower than the zero line";
                    EntryFilterShortDescription += "is higher than the zero line";
                    ExitFilterLongDescription += "is lower than the zero line";
                    ExitFilterShortDescription += "is higher than the zero line";
                    break;

                case "Oscillator crosses the zero line upward":
                    EntryFilterLongDescription += "crosses the zero line upward";
                    EntryFilterShortDescription += "crosses the zero line downward";
                    ExitFilterLongDescription += "crosses the zero line upward";
                    ExitFilterShortDescription += "crosses the zero line downward";
                    break;

                case "Oscillator crosses the zero line downward":
                    EntryFilterLongDescription += "crosses the zero line downward";
                    EntryFilterShortDescription += "crosses the zero line upward";
                    ExitFilterLongDescription += "crosses the zero line downward";
                    ExitFilterShortDescription += "crosses the zero line upward";
                    break;

                case "Oscillator changes its direction upward":
                    EntryFilterLongDescription += "changes its direction upward";
                    EntryFilterShortDescription += "changes its direction downward";
                    ExitFilterLongDescription += "changes its direction upward";
                    ExitFilterShortDescription += "changes its direction downward";
                    break;

                case "Oscillator changes its direction downward":
                    EntryFilterLongDescription += "changes its direction downward";
                    EntryFilterShortDescription += "changes its direction upward";
                    ExitFilterLongDescription += "changes its direction downward";
                    ExitFilterShortDescription += "changes its direction upward";
                    break;
            }
        }

        /// <summary>
        ///     Indicator to string
        /// </summary>
        public override string ToString()
        {
            return IndicatorName +
                   (IndParam.CheckParam[0].Checked ? "* (" : " (") +
                   IndParam.ListParam[1].Text + ", " + // Method
                   IndParam.NumParam[0].ValueToString + ", " + // Period
                   IndParam.NumParam[1].ValueToString + ")"; // Period
        }
    }
}