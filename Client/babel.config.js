module.exports = {
  presets: [
    '@quasar/babel-preset-app',
    [
      "babel-preset-proposals",
      {
        "exportDefaultFrom": true
      }
    ]
  ]
};
