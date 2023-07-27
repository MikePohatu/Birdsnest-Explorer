// Copyright (c) 2019-2023 "20Road"
// 20Road Limited [https://www.20road.com]
//
// This file is part of Birdsnest Explorer.
//
// Birdsnest Explorer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, version 3 of the License.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
import { createRouter, createWebHistory, isNavigationFailure, NavigationFailureType, RouteRecordRaw } from "vue-router";
import { store, rootPaths } from "@/store";
import { auth } from "@/assets/ts/webcrap/authcrap";
import webcrap from "@/assets/ts/webcrap/webcrap";
import { Notify } from "@/assets/ts/Notifications";

export const routeDefs = {
  portal: {
    name: "Portal",
    path: "/portal",
    meta: {
      breadcrumbs: [
        { name: "Portal" }
      ]
    },
    component: () =>
      import("@/views/Portal.vue")
  },
  about: {
    name: "About",
    path: "/about",
    meta: {
      allowAnonymous: true,
      breadcrumbs: [
        { name: "About" }
      ]
    },
    component: () =>
      import("@/views/About.vue")
  },
  docs: {
    name: "Docs",
    path: "/docs:docPath(.*)",
    rootPath: "/docs",
    meta: {
      allowAnonymous: true,
      breadcrumbs: [
        { name: "Docs", link: "/docs" }
      ]
    },
    component: () =>
      import("@/views/Docs.vue")
  },
  report: {
    name: "Report Viewer",
    path: "/report",
    meta: {
      breadcrumbs: [
        { name: "Reports", link: "/reports" },
        { name: "Report Viewer" }
      ]
    },
    component: () =>
      import("@/views/ReportView.vue")
  },
  reports: {
    name: "Reports",
    path: "/reports",
    meta: {
      breadcrumbs: [
        { name: "Reports" }
      ]
    },
    component: () =>
      import("@/views/Reports.vue")
  },
  visualizer: {
    name: "Visualizer",
    path: "/visualizer",
    meta: {
      breadcrumbs: [
        { name: "Visualizer" }
      ]
    },
    component: () =>
      import("@/views/Visualizer.vue")
  },
  admin: {
    name: "Admin",
    path: "/admin",
    meta: {
      breadcrumbs: [
        { name: "Admin" }
      ]
    },
    component: () =>
      import("@/views/Admin.vue")
  },
  login: {
    name: "Login",
    path: "/login",
    meta: {
      allowAnonymous: true,
      breadcrumbs: [
        { name: "Login" }
      ]
    },
    component: () =>
      import("@/views/Login.vue")
  },
  info: {
    name: "Server Information",
    path: "/info",
    meta: {
      breadcrumbs: [
        { name: "Server Information" }
      ]
    },
    component: () =>
      import("@/views/ServerInfoView.vue")
  },
  indexEditor: {
    name: "Index Editor",
    path: "/indexeditor",
    meta: {
      breadcrumbs: [
        { name: "Admin", link: "/admin" },
        { name: "Index Editor" }
      ]
    },
    component: () =>
      import("@/views/IndexEditorView.vue")
  },
  error: {
    name: "Error",
    path: "/error/:code",
    meta: {
      allowAnonymous: true
    },
    component: () =>
      import("@/views/Error.vue")
  }
}

const routes: Array<RouteRecordRaw> = [
  {
    path: '/',
    redirect: routeDefs.portal.path
  },
  routeDefs.portal,
  routeDefs.about,
  routeDefs.report,
  routeDefs.reports,
  routeDefs.admin,
  routeDefs.login,
  routeDefs.visualizer,
  routeDefs.info,
  routeDefs.indexEditor,
  routeDefs.docs,
  routeDefs.error
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes
});

router.beforeEach((to, from, next) => {
  if (to.meta.allowAnonymous === true) {
    if (to.name === routeDefs.docs.name && webcrap.misc.isIE() === false) {
      const path = to.path.split("#")[0];

      if (path === routeDefs.docs.rootPath) {
        next({
          path: "/docs/static/documentation/README.md"
        });
      } else if (path.endsWith("/")) {
        next({
          path: path + "README.md"
        });
      } else if (path.endsWith(".md") === false) {
        next({
          path: path + "/README.md"
        });
      } else {
        to.meta.pagecrumbs = [];

        if (to.path !== routeDefs.docs.path) {
          const pathbits = to.path.replace("/docs/static/documentation/", "").split("/");
          let currentpath = "";

          pathbits.forEach((bit: string) => {
            currentpath = currentpath + "/" + bit;
            to.meta.pagecrumbs.push({
              name: webcrap.misc.capitalize(bit),
              link: "/docs/static/documentation" + currentpath,
            });
          });
        }
        auth.softPing(); // do softPing after redirects 
        next();
      }
    } else {
      auth.softPing(); // do an auth ping to make sure all is OK e.g. menus stay current
      next();
    }
  } else {
    //Ping the server to check if cookie still valid
    auth.ping(() => {
      if (!store.state.user.isAuthorized) {
        //Still not authorised, redirect to login view
        Notify.Info(`Not authorized to ${to.path}. Redirecting to login page.`).Clear();
        next(
          {
            name: routeDefs.login.name,
            query: { redirect: to.fullPath }
          });
      }
      else {
        if (store.state.pluginManager === null || store.state.serverInfo === null) {
          store.dispatch(rootPaths.actions.UPDATE_AUTHENTICATED_DATA);
        }
        if (to.name === routeDefs.admin.name && !store.state.user.isAdmin) {
          // eslint-disable-next-line
          console.error("Access forbidden. Redirecting to portal.");
          Notify.Error("Access to admin page forbidden");
          next(from);
        } else {
          next();
        }
      }
    })
  }
});

router.afterEach((to, from, failure) => {
  if (to.matched.length === 0) { router.push(`/error/404`); }
  else if (to) { document.title = "Birdsnest Explorer - " + to.name.toString(); }
  else { console.error({error: "Router: to undefined", to: to}); }
  
  if (failure && isNavigationFailure(failure, NavigationFailureType.duplicated)===false) {
    router.push(`/error/400`);
  }
});

export default router;
