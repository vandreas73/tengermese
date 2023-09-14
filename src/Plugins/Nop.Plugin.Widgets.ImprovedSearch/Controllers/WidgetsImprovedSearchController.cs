using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Widgets.ImprovedSearch.Services;
using Nop.Web.Controllers;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Widgets.ImprovedSearch.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class WidgetsImprovedSearchController : BasePublicController
    {
        private readonly IImprovedSearchService _improvedSearchService;

        public WidgetsImprovedSearchController(IImprovedSearchService improvedSearchService)
        {
            _improvedSearchService = improvedSearchService;
        }

        [HttpGet]
        public async Task<IActionResult> Search(SearchModel searchModel)
        {
            return Ok($"Erre keresett rá: {searchModel.q}");
        }
    }
}
