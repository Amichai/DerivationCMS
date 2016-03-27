var app = angular.module('annotation', ['smart-table', 'ngCookies']);

app.controller('navBarCtrl', ['$scope', '$http', '$timeout', function ($scope, $http, $timeout) {
    $scope.currentPage = "Home";
    var url = window.location.href.toLowerCase();
    if (url.indexOf("users") != -1) {
        $scope.currentPage = "Users";
    } else if (url.indexOf("help") != -1) {
        $scope.currentPage = "Help";
    } else if (url.indexOf("register") != -1) {
        $scope.currentPage = "Register";
    }   

    $http.get(baseUrl + 'api/Users/GetCurrentUser').success(function (user) {
        $scope.currentUserModel = user;
    });

    $scope.submitSearch = function () {
        $http.get(baseUrl + 'api/Search?query=' + $scope.searchQuery).success(function (user) {

            console.log("testing")
        });
    }

}]);