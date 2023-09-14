using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.EMMA;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Topics;
using Nop.Data;
using Nop.Plugin.Widgets.ImprovedSearch.Models;
using Nop.Services.Localization;
using Nop.Web.Factories;
using Nop.Web.Models.Blogs;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Widgets.ImprovedSearch.Services
{
    public interface IImprovedSearchService
    {
        public Task<ImprovedSearchModel> Search(SearchModel searchModel);
    }

    public class ImprovedSearchService : IImprovedSearchService
    {
        private readonly IRepository<BlogPost> _blogPostRepository;
        private readonly IBlogModelFactory _blogModelFactory;
        private readonly CatalogSettings _catalogSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IRepository<Topic> _topicRepository;

        public ImprovedSearchService(
            IRepository<BlogPost> blogPostRepository,
            IBlogModelFactory blogModelFactory,
            CatalogSettings catalogSettings,
            ILocalizationService localizationService)
        {
            _blogPostRepository = blogPostRepository;
            _blogModelFactory = blogModelFactory;
            _catalogSettings = catalogSettings;
            _localizationService = localizationService;
        }

        public async Task<ImprovedSearchModel> Search(SearchModel searchModel)
        {
            if (string.IsNullOrWhiteSpace(searchModel?.q) || searchModel.q.Trim().Length < _catalogSettings.ProductSearchTermMinimumLength)
            {
                var improvedSearchModel = new ImprovedSearchModel();
                improvedSearchModel.BlogPostListModel.WarningMessage = string.Format(await _localizationService.GetResourceAsync("Search.SearchTermMinimumLengthIsNCharacters"),
                            _catalogSettings.ProductSearchTermMinimumLength);
                return improvedSearchModel;
            }

            var blogPosts = await _blogPostRepository.GetAllAsync(query =>
            {
                return from b in query
                       where b.Body.Contains(searchModel.q) || b.Title.Contains(searchModel.q) || b.Tags.Contains(searchModel.q)
                                   || b.BodyOverview.Contains(searchModel.q) || b.MetaTitle.Contains(searchModel.q)
                                   || b.MetaDescription.Contains(searchModel.q) || b.MetaKeywords.Contains(searchModel.q)
                       select b;
            });

            if (blogPosts.Count == 0)
            {
                return new ImprovedSearchModel()
                {
                    BlogPostListModel = new ImprovedBlogPostListModel()
                    {
                        NoResultMessage = await _localizationService.GetResourceAsync("plugins.widgets.improvedsearch.blog.noresult")
                    }
                };
            }

            var blogPostModels = new List<BlogPostModel>();
            foreach (var blogPost in blogPosts)
            {
                var blogPostModel = new BlogPostModel();
                await _blogModelFactory.PrepareBlogPostModelAsync(blogPostModel, blogPost, false);
                blogPostModels.Add(blogPostModel);
            }
            return new ImprovedSearchModel{
                BlogPostListModel = new ImprovedBlogPostListModel()
                {
                    BlogPosts = blogPostModels
                }
            };
        }
    }
}
