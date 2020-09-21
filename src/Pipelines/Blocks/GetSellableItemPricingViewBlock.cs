// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GetSellableItemPricingViewBlock.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2020
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Sitecore.Commerce.Core;
using Sitecore.Commerce.EntityViews;
using Sitecore.Commerce.Plugin.Catalog;
using Sitecore.Commerce.Plugin.Pricing;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Pipelines;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ajsuth.Feature.Pricing.Engine.Pipelines.Blocks
{
    /// <summary>Defines the asynchronous executing test pipeline block</summary>
    [PipelineDisplayName(Engine.PricingConstants.Pipelines.Blocks.GetSellableItemPricingView)]
    /// <seealso cref="GetListViewBlock" />
    public class GetSellableItemPricingViewBlock : GetListViewBlock
    {
        /// <summary>Initializes a new instance of the <see cref="GetSellableItemPricingViewBlock"/> class.</summary>
        /// <param name="commerceCommander">The commerce commander.</param>
        public GetSellableItemPricingViewBlock(CommerceCommander commerceCommander)
            : base(commerceCommander)
        {
        }

        /// <summary>Executes the pipeline block's code logic.</summary>
        /// <param name="enitityView">The <see cref="EntityView"/>.</param>
        /// <param name="context">The context.</param>
        /// <returns>The <see cref="EntityView"/>.</returns>
        public override async Task<EntityView> RunAsync(EntityView entityView, CommercePipelineExecutionContext context)
        {
            Condition.Requires(entityView).IsNotNull($"{Name}: The argument cannot be null");

            var viewsPolicy = context.GetPolicy<KnownCatalogViewsPolicy>();
            var request = context.CommerceContext.GetObject<EntityViewArgument>();
            var enablementPolicy = context.GetPolicy<Policies.PricingFeatureEnablementPolicy>();
            if (!enablementPolicy.PricingFromProductView
                || string.IsNullOrEmpty(request?.ViewName) ||
                (!request.ViewName.Equals(viewsPolicy.Master, StringComparison.OrdinalIgnoreCase) &&
                !request.ViewName.Equals(viewsPolicy.Variant, StringComparison.OrdinalIgnoreCase)) ||
                !string.IsNullOrWhiteSpace(request.ForAction) ||
                !(request.Entity is SellableItem))
            {
                return await Task.FromResult(entityView).ConfigureAwait(false);
            }
            
            var pricingView = entityView.ChildViews.FirstOrDefault(x => x is EntityView && ((EntityView)x).Name.Equals(viewsPolicy.SellableItemPricing, StringComparison.OrdinalIgnoreCase)) as EntityView;
            if (pricingView == null)
            {
                return await Task.FromResult(entityView).ConfigureAwait(false);
            }

            // Move List Pricing View under the Master view
            var childViews = pricingView.ChildViews.Where(x => x is EntityView);
            pricingView.ChildViews = pricingView.ChildViews.Except(childViews).ToList();
            var position = entityView.ChildViews.IndexOf(pricingView);
            entityView.ChildViews.InsertRange(++position, childViews);

            var priceCardName = pricingView.GetPropertyValue("PriceCardName") as string;

            var listName = $"{CommerceEntity.ListName<PriceBook>()}";
            await SetListMetadata(pricingView, listName, context.GetPolicy<KnownPricingActionsPolicy>().PaginatePriceBooks, context).ConfigureAwait(false);
            var entities = await GetEntities<PriceBook>(pricingView, listName, context).ConfigureAwait(false);
            foreach (var book in entities.OfType<PriceBook>())
            {
                var priceCardId = $"{book.Name}-{priceCardName}".ToEntityId<PriceCard>();
                var priceCard = await Commander.DoesEntityExists<PriceCard>(context.CommerceContext, priceCardId).ConfigureAwait(false);
                if (!priceCard)
                {
                    continue;
                }

                var bookView = new EntityView(book)
                {
                    ItemId = priceCardId,
                    Name = context.GetPolicy<Policies.KnownPricingViewsPolicy>().SellableItemToPriceCard
                };

                bookView.Properties.Add(new ViewProperty
                {
                    Name = "ItemId",
                    RawValue = priceCardId,
                    IsReadOnly = true,
                    IsHidden = true
                });
                bookView.Properties.Add(new ViewProperty
                {
                    Name = "Name",
                    RawValue = priceCardName,
                    IsReadOnly = true,
                    UiType = "EntityLink"
                });
                bookView.Properties.Add(new ViewProperty
                {
                    Name = "PriceBook",
                    RawValue = book.Name,
                    IsReadOnly = true
                });
                pricingView.ChildViews.Add(bookView);
            }

            return entityView;
        }
    }
}
