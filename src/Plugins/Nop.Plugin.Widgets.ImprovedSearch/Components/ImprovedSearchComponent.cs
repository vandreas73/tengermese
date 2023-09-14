﻿using System;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Widgets.ImprovedSearch.Services;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Blogs;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Widgets.ImprovedSearch.Components
{
    [ViewComponent(Name = "ImprovedSearch")]
    public class ImprovedSearchComponent : NopViewComponent
    {
        private readonly IImprovedSearchService _improvedSearchService;

        public ImprovedSearchComponent(IImprovedSearchService improvedSearchService)
        {
            _improvedSearchService = improvedSearchService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, SearchModel additionalData)
        {
            return View("~/Plugins/Widgets.ImprovedSearch/Views/BlogList.cshtml", new BlogPostListModel
            {
                BlogPosts = await _improvedSearchService.Search(additionalData)
            });
        }
    }
}
