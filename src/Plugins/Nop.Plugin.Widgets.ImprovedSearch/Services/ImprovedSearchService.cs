using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Topics;
using Nop.Data;
using Nop.Web.Factories;
using Nop.Web.Models.Blogs;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Widgets.ImprovedSearch.Services
{
    public interface IImprovedSearchService
    {
        public Task<IList<BlogPostModel>> Search(SearchModel searchModel);
    }

    public class ImprovedSearchService : IImprovedSearchService
    {
        private readonly IRepository<BlogPost> _blogPostRepository;
        private readonly IBlogModelFactory _blogModelFactory;
        private readonly CatalogSettings _catalogSettings;
        private readonly IRepository<Topic> _topicRepository;

        public ImprovedSearchService(
            IRepository<BlogPost> blogPostRepository,
            IBlogModelFactory blogModelFactory,
            CatalogSettings catalogSettings)
        {
            _blogPostRepository = blogPostRepository;
            _blogModelFactory = blogModelFactory;
            _catalogSettings = catalogSettings;
        }

        public async Task<IList<BlogPostModel>> Search(SearchModel searchModel)
        {
            if (string.IsNullOrWhiteSpace(searchModel?.q))
            {
                return new List<BlogPostModel>();
            }
            if (searchModel.q.Trim().Length < _catalogSettings.ProductSearchTermMinimumLength)
            {
                return new List<BlogPostModel>();
            }

            var blogPosts = await _blogPostRepository.GetAllAsync(query =>
            {
                return from b in query
                    where b.Body.Contains(searchModel.q) || b.Title.Contains(searchModel.q) || b.Tags.Contains(searchModel.q) 
                        || b.BodyOverview.Contains(searchModel.q) || b.MetaTitle.Contains(searchModel.q) 
                        || b.MetaDescription.Contains(searchModel.q) || b.MetaKeywords.Contains(searchModel.q)
                    select b;
            });
            var blogPostModels = new List<BlogPostModel>();
            foreach (var blogPost in blogPosts)
            {
                var blogPostModel = new BlogPostModel();
                await _blogModelFactory.PrepareBlogPostModelAsync(blogPostModel, blogPost, false);
                blogPostModels.Add(blogPostModel);
            }
            return blogPostModels;
        }
    }
}
