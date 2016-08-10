(function () {
    'use strict';

    angular.module('print.module').controller('ratesCtrl', ratesCtrl);

    function ratesCtrl($http, $scope, $rootScope) {
        var vm = this;

        //test rates data
        vm.ratesData = [{
            "Tier": "Tier 1",
            "Tier Range": "Up to $5,3000",
            "Discount %": "",
            "Earned NET P4C Rate": "",
            "Earned NET P4CB Rate": "",
            "Earned NET P4C CPM": "",
            "Earned NET P4CB CPM": "" 
        }];
        //vm.ratesData = ratesData;
        vm.DiscountStruc = {
            data: vm.ratesData,
            enableSorting: true,
            enableGridMenu: true,
            columnDefs: [
                {field: "Tier", displayName: "Created By", width: '10%' },
                {field: "Tier Range", width: '10%'},
                {field: "Discount %", width: '10%' },
                {field: "Earned NET P4C Rate", width: '15%'},
                {field: "Earned NET P4CB Rate", width: '15%'},
                {field: "Earned NET P4C CPM", width: '15%'},
                {field: "Earned NET P4CB CPM", width: '15%'}
            ]

        };
        console.log(vm.ratesData)

    }
})();