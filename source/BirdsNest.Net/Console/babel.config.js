module.exports = function (api) {
    api.cache(true);

    const presets = [
        [
            "@babel/preset-env",            
            {
                "debug": true,
                "targets": {
                    "chrome": "68",
                    "firefox": "66",
                    "ie": "11"
                }
            }
        ]
    ];
    //const plugins = [... ];

    return {
        presets
        //plugins
    };
}