(function () {
    "use strict";

    var module = angular.module('print.module', [
        'ui.router',
        'ui.grid'
    ]);
    module.config(function ($stateProvider, $urlRouterProvider, $locationProvider) {
        $urlRouterProvider.otherwise('/print');
        $stateProvider
            .state('print', {
                url: '/print',
                templateUrl: "Public/scripts/sharedViews/printNavbar.html"

            })
            .state('books', {
                url: "/print/books",
                templateUrl: "Public/scripts/books/books.view.html",
                controller: 'booksCtrl',
                controllerAs: 'vm'
            })
            .state('print.circulation', {
                url: "/circulation",
                templateUrl: "Public/scripts/circulation/circulation.view.html"
            })
            .state('print.endReport', {
                url: "/endreport",
                templateUrl: "Public/scripts/endReport/endReport.view.html"
            })
            .state('print.rates', {
                url: "/rates",
                controller: 'ratesCtrl',
                templateUrl: "Public/scripts/rates/rates.view.html",
                controllerAs: 'vm'
            })
            .state('print.terms', {
                url: "/termsconditions",
                templateUrl: "Public/scripts/terms/terms.view.html",
                controller: 'termsCtrl',
                controllerAs: 'vm'

            })
            .state('print.tablet', {
                url: "/tablet",
                templateUrl: "Public/scripts/tablet/tablet.view.html"
            })
            .state('print.signSubmit', {
                url: "/signsubmit",
                templateUrl: "Public/scripts/signSubmit/signSubmit.view.html"
            });
        
        //("print/home"), { template: "<home-print></home-print>" }

       $locationProvider.html5Mode(true);
    });

    //module.component("homePrint", {
    //    template: "This is the print home page"
    //});
}());