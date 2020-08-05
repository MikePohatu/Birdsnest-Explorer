// Copyright (c) 2019-2020 "20Road"
// 20Road Limited [https://20road.com]
//
// This file is part of BirdsNest.
//
// BirdsNest is free software: you can redistribute it and/or modify
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
import Vue from "vue";
import VueRouter, { RouteConfig } from "vue-router";
import store, { rootPaths }  from "../store";
import { auth } from "../assets/ts/webcrap/authcrap";
Vue.use(VueRouter);

export const routeDefs = {
  portal: { 
    name: "Portal",
    path: "/portal"
  },
  about: {
    name: "About",
    path: "/about"
  },
  report: {
    name: "Report",
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
    name: "Info",
    path: "/info"
  }
}

const routes: Array<RouteConfig> = [
  {
    path: routeDefs.portal.path,
    name: routeDefs.portal.name,
    component: () =>
      import(/* webpackChunkName: "portal" */ "../views/Portal.vue")
  },
  {
    path: '/',
    redirect: routeDefs.portal.path
  },
  {
    path: routeDefs.about.path,
    name: routeDefs.about.name,
    // route level code-splitting
    // this generates a separate chunk (about.[hash].js) for this route
    // which is lazy-loaded when the route is visited.
    component: () =>
      import(/* webpackChunkName: "about" */ "../views/About.vue")
  },
  {
    path: routeDefs.reports.path,
    name: routeDefs.reports.name,
    component: () =>
      import(/* webpackChunkName: "reports" */ "../views/Reports.vue")
  },
  {
    path: routeDefs.report.path,
    name: routeDefs.report.name,
    component: () =>
      import(/* webpackChunkName: "reports" */ "../views/ReportView.vue")
  },
  {
    path: routeDefs.admin.path,
    name: routeDefs.admin.name,
    component: () =>
      import(/* webpackChunkName: "admin" */ "../views/Admin.vue")
  },
  {
    path: routeDefs.login.path,
    name: routeDefs.login.name,
    component: () =>
      import(/* webpackChunkName: "login" */ "../views/Login.vue")
  },
  {
    path: routeDefs.visualizer.path,
    name: routeDefs.visualizer.name,
    component: () =>
      import(/* webpackChunkName: "visualizer" */ "../views/Visualizer.vue")
  },
  {
    path: routeDefs.info.path,
    name: routeDefs.info.name,
    component: () =>
      import(/* webpackChunkName: "visualizer" */ "../views/ServerInfoView.vue")
  }
];

const router = new VueRouter({
  mode: "history",
  base: process.env.BASE_URL,
  routes
});

router.beforeEach((to, from, next) => {
  if (to.name === routeDefs.login.name || to.name === routeDefs.about.name) {
    next();
  } else {
    //potentially a refresh. Ping the server to check if cookie still valid
    auth.ping(() => {
      if (!store.state.user.isAuthorized) {
        //Still not authorised, redirect to login view
        console.log("Not authorized. Redirecting to login page.");
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
        next();
      }
    })
  }
});

router.afterEach((to) => {
  document.title = "BirdsNest - " + to.name;
});

export default router;
