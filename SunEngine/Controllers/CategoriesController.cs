﻿using Microsoft.AspNetCore.Mvc;
using SunEngine.Managers;
using SunEngine.Presenters;
using SunEngine.Stores;

namespace SunEngine.Controllers
{
    public class CategoriesController : BaseController
    {
        private readonly CategoriesPresenter categoriesPresenter;

        public CategoriesController(
            IUserGroupStore userGroupStore,
            CategoriesPresenter categoriesPresenter,
            MyUserManager userManager) : base(userGroupStore, userManager)
        {
            this.categoriesPresenter = categoriesPresenter;
        }

        [HttpPost]
        [HttpGet] // HttpGet - For pulse and testing 
        public CategoryInfoWithAccesses GetAllCategoriesAndAccesses()
        {
            var rez = categoriesPresenter.CategoryInfoWithAccessesFromCategory(User.UserGroups);
            return rez;
        }
    }
}