(function () {
    'use strict';

    angular.module('print.module').controller('booksCtrl', booksCtrl);

    function booksCtrl($http, $scope, $rootScope, $state) {
        var vm = this;
        var dataService = $http;
        vm.$scope = $scope;

        vm.books = [];
        vm.searchYears = [];
        vm.searchInput = {
            selectedYear: {
                YearId: 0,
                YearName: ''
            }
        };

        vm.selectedBookOptions = function () {
            if (vm.booksForm.$valid) {
                console.log("selected book", vm.selectedBook);
                dataService.put('api/books/' + vm.selectedBook.ID)
                .then(function (result) {
                    console.log("Book Submitted to cookie", result);

                    // Redirect to Terms tab
                    $state.go('print.terms');
                });
            }
        }

        getBooksList();

        function getBooksList() {
            dataService.get("/api/Books/GetBooks/2017")
            .then(function (result) {
                vm.books = result.data;
                console.log(vm.books);
            }, function (error) {
                handleException(error);
            });
        }
    }
})();