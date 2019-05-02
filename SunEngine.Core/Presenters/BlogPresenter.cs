using System;
using System.Linq;
using System.Threading.Tasks;
using SunEngine.Core.DataBase;
using SunEngine.Core.Services;
using SunEngine.Core.Utils.PagedList;

namespace SunEngine.Core.Presenters
{
    public interface IBlogPresenter
    {
        Task<IPagedList<PostView>> GetPostsAsync(int categoryId, int page, int pageSize);

        Task<IPagedList<PostView>>
            GetPostsFromMultiCategoriesAsync(int[] categoriesIds, int page, int pageSize);
    }

    public class BlogPresenter : DbService, IBlogPresenter
    {
        public BlogPresenter(DataBaseConnection db) : base(db)
        {
        }

        public virtual Task<IPagedList<PostView>> GetPostsAsync(int categoryId, int page, int pageSize)
        {
            return db.MaterialsNotDeleted.GetPagedListAsync(
                x => new PostView
                {
                    Id = x.Id,
                    Title = x.Title,
                    Preview = x.Preview,
                    CommentsCount = x.CommentsCount,
                    AuthorName = x.Author.UserName,
                    AuthorLink = x.Author.Link,
                    AuthorAvatar = x.Author.Avatar,
                    PublishDate = x.PublishDate,
                    CategoryName = x.Category.NameNormalized,
                    HasMoreText = x.Text.Length != x.Preview.Length
                },
                x => x.CategoryId == categoryId,
                x => x.OrderByDescending(y => y.PublishDate),
                page,
                pageSize);
        }
        
        public virtual Task<IPagedList<PostView>> GetPostsFromMultiCategoriesAsync(int[] categoriesIds, int page, int pageSize)
        {
            return db.MaterialsNotDeleted.GetPagedListAsync(
                x => new PostView
                {
                    Id = x.Id,
                    Title = x.Title,
                    Preview = x.Preview,
                    CommentsCount = x.CommentsCount,
                    AuthorName = x.Author.UserName,
                    AuthorLink = x.Author.Link,
                    AuthorAvatar = x.Author.Avatar,
                    PublishDate = x.PublishDate,
                    CategoryName = x.Category.NameNormalized,
                    CategoryTitle = x.Category.Title,
                    HasMoreText = x.Text.Length != x.Preview.Length
                },
                x => categoriesIds.Contains(x.CategoryId),
                x => x.OrderByDescending(y => y.PublishDate),
                page,
                pageSize);
        }
    }

    public class PostView
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public string AuthorLink { get; set; }
        public string AuthorAvatar { get; set; }
        public string Preview { get; set; }
        public int CommentsCount { get; set; }
        public DateTime PublishDate { get; set; }
        public string CategoryName { get; set; }
        public string CategoryTitle { get; set; }
        public bool HasMoreText { get; set; }
    }
}