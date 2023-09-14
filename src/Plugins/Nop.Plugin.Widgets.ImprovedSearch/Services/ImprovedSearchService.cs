using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Blogs;
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
        private readonly IRepository<Topic> _topicRepository;
        private readonly IBlogModelFactory _blogModelFactory;

        public ImprovedSearchService(
            IRepository<BlogPost> blogPostRepository,
            IBlogModelFactory blogModelFactory)
        {
            _blogPostRepository = blogPostRepository;
            _blogModelFactory = blogModelFactory;
        }

        public async Task<IList<BlogPostModel>> Search(SearchModel searchModel)
        {
            var blogPosts = await _blogPostRepository.GetAllAsync(query =>
            {
                return from b in query
                       where b.Body.Contains(searchModel.q)
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
