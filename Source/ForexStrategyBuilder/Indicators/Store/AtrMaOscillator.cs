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
    public class AtrMaOscillator : Indicator
    {
        public AtrMaOscillator()
        {
            IndicatorName = "ATR MA Oscillator";
            PossibleSlots = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
            SeparatedChart = true;

            IndicatorAuthor = "Miroslav Popov";
            IndicatorVersion = "2.0";
            IndicatorDescription = "Bundled in FSB distribution.";
        }

        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            IndParam.IndicatorType = TypeOfIndicator.IndicatorsMA;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            IndParam.ListParam[0].ItemList = new[]
                {
                    "ATR MA Oscillator rises",
                    "ATR MA Oscillator falls",
                    "ATR MA Oscillator is higher than the zero line",
                    "ATR MA Oscillator is lower than the zero line",
                    "ATR MA Oscillator crosses the zero line upward",
                    "ATR MA Oscillator crosses the zero line downward",
                    "ATR MA Oscillator changes its direction upward",
                    "ATR MA Oscillator changes its direction downward"
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
            IndParam.ListParam[1].ToolTip = "The Moving Average method used for smoothing the ATR value.";

            IndParam.ListParam[2].Caption = "Signal line method";
            IndParam.ListParam[2].ItemList = Enum.GetNames(typeof (MAMethod));
            IndParam.ListParam[2].Index = (int) MAMethod.Exponential;
            IndParam.ListParam[2].Text = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled = true;
            IndParam.ListParam[2].ToolTip = "The Moving Average method used for smoothing the signal line.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "ATR period";
            IndParam.NumParam[0].Value = 10;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period of ATR.";

            IndParam.NumParam[1].Caption = "Signal line period";
            IndParam.NumParam[1].Value = 14;
            IndParam.NumParam[1].Min = 1;
            IndParam.NumParam[1].Max = 200;
            IndParam.NumParam[1].Enabled = true;
            IndParam.NumParam[1].ToolTip = "The period of signal line.";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";
        }

        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Reading the parameters
            var maSignalMaMethod = (MAMethod) IndParam.ListParam[2].Index;
            var period1 = (int) IndParam.NumParam[0].Value;
            var period2 = (int) IndParam.NumParam[1].Value;
            int previous = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            int firstBar = period1 + period2 + 2;
            var oscillator = new double[Bars];

// ---------------------------------------------------------
            var atr = new AverageTrueRange();
            atr.Initialize(SlotType);
            atr.IndParam.ListParam[1].Index = IndParam.ListParam[1].Index;
            atr.IndParam.NumParam[0].Value = IndParam.NumParam[0].Value;
            atr.IndParam.CheckParam[0].Checked = IndParam.CheckParam[0].Checked;
            atr.Calculate(dataSet);

            double[] indicator1 = atr.Component[0].Value;
            double[] indicator2 = MovingAverage(period2, 0, maSignalMaMethod, indicator1);
// ----------------------------------------------------------

            for (int bar = firstBar; bar < Bars; bar++)
            {
                oscillator[bar] = indicator1[bar] - indicator2[bar];
            }

            // Saving the components
            Component = new IndicatorComp[3];

            Component[0] = new IndicatorComp
                {
                    CompName = "Histogram",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Histogram,
                    FirstBar = firstBar,
                    Value = oscillator
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
                case "ATR MA Oscillator rises":
                    indLogic = IndicatorLogic.The_indicator_rises;
                    break;

                case "ATR MA Oscillator falls":
                    indLogic = IndicatorLogic.The_indicator_falls;
                    break;

                case "ATR MA Oscillator is higher than the zero line":
                    indLogic = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                    break;

                case "ATR MA Oscillator is lower than the zero line":
                    indLogic = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                    break;

                case "ATR MA Oscillator crosses the zero line upward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                    break;

                case "ATR MA Oscillator crosses the zero line downward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                    break;

                case "ATR MA Oscillator changes its direction upward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_upward;
                    break;

                case "ATR MA Oscillator changes its direction downward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_downward;
                    break;
            }

            NoDirectionOscillatorLogic(firstBar, previous, oscillator, 0, ref Component[1], indLogic);
            Component[2].Value = Component[1].Value;
        }

        public override void SetDescription()
        {
            EntryFilterLongDescription = ToString() + " ";
            EntryFilterShortDescription = ToString() + " ";
            ExitFilterLongDescription = ToString() + " ";
            ExitFilterShortDescription = ToString() + " ";

            switch (IndParam.ListParam[0].Text)
            {
                case "ATR MA Oscillator rises":
                    EntryFilterLongDescription += "rises";
                    EntryFilterShortDescription += "rises";
                    ExitFilterLongDescription += "rises";
                    ExitFilterShortDescription += "rises";
                    break;

                case "ATR MA Oscillator falls":
                    EntryFilterLongDescription += "falls";
                    EntryFilterShortDescription += "falls";
                    ExitFilterLongDescription += "falls";
                    ExitFilterShortDescription += "falls";
                    break;

                case "ATR MA Oscillator is higher than the zero line":
                    EntryFilterLongDescription += "is higher than the zero line";
                    EntryFilterShortDescription += "is higher than the zero line";
                    ExitFilterLongDescription += "is higher than the zero line";
                    ExitFilterShortDescription += "is higher than the zero line";
                    break;

                case "ATR MA Oscillator is lower than the zero line":
                    EntryFilterLongDescription += "is lower than the zero line";
                    EntryFilterShortDescription += "is lower than the zero line";
                    ExitFilterLongDescription += "is lower than the zero line";
                    ExitFilterShortDescription += "is lower than the zero line";
                    break;

                case "ATR MA Oscillator crosses the zero line upward":
                    EntryFilterLongDescription += "crosses the zero line upward";
                    EntryFilterShortDescription += "crosses the zero line upward";
                    ExitFilterLongDescription += "crosses the zero line upward";
                    ExitFilterShortDescription += "crosses the zero line upward";
                    break;

                case "ATR MA Oscillator crosses the zero line downward":
                    EntryFilterLongDescription += "crosses the zero line downward";
                    EntryFilterShortDescription += "crosses the zero line downward";
                    ExitFilterLongDescription += "crosses the zero line downward";
                    ExitFilterShortDescription += "crosses the zero line downward";
                    break;

                case "ATR MA Oscillator changes its direction upward":
                    EntryFilterLongDescription += "changes its direction upward";
                    EntryFilterShortDescription += "changes its direction upward";
                    ExitFilterLongDescription += "changes its direction upward";
                    ExitFilterShortDescription += "changes its direction upward";
                    break;

                case "ATR MA Oscillator changes its direction downward":
                    EntryFilterLongDescription += "changes its direction downward";
                    EntryFilterShortDescription += "changes its direction downward";
                    ExitFilterLongDescription += "changes its direction downward";
                    ExitFilterShortDescription += "changes its direction downward";
                    break;
            }
        }

        public override string ToString()
        {
            return IndicatorName +
                   (IndParam.CheckParam[0].Checked ? "* (" : " (") +
                   IndParam.ListParam[1].Text + ", " + // Smoothing method
                   IndParam.ListParam[2].Text + ", " + // Signal line method
                   IndParam.NumParam[0].ValueToString + ", " + // ATR period
                   IndParam.NumParam[1].ValueToString + ")"; // Signal line period
        }
    }
}