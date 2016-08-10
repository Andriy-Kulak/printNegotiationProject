debugger;
require.config(
{
    baseUrl: 'scripts',
    //waitSeconds: 200,
    paths: {
        'print.module': 'print.module',
        domReady: '../../node_modules/requirejs-domready/domReady',
        'angular': "../../node_modules/angular/angular.min",
        'ui.router': '../../node_modules/angular-ui-router/release/angular-ui-router',
        "requirejs-domready": "vendor/requirejs-domready/domReady"
    },
    shim: {
        "print.module": {
            deps: [
              'angular',
              'ui.router'
              //'ngAnimate',
              //'toaster',
              //'ui.grid',
            ],
        },
        "angular": {
            exports: 'angular'
        },
        'ui.router': {
            deps: ['angular']
        }
        //'angular-clipboard': {
        //    deps: ['angular']
        //},
        //'ngAnimate': {
        //    deps: ['angular']
        //},
        //'toaster': {
        //    deps: ['angular', 'ngAnimate']
        //},
        //'ui.grid': {
        //    deps: ['angular', ]
        //},
    }
});
