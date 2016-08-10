(function () {
    'use strict';

    angular.module('print.module').controller('termsCtrl', termsCtrl);

    function termsCtrl($http, $scope, $rootScope) {
        var vm = this;
        var dataService = $http;
        getQuestionsList();



        //Stores Top Level Answers to Questions
        vm.termAnswers;

        const pageMode = {
            LIST: 'List',
            EDIT: 'Edit',
            ADD: 'Add'
        };
        vm.uiState = {
            mode: pageMode.LIST,
            isDetailAreaVisible: false,
            isListAreaVisible: true,
            isSearchAreaVisible: true,
            isValid: true,
            messages: []
        };

        // Controller Functions
        function getQuestionsList() {
            dataService.get("/api/Terms")
            .then(function (result) {
                vm.terms = result.data;
                console.log(vm.terms);               
            }, function (error) {
                handleException(error);
            });
        }

        vm.saveAnswers = function() {
            debugger;
            //combines the index of YesNo and FreeForm Answers
            let AnsIndex = Object.keys(vm.termYesNoAns).concat(Object.keys(vm.termFreeFormAns));
            //creates a unique index AnsIndex
            let uniqueAnsIndex = AnsIndex.filter(function (item, i, ar) { return ar.indexOf(item) === i; })

            console.log("Keys", uniqueAnsIndex);

            //termAnswers is where all the answer objects will be stored for QuestionID, AnswerFreeForm, AnswerYesNo
            vm.termAnswers = [];
            //loop that pushes all the answers into termAnswers
            for (let i = 0; i < uniqueAnsIndex.length; i++) {
                vm.termAnswers.push({
                    QuestionID: uniqueAnsIndex[i],
                    AnswerYesNo: vm.termYesNoAns[uniqueAnsIndex[i]],
                    AnswerFreeForm: vm.termFreeFormAns[uniqueAnsIndex[i]]
                });
            }
            console.log("AnswerTest after FreeForm", vm.termAnswers);


            debugger;

            dataService.post("/api/terms/saveAnswers/answers",
                vm.termAnswers)
            .then(function (result) {
                // Update product object
                vm.answers = result.data;

                setUIState(pageMode.LIST);
            }, function (error) {
                handleException(error);
            });
        }

        //Andriy Test method that returns proper objects to be used by POST method
        vm.Test123 = function () {
            
            //combines the index of YesNo and FreeForm Answers
            let AnsIndex = Object.keys(vm.termYesNoAns).concat(Object.keys(vm.termFreeFormAns));
            //creates a unique index AnsIndex
            let uniqueAnsIndex = AnsIndex.filter(function (item, i, ar) { return ar.indexOf(item) === i; })

            console.log("Keys", uniqueAnsIndex);

            //termAnswers is where all the answer objects will be stored for QuestionID, AnswerFreeForm, AnswerYesNo
            vm.termAnswers = [];
            //loop that pushes all the answers into termAnswers
            for (let i = 0; i < uniqueAnsIndex.length; i++) {
                vm.termAnswers.push({
                    QuestionID: uniqueAnsIndex[i],
                    AnswerYesNo: vm.termYesNoAns[uniqueAnsIndex[i]],
                    AnswerFreeForm: vm.termFreeFormAns[uniqueAnsIndex[i]]
                });
            }
            console.log("AnswerTest after FreeForm", vm.termAnswers);
        }

        function handleException(error) {
            vm.uiState.isValid = false;
            var msg = {
                property: 'Error',
                message: ''
            };

            vm.uiState.messages = [];

            switch (error.status) {
                case 400:  // Bad Request
                    // Model state erros
                    var errors = error.data.ModelState;
                    debugger;

                    // Loop through and get all validation errors
                    for (var key in errors) {
                        for (var i = 0; i < errors[key].length; i++) {
                            addValidationMessage(key, errors[key][i]);
                        }

                    }
                    break;

                case 404:  // 'Not Found'
                    msg.message = 'The product you were ' +
                                  'requesting could not be found';
                    vm.uiState.messages.push(msg);

                    break;

                case 500:  // 'Internal Error'
                    msg.message =
                      error.data.ExceptionMessage;
                    vm.uiState.messages.push(msg);

                    break;

                default:
                    msg.message = 'Status: ' +
                                error.status +
                                ' - Error Message: ' +
                                error.statusText;
                    vm.uiState.messages.push(msg);

                    break;
            }
        }

        function addValidationMessage(prop, msg) {
            vm.uiState.messages.push({
                property: prop,
                message: msg
            });
        }

        function setUIState(state) {
            vm.uiState.mode = state;

            vm.uiState.isDetailAreaVisible =
                (state == pageMode.ADD || state == pageMode.EDIT);
            vm.uiState.isListAreaVisible = (state == pageMode.LIST);
            vm.uiState.isSearchAreaVisible = (state == pageMode.LIST);
        }
    }
})();