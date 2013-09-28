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
    public class CciMAOscillator : Indicator
    {
        public CciMAOscillator()
        {
            IndicatorName = "CCI MA Oscillator";
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
                    "CCI MA Oscillator rises",
                    "CCI MA Oscillator falls",
                    "CCI MA Oscillator is higher than the zero line",
                    "CCI MA Oscillator is lower than the zero line",
                    "CCI MA Oscillator crosses the zero line upward",
                    "CCI MA Oscillator crosses the zero line downward",
                    "CCI MA Oscillator changes its direction upward",
                    "CCI MA Oscillator changes its direction downward"
                };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the oscillator.";

            IndParam.ListParam[1].Caption = "Smoothing method";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof (MAMethod));
            IndParam.ListParam[1].Index = (int) MAMethod.Simple;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "The Moving Average method used for smoothing the CCI value.";

            IndParam.ListParam[2].Caption = "Signal line method";
            IndParam.ListParam[2].ItemList = Enum.GetNames(typeof (MAMethod));
            IndParam.ListParam[2].Index = (int) MAMethod.Exponential;
            IndParam.ListParam[2].Text = IndParam.ListParam[2].ItemList[IndParam.ListParam[2].Index];
            IndParam.ListParam[2].Enabled = true;
            IndParam.ListParam[2].ToolTip = "The Moving Average method used for smoothing the signal line.";

            IndParam.ListParam[3].Caption = "Base price";
            IndParam.ListParam[3].ItemList = Enum.GetNames(typeof (BasePrice));
            IndParam.ListParam[3].Index = (int) BasePrice.Typical;
            IndParam.ListParam[3].Text = IndParam.ListParam[3].ItemList[IndParam.ListParam[3].Index];
            IndParam.ListParam[3].Enabled = true;
            IndParam.ListParam[3].ToolTip = "The base price of Commodity Channel Index.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption = "CCI period";
            IndParam.NumParam[0].Value = 14;
            IndParam.NumParam[0].Min = 1;
            IndParam.NumParam[0].Max = 200;
            IndParam.NumParam[0].Enabled = true;
            IndParam.NumParam[0].ToolTip = "The period of Commodity Channel Index.";

            IndParam.NumParam[1].Caption = "Signal line period";
            IndParam.NumParam[1].Value = 13;
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
            var maSignalMAMethod = (MAMethod) IndParam.ListParam[2].Index;
            var iPeriod1 = (int) IndParam.NumParam[0].Value;
            var iPeriod2 = (int) IndParam.NumParam[1].Value;
            int iPrvs = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            int iFirstBar = iPeriod1 + iPeriod2 + 2;
            var adOscillator = new double[Bars];

// ---------------------------------------------------------
            var cci = new CommodityChannelIndex();
            cci.Initialize(SlotType);
            cci.IndParam.ListParam[1].Index = IndParam.ListParam[1].Index;
            cci.IndParam.ListParam[2].Index = IndParam.ListParam[3].Index;
            cci.IndParam.NumParam[0].Value = IndParam.NumParam[0].Value;
            cci.IndParam.CheckParam[0].Checked = IndParam.CheckParam[0].Checked;
            cci.Calculate(DataSet);

            double[] adIndicator1 = cci.Component[0].Value;
            double[] adIndicator2 = MovingAverage(iPeriod2, 0, maSignalMAMethod, adIndicator1);
