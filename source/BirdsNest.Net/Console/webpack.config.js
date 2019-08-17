"use strict";
const webpack = require('webpack');
const path = require('path');

module.exports = {
    entry: {
        console: './src/console-entry.js',
        visualizer: './src/visualizer-entry.js'
    },
    output: {
        filename: '[name]-bundle.js',
        path: __dirname + '/wwwroot/dist'
    },
    module: {
        rules: [
            {
                test: /\.(ts|tsx)?$/,
                loader: 'ts-loader',
                exclude: [/node_modules/, /wwwroot/]
            }
        ]
    },
    resolve: {
        extensions: [
            '.tsx',
            '.ts',
            '.js'
        ]
    }
};