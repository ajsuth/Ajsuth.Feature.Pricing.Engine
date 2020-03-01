// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KnownPricingViewsPolicy.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Ajsuth.Feature.Pricing.Engine.Policies
{
    /// <summary>
    /// Defines the known pricing Views.
    /// </summary>
    public class KnownPricingViewsPolicy : Sitecore.Commerce.Plugin.Pricing.KnownPricingViewsPolicy
    {
        /// <summary>
        /// Gets or sets the sellable item to price card view name.
        /// </summary>
        public string SellableItemToPriceCard { get; set; } = nameof(SellableItemToPriceCard);
    }
}
