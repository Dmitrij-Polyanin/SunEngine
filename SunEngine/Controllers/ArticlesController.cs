﻿using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SunEngine.Authorization;
using SunEngine.Commons.Models;
using SunEngine.Commons.PagedList;
using SunEngine.Commons.Services;
using SunEngine.EntityServices;
using SunEngine.Options;
using SunEngine.Stores;

namespace SunEngine.Controllers
{
    public class ArticlesController : BaseController
    {
        private readonly OperationKeysContainer OperationKeys;
        
        private readonly ArticlesOptions articlesOptions;
        private readonly ICategoriesStore categoriesStore;
        private readonly IAuthorizationService authorizationService;
        private readonly CategoriesAuthorization categoriesAuthorization;
        
        private readonly ArticlesPresenter articlesPresenter;


        public ArticlesController(
            IOptions<ArticlesOptions> articlesOptions,
            IAuthorizationService authorizationService,
            ICategoriesStore categoriesStore,
            OperationKeysContainer operationKeysContainer,
            ArticlesPresenter articlesPresenter, 
            MyUserManager userManager,
            IUserGroupStore userGroupStore) : base(userGroupStore, userManager)
        {
            this.OperationKeys = operationKeysContainer;
            
            this.articlesOptions = articlesOptions.Value;
            this.authorizationService = authorizationService;
            this.categoriesStore = categoriesStore;
            this.articlesPresenter = articlesPresenter;
        }

        [HttpPost]
        public async Task<IActionResult> GetArticles(string categoryName, int page = 1)
        {
            Category category = categoriesStore.GetCategory(categoryName);

            if (category == null)
            {
                return BadRequest();
            }

            if (!authorizationService.HasAccess(User.UserGroups, category, OperationKeys.MaterialAndMessagesRead))
            {
                return Unauthorized();
            }

            IPagedList<ArticleInfoViewModel> articles = await articlesPresenter.GetArticlesAsync(category.Id,page,articlesOptions.ArticlesCategoryPageSize);

            return Json(articles);
        }
    }
    
    public class ArticleInfoViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string AuthorName { get; set; }
        public int MessagesCount { get; set; }
        public DateTime PublishDate { get; set; }
        public string CategoryTitle { get; set; }
        public string CategoryName { get; set; }
    }
}