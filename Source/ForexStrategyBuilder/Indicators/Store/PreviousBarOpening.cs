//==============================================================
// Forex Strategy Builder
// Copyright � Miroslav Popov. All rights reserved.
//==============================================================
// THIS CODE IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND,
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE.
//==============================================================

using ForexStrategyBuilder.Infrastructure.Entities;
using ForexStrategyBuilder.Infrastructure.Enums;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Indicators.Store
{
    /// <summary>
    ///     Previous Bar Opening Indicator
    /// </summary>
    public class PreviousBarOpening : Indicator
    {
        public PreviousBarOpening()
        {
            // General properties
            IndicatorName = "Previous Bar Opening";
            PossibleSlots = SlotTypes.Open | SlotTypes.OpenFilter | SlotTypes.Close | SlotTypes.CloseFilter;
        }

        /// <summary>
        ///     Sets the default indicator parameters for the designated slot type
        /// </summary>
        public override void Initialize(SlotTypes slotType)
        {
            SlotType = slotType;

            // Setting up the indicator parameters
            IndParam.IndicatorType = TypeOfIndicator.Additional;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption = "Logic";
            if (SlotType == SlotTypes.Open)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Enter the market at the previous Bar Opening"
                    };
            else if (SlotType == SlotTypes.OpenFilter)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "The bar opens above the previous Bar Opening",
                        "The bar opens below the previous Bar Opening",
                        "The position opens above the previous Bar Opening",
                        "The position opens below the previous Bar Opening"
                    };
            else if (SlotType == SlotTypes.Close)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Exit the market at the previous Bar Opening"
                    };
            else if (SlotType == SlotTypes.CloseFilter)
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "The bar closes above the previous Bar Opening",
                        "The bar closes below the previous Bar Opening"
                    };
            else
                IndParam.ListParam[0].ItemList = new[]
                    {
                        "Not Defined"
                    };
            IndParam.ListParam[0].Index = 0;
            IndParam.ListParam[0].Text = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled = true;
            IndParam.ListParam[0].ToolTip = "Logic of application of the indicator.";

            IndParam.ListParam[1].Caption = "Base price";
            IndParam.ListParam[1].ItemList = new[] {"Previous Bar Opening"};
            IndParam.ListParam[1].Index = 0;
            IndParam.ListParam[1].Text = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled = true;
            IndParam.ListParam[1].ToolTip = "Used price from the indicator.";
        }

        /// <summary>
        ///     Calculates the indicator's components
        /// </summary>
        public override void Calculate(IDataSet dataSet)
        {
            DataSet = dataSet;

            // Calculation
            var adPrevBarOpening = new double[Bars];

            const int firstBar = 1;

            for (int bar = firstBar; bar < Bars; bar++)
            {
                adPrevBarOpening[bar] = Open[bar - 1];
            }

            // Saving the components
            if (SlotType == SlotTypes.Open || SlotType == SlotTypes.Close)
            {
                Component = new IndicatorComp[1];
            }
            else
            {
                Component = new IndicatorComp[3];

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
            }

            Component[0] = new IndicatorComp
                {
                    DataType = IndComponentType.IndicatorValue,
                    CompName = "Previous Bar Opening",
                    ChartType = IndChartType.NoChart,
                    FirstBar = firstBar,
                    Value = adPrevBarOpening
                };

            // Sets the Component's type
            if (SlotType == SlotTypes.Open)
            {
                Component[0].DataType = IndComponentType.OpenPrice;
            }
            else if (SlotType == SlotTypes.OpenFilter)
            {
                Component[1].DataType = IndComponentType.AllowOpenLong;
                Component[1].CompName = "Is long entry allowed";
                Component[2].DataType = IndComponentType.AllowOpenShort;
                Component[2].CompName = "Is short entry allowed";
            }
            else if (SlotType == SlotTypes.Close)
            {
                Component[0].DataType = IndComponentType.ClosePrice;
            }
            else if (SlotType == SlotTypes.CloseFilter)
            {
                Component[1].DataType = IndComponentType.ForceCloseLong;
                Component[1].CompName = "Close out long position";
                Component[2].DataType = IndComponentType.ForceCloseShort;
                Component[2].CompName = "Close out short position";
            }

            if (SlotType == SlotTypes.OpenFilter || SlotType == SlotTypes.CloseFilter)
            {
                switch (IndParam.ListParam[0].Text)
                {
                    case "The bar opens below the previous Bar Opening":
                        BarOpensBelowIndicatorLogic(firstBar, 0, adPrevBarOpening, ref Component[1], ref Component[2]);
                        break;

                    case "The bar opens above the previous Bar Opening":
                        BarOpensAboveIndicatorLogic(firstBar, 0, adPrevBarOpening, ref Component[1], ref Component[2]);
                        break;

                    case "The position opens above the previous Bar Opening":
                        Component[0].PosPriceDependence = PositionPriceDependence.BuyHigherSellLower;
                        Component[1].DataType = IndComponentType.Other;
                        Component[2].DataType = IndComponentType.Other;
                        Component[1].ShowInDynInfo = false;
                        Component[2].ShowInDynInfo = false;
                        break;

                    case "The position opens below the previous Bar Opening":
                        Component[0].PosPriceDependence = PositionPriceDependence.BuyLowerSelHigher;
                        Component[1].DataType = IndComponentType.Other;
                        Component[2].DataType = IndComponentType.Other;
                        Component[1].ShowInDynInfo = false;
                        Component[2].ShowInDynInfo = false;
                        break;

                    case "The bar closes below the previous Bar Opening":
                        BarClosesBelowIndicatorLogic(firstBar, 0, adPrevBarOpening, ref Component[1], ref Component[2]);
                        break;

                    case "The bar closes above the previous Bar Opening":
                        BarClosesAboveIndicatorLogic(firstBar, 0, adPrevBarOpening, ref Component[1], ref Component[2]);
                        break;
                }
            }
        }

        /// <summary>
        ///     Sets the indicator logic description
        /// </summary>
        public override void SetDescription()
        {
            switch (IndParam.ListParam[0].Text)
            {
                case "Enter the market at the previous Bar Opening":
                    EntryPointLongDescription = "at the opening price of the previous bar";
                    EntryPointShortDescription = "at the opening price of the previous bar";
                    break;

                case "The position opens above the previous Bar Opening":
                    EntryFilterLongDescription = "the position opens above the opening price of the previous bar";
                    EntryFilterShortDescription = "the position opens below the opening price of the previous bar";
                    break;
                case "The position opens below the previous Bar Opening":
                    EntryFilterLongDescription = "the position opens below the opening price of the previous bar";
                    EntryFilterShortDescription = "the position opens above the opening price of the previous bar";
                    break;

                case "The bar opens above the previous Bar Opening":
                    EntryFilterLongDescription = "the bar opens above the opening price of the previous bar";
                    EntryFilterShortDescription = "the bar opens below the opening price of the previous bar";
                    break;
                case "The bar opens below the previous Bar Opening":
                    EntryFilterLongDescription = "the bar opens below the opening price of the previous bar";
                    EntryFilterShortDescription = "the bar opens above the opening price of the previous bar";
                    break;

                case "The bar closes above the previous Bar Opening":
                    ExitFilterLongDescription = "the bar closes above the opening price of the previous bar";
                    ExitFilterShortDescription = "the bar closes below the opening price of the previous bar";
                    break;
                case "The bar closes below the previous Bar Opening":
                    ExitFilterLongDescription = "the bar closes below the opening price of the previous bar";
                    ExitFilterShortDescription = "the bar closes above the opening price of the previous bar";
                    break;

                case "Exit the market at the previous Bar Opening":
                    ExitPointLongDescription = "at the opening price of the previous bar";
                    ExitPointShortDescription = "at the opening price of the previous bar";
                    break;
            }
        }

        /// <summary>
        ///     Indicator to string
        /// </summary>
        public override string ToString()
        {
            return IndicatorName;
        }
    }
}