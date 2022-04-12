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
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import legacy from '@vitejs/plugin-legacy'
import path from 'path';


// https://vitejs.dev/config/
export default defineConfig(({ command, mode }) => ({
  base: command === 'build' ? '/etc.clientlibs/<project>/clientlibs/' : '/',
  build: {
    brotliSize: false,
    manifest: false,
    minify: mode === 'development' ? false : 'terser',
    outDir: 'dist',
    sourcemap: command === 'serve' ? 'inline' : false,
    rollupOptions: {
      output: {
        assetFileNames: 'resources/[ext]/[name][extname]',
        chunkFileNames: 'resources/chunks/[name].[hash].js',
        entryFileNames: 'resources/js/[name].js',
      },
    },
  },
  css: {
    postcss: {
      plugins: [
        {
          postcssPlugin: 'internal:charset-removal',
          AtRule: {
            charset: (atRule) => {
              if (atRule.name === 'charset') {
                atRule.remove();
              }
            }
          }
        }
      ]
    }
  },
  plugins: [
    vue(),
    legacy({
      targets: ['ie >= 11'],
      additionalLegacyPolyfills: ['regenerator-runtime/runtime']
    })
  ],
  resolve: {
    alias: [
      {
        find: '@',
        replacement: path.resolve(__dirname, 'src')
      }
    ],
    extensions: ['.js', '.ts', '.vue']
  },
  server: {
    proxy: {
      '/api': {
        changeOrigin: true,
        target: 'https://localhost:44341',
        secure: false
      },
      '/dynamic': {
        changeOrigin: true,
        target: 'https://localhost:44341',
        secure: false
      }
    }
  }
}));
