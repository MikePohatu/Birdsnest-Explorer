"use strict";
const webpack = require('webpack');
const path = require('path');
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin;

module.exports = {
    entry: {
        visualizer: './src/visualizer-entry.js',
        console: './src/console-entry.js'
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
    },
    plugins: [
        new BundleAnalyzerPlugin({
            analyzerMode: 'static',
            reportFilename: '../../stats/report.html',
            openAnalyzer: false
        })
    ],
    optimization: {
        minimize: false // <---- disables uglify.
        // minimizer: [new UglifyJsPlugin()] if you want to customize it.
    }
};