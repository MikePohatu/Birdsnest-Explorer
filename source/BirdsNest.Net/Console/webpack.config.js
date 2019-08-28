"use strict";
const webpack = require('webpack');
const path = require('path');
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin;
const MiniCssExtractPlugin = require('mini-css-extract-plugin');

var config = {
    entry: {
        visualizer: './src/visualizer-entry.js',
        reports: './src/reports-entry.js',
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
                use: [
                    {
                        loader: 'ts-loader'
                    }
                ],
                exclude: [/node_modules/, /wwwroot/]
            },
            {
                test: /\.css$/i,
                use: [
                    {
                        loader: MiniCssExtractPlugin.loader
                    },
                    {
                        loader: 'css-loader'
                    }
                ]
            },
            {
                test: /\.(png|jpe?g|gif)$/i,
                use: [
                    {
                        loader: 'file-loader',
                        options: {
                            outputPath: 'image',
                            name: '[name].[ext]'
                        }
                    }
                ]
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
        new MiniCssExtractPlugin({
            // Options similar to the same options in webpackOptions.output
            // all options are optional
            filename: 'css/[name]-bundle.css',
            chunkFilename: 'css/[name]-bundle.css',
            ignoreOrder: false // Enable to remove warnings about conflicting order
        }),
        new BundleAnalyzerPlugin({
            analyzerMode: 'static',
            reportFilename: '../../stats/report.html',
            openAnalyzer: false
        })
    ],
    optimization: {
        //minimize: env.production, // only --env.production used
        splitChunks: {
            cacheGroups: {
                d3: {
                    test: /[\\/]node_modules[\\/]((d3).*)[\\/]/,
                    name: "d3",
                    chunks: "all",
                    priority: -10
                },
                vendors: {
                    test: /[\\/]node_modules[\\/]/,
                    name: "common-vendor",
                    chunks: "all",
                    priority: -20
                }
            }
        }
    }
};

module.exports = (env, argv) => {
    if (argv.mode === 'development') {
        config.optimization.minimize = false;
        config.plugins.push(
            new webpack.DefinePlugin({
                'PRODUCTION': JSON.stringify(false),
                'ENV': JSON.stringify(argv.mode)
            }));
    }

    if (argv.mode === 'production') {
        config.optimization.minimize = true;
        config.plugins.push(
            new webpack.DefinePlugin({
                'PRODUCTION': JSON.stringify(true),
                'ENV': JSON.stringify(argv.mode)
            }));
    }

    return config;
};