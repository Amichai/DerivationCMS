app.controller('userCtrl', ['$scope', '$http', function ($scope, $http) {
    
    function loadProfile() {
        $http.get(baseUrl + 'api/User/' + userId).success(function (profile) {
            $scope.createdString = profile.CreatedString;
            $scope.annotations = profile.RecentAnnotations;
            $scope.documents = profile.Documents;
        })
    }

    loadProfile()
}]);