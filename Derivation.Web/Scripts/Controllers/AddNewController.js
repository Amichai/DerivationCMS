app.controller('addNewCtrl',
    ['$scope', '$http', function ($scope, $http) {
        $scope.submit = function () {
            var body = new Object();
            body.Steps = $scope.steps;
            body.IsArchived = false;
            body.Owner = userId;
            body.Title = $scope.newTitle;
            body.Description = $scope.newDescription;

            $http.post(baseUrl + 'api/Derivation', body).success(function () {
                document.location.href = '/';
            });

        }

        $scope.steps = [];

        function reset() {
            $scope.newEquation = "";
            $scope.newTransition = "";
            $scope.newNotes = "";
        }

        $scope.$watch('steps', function (value) {
            setTimeout(function () {
                if (MathJax) {
                    MathJax.Hub.Queue(["Typeset", MathJax.Hub]);
                }
            }, 0);

        }, true);

        $scope.add = function() {
            var step = new Object();
            step.Equation = $scope.newEquation;
            step.Transition = $scope.newTransition;
            step.Notes = $scope.newNotes;
            $scope.steps.push(step);

            reset();
        }
    }]);