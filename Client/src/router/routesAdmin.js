import Admin from 'admin/Admin.vue';
import AdminPanel from 'admin/AdminPanel.vue';
import CategoriesAdmin from 'admin/CategoriesAdmin.vue';
import AddCategory from 'admin/AddEditCategory/AddCategory.vue';
import EditCategory from 'admin/AddEditCategory/EditCategory.vue';
import UserGroupsAdmin from 'admin/UserGroupsAdmin.vue';
import GroupUsers from 'admin/GroupUsers.vue';
import Groups from 'admin/Groups.vue';


import {store} from 'store';

const routes = [
  {
    name: 'Admin',
    path: '/admin',
    components: {
      default: Admin,
      navigation: null,
    }
  },
  {
    name: 'CategoriesAdmin',
    path: '/admin/CategoriesAdmin'.toLowerCase(),
    components: {
      default: CategoriesAdmin,
      navigation: AdminPanel
    }
  },
  {
    name: 'AddCategory',
    path: '/admin/AddCategory'.toLowerCase(),
    components: {
      default: AddCategory,
      navigation: AdminPanel
    }
  },
  {
    name: 'EditCategory',
    path: '/admin/EditCategory/:id'.toLowerCase(),
    components: {
      default: EditCategory,
      navigation: AdminPanel
    },
    props: {
      default: (route) => {
        return {categoryId: +route.params.id};
      },
      navigation: null
    }
  },
  {
    name: 'UserGroupsAdmin',
    path: '/admin/UserGroupsAdmin'.toLowerCase(),
    components: {
      default: UserGroupsAdmin,
      navigation: AdminPanel
    }
  },
  {
    name: 'Groups',
    path: '/admin/Groups'.toLowerCase(),
    components: {
      default: Groups,
      navigation: AdminPanel
    },
    children: [
      {
        name: 'GroupUsers',
        path: ':groupName',
        component: GroupUsers,
        props: true
      }
    ]
  }
];


for (let rote of routes) {
  if (!rote.meta) {
    rote.meta = {
      roles: ["Admin"]
    };
  }
}


export default routes;

