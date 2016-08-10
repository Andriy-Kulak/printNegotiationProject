(function () {
    'use strict';

    angular.module('print.module', [])
    .service('booksValue', function () {
        var bookName;

        return {
            //getProperty: function () {
            //    return property;
            //},
            setProperty: function (value) {
                bookName = value;
            }
        };
    });

})();