const path = require('path');
const webpack = require('webpack');
const ExtractTextPlugin = require('extract-text-webpack-plugin');
const CheckerPlugin = require('awesome-typescript-loader').CheckerPlugin;
const UglifyJsPlugin = require('uglifyjs-webpack-plugin');
const bundleOutputDir = './wwwroot/dist';

module.exports = env => {
  const isDevBuild = !(env && env.prod);
  return [
    {
      stats: { modules: false },
      entry: {
        'create-invoice': './ClientApp/pages/CreateInvoice.tsx',
        'edit-invoice': './ClientApp/pages/EditInvoice.tsx',
        root: './ClientApp/root',
        vendor: [
          'event-source-polyfill',
          'isomorphic-fetch',
          'react',
          'react-dom',
        ]
      },
      resolve: { extensions: ['.js', '.jsx', '.ts', '.tsx'] },
      output: {
        path: path.join(__dirname, bundleOutputDir),
        filename: '[name].js',
        publicPath: '/dist/',
      },
      devtool: 'source-map',
      module: {
        rules: [
          {
            test: /\.tsx?$/,
            include: /ClientApp/,
            use: 'awesome-typescript-loader?silent=true',
          },
          {
            test: /\.css$/,
            use: isDevBuild
              ? [
                  { loader: 'style-loader', options: { } },
                  { loader: 'css-loader', options: { sourceMap: true } },
                ]
              : ExtractTextPlugin.extract({ use: 'css-loader?minimize' }),
          },
          {
            test: /\.scss$/,
            use: isDevBuild
              ? [
                  { loader: 'style-loader', options: { } },
                  { loader: 'css-loader', options: { sourceMap: true } },
                  { loader: 'sass-loader', options: { sourceMap: true } },
                ]
              : ExtractTextPlugin.extract({ use: ['css-loader?minimize', 'sass-loader'] }),
          },
          {
            test: /\.(png|jpg|jpeg|gif|svg|woff)$/,
            use: 'url-loader?limit=25000',
          },
        ],
      },
      plugins: [
        new CheckerPlugin(),
        new webpack.DefinePlugin({
          'process.env.NODE_ENV': isDevBuild ? '"development"' : '"production"',
        }),
        new webpack.optimize.CommonsChunkPlugin({
          name: 'vendor',
          minChunks: Infinity,
        })
      ].concat(
        isDevBuild
          ? [
            // Plugins that apply in development builds only
            new webpack.SourceMapDevToolPlugin({
              filename: '[file].map', // Remove this line if you prefer inline source maps
              moduleFilenameTemplate: path.relative(
                bundleOutputDir,
                '[resourcePath]'
              ), // Point sourcemap entries to the original file locations on disk
            }),
          ]
          : [
            // Plugins that apply in production builds only
            new UglifyJsPlugin(),
            new ExtractTextPlugin('site.min.css')
          ]
      )
    }
  ];
};
