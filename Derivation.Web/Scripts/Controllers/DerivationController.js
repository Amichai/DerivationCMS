app.controller('derivationCtrl',
    ['$scope', '$http', function ($scope, $http) {

        function loadDerivation() {
            $http.get(baseUrl + 'api/Derivation/' + documentId).success(function (derivation) {
                $scope.derivation = derivation;
            });
        }

        loadDerivation();
    }]);