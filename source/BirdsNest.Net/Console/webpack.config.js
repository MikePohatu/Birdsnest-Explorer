"use strict";
const webpack = require('webpack');
const path = require('path');

module.exports = {
    entry: {
        console: './src/console.js',
        visualizer: './src/visualizer.js'
    },
    output: {
        filename: '[name]-bundle.js',
        path: __dirname + '/wwwroot'
    },
    module: {
        rules: [
            {
                test: /\.(ts|tsx)?$/,
                loader: 'ts-loader',
                exclude: /node_modules/
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