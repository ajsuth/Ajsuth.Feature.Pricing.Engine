// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PricingFeatureEnablementPolicy.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Ajsuth.Feature.Pricing.Engine.Policies
{
    using Sitecore.Commerce.Core;

    /// <inheritdoc />
    /// <summary>
    /// Defines the pricing feature enablement policy
    /// </summary>
    /// <seealso cref="Policy" />
    public class PricingFeatureEnablementPolicy : Policy
    {
        public bool PricingFromProductView { get; set; }
    }
}
