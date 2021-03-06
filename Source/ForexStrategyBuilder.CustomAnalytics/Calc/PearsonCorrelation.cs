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

namespace ForexStrategyBuilder.CustomAnalytics
{
    /// <summary>
    ///     Generator Custom Math
    /// </summary>
    public static partial class Calc
    {
        /// <summary>
        ///     Pearson Correlation
        /// </summary>
        public static double PearsonCorrelation(double[] p, double[] q)
        {
            double pSum = 0, qSum = 0, pSumSq = 0, qSumSq = 0, productSum = 0;
            int n = p.Length;

            for (int x = 0; x < n; x++)
            {
                double pValue = p[x];
                double qValue = q[x];

                pSum += pValue;
                qSum += qValue;
                pSumSq += pValue*pValue;
                qSumSq += qValue*qValue;
                productSum += pValue*qValue;
            }

            double numerator = productSum - ((pSum*qSum)/n);
            double denominator = Math.Sqrt((pSumSq - (pSum*pSum)/n)*(qSumSq - (qSum*qSum)/n));

            const double epsilon = 0.000001;
            double c;
            if (Math.Abs(denominator - 0) < epsilon)
                c = 0;
            else
                c = numerator/denominator;

            if (Math.Abs(c - 1) < epsilon)
                c = 0;

            return c;
        }
    }
}