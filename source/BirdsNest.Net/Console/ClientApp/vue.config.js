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
module.exports = {
  devServer: {
    host: '0.0.0.0',
    https: true,
    proxy: {
      '^/api': {
        changeOrigin: true,
        target: 'https://localhost:44341',
        secure: false,
        withCredentials: true
      },
      '^/dynamic': {
        changeOrigin: true,
        target: 'https://localhost:44341',
        secure: false,
        withCredentials: true
      }

    },
    watchOptions: {
      ignored: ['**/node_modules/**']
    },
  },

  pwa: {
    iconPaths: {
      favicon32: 'img/icons/favicon-32x32.png',
      favicon16: 'img/icons/favicon-16x16.png'
    },
    name: 'BirdsNest'
  },

  productionSourceMap: false
}