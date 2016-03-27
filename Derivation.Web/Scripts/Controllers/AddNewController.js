app.controller('addNewCtrl',
    ['$scope', '$http', function ($scope, $http) {
        $scope.submit = function () {
            var body = {
                Body: $scope.body,
                Title: $scope.title,
                Author: $scope.author
            }
            $http.post(baseUrl + 'api/Document', body).success(function () {
                document.location.href = '/';
            });
        }
}]);