const path = require('path');
const webpack = require('webpack');

const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const OptimizeCSSAssetsPlugin = require("optimize-css-assets-webpack-plugin");
const UglifyJsPlugin = require('uglifyjs-webpack-plugin');
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer').BundleAnalyzerPlugin;

const bundleOutputDir = './wwwroot/dist';

module.exports = env => {
    const isDevBuild = !(env && env.prod);
    const isAnalyze = (env && env.analyze);
    return [{
        devServer: {
            contentBase: path.join(__dirname, "./wwwroot"),
            compress: true,
            overlay: true,
            port: 3001,
            // proxying back to the fronting aspnetcore app can cause an infinite loop
            // proxy: {
            //   "/": "http://localhost:5000",
            // },
        },
        stats: {
            modules: false
        },
        entry: {
            'create-invoice': './ClientApp/pages/CreateInvoice.tsx',
            'edit-invoice': './ClientApp/pages/EditInvoice.tsx',
            'root': './ClientApp/root',
        },
        resolve: {
            extensions: ['.js', '.jsx', '.ts', '.tsx']
        },
        output: {
            path: path.join(__dirname, bundleOutputDir),
            filename: '[name].js',
            publicPath: '/dist/',
            devtoolModuleFilenameTemplate: info =>
                path.resolve(info.absoluteResourcePath).replace(/\\/g, '/'),
        },
        mode: isDevBuild ? 'development' : 'production',
        module: {
            rules: [{
                    test: /\.tsx?$/,
                    include: /ClientApp/,
                    use: 'ts-loader',
                },
                {
                    test: /\.css$/,
                    use: [
                        !isDevBuild 
                            ? MiniCssExtractPlugin.loader
                            : {
                                loader: 'style-loader',
                                options: {
                                    sourceMap: true,
                                },
                            },
                        {
                            loader: 'css-loader',
                            options: {
                                sourceMap: true,
                            },
                        },
                    ]
                },
                {
                    test: /\.scss$/,
                    use: [
                        !isDevBuild 
                            ? MiniCssExtractPlugin.loader
                            : {
                                loader: 'style-loader',
                                options: {
                                    sourceMap: true,
                                },
                            },
                        {
                            loader: 'css-loader',
                            options: {
                                sourceMap: true,
                            },
                        },
                        {
                            loader: 'sass-loader',
                            options: {
                                sourceMap: true,
                                implementation: require("sass")
                            },
                        },
                    ]
                },
                {
                    test: /\.(png|jpg|jpeg|gif|svg|woff)$/,
                    use: 'url-loader?limit=25000',
                },
            ],
        },
        optimization: {
            minimizer: isDevBuild ? [] : [
                new UglifyJsPlugin({
                    cache: true,
                    parallel: true,
                    sourceMap: true ,
                }),
                new OptimizeCSSAssetsPlugin({
                }),
            ],
            splitChunks: {
                chunks: 'all',
                cacheGroups: {
                    default: false,
                    vendor: {
                        name: 'vendor',
                        test: /[\\/]node_modules[\\/]/,
                        priority: -10
                    },
                }
            }
        },
        plugins: [
            ...isDevBuild ?
            [
                // Plugins that apply in development builds only
                new webpack.EvalSourceMapDevToolPlugin({
                    filename: '[file].map', // Remove this line if you prefer inline source maps
                })
            ] : [
                // Plugins that apply in production builds only
                new webpack.SourceMapDevToolPlugin({
                    filename: '[file].map', // Remove this line if you prefer inline source maps
                }),
                new MiniCssExtractPlugin({
                    filename: 'site.min.css',
                }),
            ],
            // Webpack Bundle Analyzer
            // https://github.com/th0r/webpack-bundle-analyzer
            ...isAnalyze ? [ new BundleAnalyzerPlugin() ] : [],
        ],
    }];
};
