// Copyright (c) 2019-2020 "20Road"
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
import { bus, events } from "@/bus";
import { createRouter, createWebHistory, RouteRecordRaw, breadcrumb } from "vue-router";
import { store, rootPaths } from "@/store";
import { auth } from "@/assets/ts/webcrap/authcrap";
import webcrap from "@/assets/ts/webcrap/webcrap";

export const routeDefs = {
  portal: {
    name: "Portal",
    path: "/portal"
  },
  about: {
    name: "About",
    path: "/about"
  },
  docs: {
    name: "Docs",
    path: "/:docs*"
  },
  report: {
    name: "Report Viewer",
    path: "/report"
  },
  reports: {
    name: "Reports",
    path: "/reports"
  },
  visualizer: {
    name: "Visualizer",
    path: "/visualizer"
  },
  admin: {
    name: "Admin",
    path: "/admin"
  },
  login: {
    name: "Login",
    path: "/login"
  },
  info: {
    name: "Server Information",
    path: "/info"
  },
  indexEditor: {
    name: "Index Editor",
    path: "/indexeditor"
  }
}

const routes: Array<RouteRecordRaw> = [
  {
    path: routeDefs.portal.path,
    name: routeDefs.portal.name,
    meta: {
      breadcrumbs: [
        { name: routeDefs.portal.name }
      ]
    },
    component: () =>
      import(/* webpackChunkName: "portal" */ "@/views/Portal.vue")
  },
  {
    path: '/',
    redirect: routeDefs.portal.path
  },
  {
    path: routeDefs.about.path,
    name: routeDefs.about.name,
    meta: {
      allowAnonymous: true,
      breadcrumbs: [
        { name: routeDefs.about.name }
      ]
    },
    // route level code-splitting
    // this generates a separate chunk (about.[hash].js) for this route
    // which is lazy-loaded when the route is visited.
    component: () =>
      import(/* webpackChunkName: "about" */ "@/views/About.vue")
  },
  {
    path: routeDefs.reports.path,
    name: routeDefs.reports.name,
    meta: {
      breadcrumbs: [
        { name: routeDefs.reports.name }
      ]
    },
    component: () =>
      import(/* webpackChunkName: "reports" */ "@/views/Reports.vue")
  },
  {
    path: routeDefs.report.path,
    name: routeDefs.report.name,
    meta: {
      breadcrumbs: [
        { name: routeDefs.reports.name, link: routeDefs.reports.path },
        { name: routeDefs.report.name }
      ]
    },
    component: () =>
      import(/* webpackChunkName: "reports" */ "@/views/ReportView.vue")
  },
  {
    path: routeDefs.admin.path,
    name: routeDefs.admin.name,
    meta: {
      breadcrumbs: [
        { name: routeDefs.admin.name }
      ]
    },
    component: () =>
      import(/* webpackChunkName: "admin" */ "@/views/Admin.vue")
  },
  {
    path: routeDefs.login.path,
    name: routeDefs.login.name,
    meta: {
      allowAnonymous: true,
      breadcrumbs: [
        { name: routeDefs.login.name }
      ]
    },
    component: () =>
      import(/* webpackChunkName: "login" */ "@/views/Login.vue")
  },
  {
    path: routeDefs.visualizer.path,
    name: routeDefs.visualizer.name,
    meta: {
      breadcrumbs: [
        { name: routeDefs.visualizer.name }
      ]
    },
    component: () =>
      import(/* webpackChunkName: "visualizer" */ "@/views/Visualizer.vue")
  },
  {
    path: routeDefs.info.path,
    name: routeDefs.info.name,
    meta: {
      breadcrumbs: [
        { name: routeDefs.info.name }
      ]
    },
    component: () =>
      import(/* webpackChunkName: "portal" */ "@/views/ServerInfoView.vue")
  },
  {
    path: routeDefs.indexEditor.path,
    name: routeDefs.indexEditor.name,
    meta: {
      breadcrumbs: [
        { name: routeDefs.admin.name, link: routeDefs.admin.path },
        { name: routeDefs.indexEditor.name }
      ]
    },
    component: () =>
      import(/* webpackChunkName: "admin" */ "@/views/IndexEditorView.vue")
  },
  {
    path: routeDefs.docs.path,
    name: routeDefs.docs.name,
    meta: {
      allowAnonymous: true,
      breadcrumbs: [
        { name: routeDefs.docs.name, link: routeDefs.docs.path }
      ]
    },
    component: () =>
      import(/* webpackChunkName: "docs" */ "@/views/Docs.vue")
  }
];

const router = createRouter({
  history: createWebHistory(process.env.BASE_URL),
  routes
});

router.beforeEach((to, from, next) => {
  if (to.meta.allowAnonymous === true) {    
    if (to.name === routeDefs.docs.name && webcrap.misc.isIE() === false) {
      const path = to.path.split("#")[0];

      if (path === routeDefs.docs.path) {
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
        // eslint-disable-next-line
        console.log("Not authorized. Redirecting to login page. Path: " + to.path);
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
          bus.emit(events.Notifications.Error, "Access to admin page forbidden");
          next(from);
        } else {
          next();
        }
      }
    })
  }
});

router.afterEach((to) => {
  if (to) { document.title = "Birdsnest Explorer - " + to.name.toString(); }
  else { console.error({error: "Router: to undefined", to: to}); }
  
});

export default router;
