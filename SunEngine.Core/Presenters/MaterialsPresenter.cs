using System;
using System.Linq;
using System.Threading.Tasks;
using LinqToDB;
using SunEngine.Core.DataBase;
using SunEngine.Core.Models.Materials;
using SunEngine.Core.Services;

namespace SunEngine.Core.Presenters
{
    public interface IMaterialsPresenter
    {
        Task<MaterialView> GetViewModelAsync(int id);
        Task<MaterialView> GetViewModelAsync(string name);
    }

    public class MaterialsPresenter : DbService, IMaterialsPresenter
    {
        public MaterialsPresenter(DataBaseConnection db) : base(db)
        {
        }

        public virtual Task<MaterialView> GetViewModelAsync(int id)
        {
            var query = db.Materials.Where(x => x.Id == id);
            return GetViewModelAsync(query);
        }

        public virtual Task<MaterialView> GetViewModelAsync(string name)
        {
            var query = db.Materials.Where(x => x.Name == name);
            return GetViewModelAsync(query);
        }

        protected virtual Task<MaterialView> GetViewModelAsync(IQueryable<Material> query)
        {
            return query.Select(x =>
                new MaterialView
                {
                    Id = x.Id,
                    Name = x.Name,
                    Title = x.Title,
                    Description = x.Description,
                    AuthorLink = x.Author.Link,
                    AuthorName = x.Author.UserName,
                    AuthorAvatar = x.Author.Avatar,
                    AuthorId = x.Author.Id,
                    PublishDate = x.PublishDate,
                    EditDate = x.EditDate,
                    CommentsCount = x.CommentsCount,
                    Text = x.Text,
                    CategoryName = x.Category.NameNormalized,
                    IsDeleted = x.IsDeleted,
                    Tags = x.TagMaterials.OrderBy(y => y.Tag.Name).Select(y => y.Tag.Name).ToArray()
                }
            ).FirstOrDefaultAsync();
        }
    }

    public class MaterialView
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Text { get; set; }
        public string AuthorName { get; set; }
        public int AuthorId { get; set; }
        public string AuthorLink { get; set; }
        public string AuthorAvatar { get; set; }
        public int CommentsCount { get; set; }
        public DateTime PublishDate { get; set; }
        public DateTime? EditDate { get; set; }
        public string CategoryName { get; set; }
        public bool IsDeleted { get; set; }
        public string[] Tags { get; set; }
    }
}