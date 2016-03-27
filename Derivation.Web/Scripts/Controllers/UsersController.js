app.controller('usersCtrl', ['$scope', '$http', function ($scope, $http) {
    function loadUsers() {
        $http.get(baseUrl + 'api/Users/').success(function (users) {
            $scope.users = users;
            $scope.usersSafe = [].concat($scope.users);
        });
    }

    loadUsers();

    $scope.editing = undefined;

    $scope.edit = function(user) {
        $scope.editing = user;
    }

    $scope.save = function (user) {
        $http.put(baseUrl + 'api/Users/' + user.UserId, user).success(function () {
            loadUsers();
            $scope.editing = undefined;
        });
    }
}]);