// ----------------------------------------------------------

            for (int iBar = iFirstBar; iBar < Bars; iBar++)
            {
                adOscillator[iBar] = adIndicator1[iBar] - adIndicator2[iBar];
            }

            // Saving the components
            Component = new IndicatorComp[3];

            Component[0] = new IndicatorComp
                {
                    CompName = "Histogram",
                    DataType = IndComponentType.IndicatorValue,
                    ChartType = IndChartType.Histogram,
                    FirstBar = iFirstBar,
                    Value = adOscillator
                };

            Component[1] = new IndicatorComp
                {
                    ChartType = IndChartType.NoChart,
                    FirstBar = iFirstBar,
                    Value = new double[Bars]
                };

            Component[2] = new IndicatorComp
                {
                    ChartType = IndChartType.NoChart,
                    FirstBar = iFirstBar,
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
                case "CCI MA Oscillator rises":
                    indLogic = IndicatorLogic.The_indicator_rises;
                    break;

                case "CCI MA Oscillator falls":
                    indLogic = IndicatorLogic.The_indicator_falls;
                    break;

                case "CCI MA Oscillator is higher than the zero line":
                    indLogic = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                    break;

                case "CCI MA Oscillator is lower than the zero line":
                    indLogic = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                    break;

                case "CCI MA Oscillator crosses the zero line upward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                    break;

                case "CCI MA Oscillator crosses the zero line downward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                    break;

                case "CCI MA Oscillator changes its direction upward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_upward;
                    break;

                case "CCI MA Oscillator changes its direction downward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_downward;
                    break;
            }

            OscillatorLogic(iFirstBar, iPrvs, adOscillator, 0, 0, ref Component[1], ref Component[2], indLogic);
        }

        public override void SetDescription()
        {
            EntryFilterLongDescription = "the " + ToString() + " ";
            EntryFilterShortDescription = "the " + ToString() + " ";
            ExitFilterLongDescription = "the " + ToString() + " ";
            ExitFilterShortDescription = "the " + ToString() + " ";

            switch (IndParam.ListParam[0].Text)
            {
                case "CCI MA Oscillator rises":
                    EntryFilterLongDescription += "rises";
                    EntryFilterShortDescription += "falls";
                    ExitFilterLongDescription += "rises";
                    ExitFilterShortDescription += "falls";
                    break;

                case "CCI MA Oscillator falls":
                    EntryFilterLongDescription += "falls";
                    EntryFilterShortDescription += "rises";
                    ExitFilterLongDescription += "falls";
                    ExitFilterShortDescription += "rises";
                    break;

                case "CCI MA Oscillator is higher than the zero line":
                    EntryFilterLongDescription += "is higher than the zero line";
                    EntryFilterShortDescription += "is lower than the zero line";
                    ExitFilterLongDescription += "is higher than the zero line";
                    ExitFilterShortDescription += "is lower than the zero line";
                    break;

                case "CCI MA Oscillator is lower than the zero line":
                    EntryFilterLongDescription += "is lower than the zero line";
                    EntryFilterShortDescription += "is higher than the zero line";
                    ExitFilterLongDescription += "is lower than the zero line";
                    ExitFilterShortDescription += "is higher than the zero line";
                    break;

                case "CCI MA Oscillator crosses the zero line upward":
                    EntryFilterLongDescription += "crosses the zero line upward";
                    EntryFilterShortDescription += "crosses the zero line downward";
                    ExitFilterLongDescription += "crosses the zero line upward";
                    ExitFilterShortDescription += "crosses the zero line downward";
                    break;

                case "CCI MA Oscillator crosses the zero line downward":
                    EntryFilterLongDescription += "crosses the zero line downward";
                    EntryFilterShortDescription += "crosses the zero line upward";
                    ExitFilterLongDescription += "crosses the zero line downward";
                    ExitFilterShortDescription += "crosses the zero line upward";
                    break;

                case "CCI MA Oscillator changes its direction upward":
                    EntryFilterLongDescription += "changes its direction upward";
                    EntryFilterShortDescription += "changes its direction downward";
                    ExitFilterLongDescription += "changes its direction upward";
                    ExitFilterShortDescription += "changes its direction downward";
                    break;

                case "CCI MA Oscillator changes its direction downward":
                    EntryFilterLongDescription += "changes its direction downward";
                    EntryFilterShortDescription += "changes its direction upward";
                    ExitFilterLongDescription += "changes its direction downward";
                    ExitFilterShortDescription += "changes its direction upward";
                    break;
            }
        }

        public override string ToString()
        {
            return IndicatorName +
                   (IndParam.CheckParam[0].Checked ? "* (" : " (") +
                   IndParam.ListParam[1].Text + ", " + // Method
                   IndParam.ListParam[2].Text + ", " + // Method
                   IndParam.ListParam[3].Text + ", " + // Price
                   IndParam.NumParam[0].ValueToString + ", " + // Period
                   IndParam.NumParam[1].ValueToString + ")"; // Period
        }
    }
}