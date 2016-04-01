app.controller('addNewCtrl',
    ['$scope', '$http', function($scope, $http) {
        $scope.submit = function() {
            var body = new Object();
            body.Steps = $scope.steps;
            body.IsArchived = false;
            body.Owner = userId;
            body.Title = $scope.newTitle;
            body.Description = $scope.newDescription;

            $http.post(baseUrl + 'api/Derivation/' + derivationId, body).success(function () {
                document.location.href = '/';
            });
            //if (derivationId) {
            //} else {
            //    $http.post(baseUrl + 'api/Derivation', body).success(function () {
            //        document.location.href = '/';
            //    });
            //}
        }


        if (derivationId) {
            $http.get(baseUrl + 'api/Derivation/' + derivationId).success(function(derivation) {
                console.log(derivation);

                $scope.newTitle = derivation.Title;
                $scope.newDescription = derivation.Description;

                $scope.steps = derivation.Steps;
            });
        }

        console.log(userId);
        console.log(derivationId);

        $scope.trash = function(step) {
            $scope.steps.remove(step);
        }

        $scope.edit = function(step) {
            $scope.editStep = step;
        }

        $scope.doneEditing = function(step) {
            $scope.editStep = undefined;
        }

        $scope.editStep = undefined;

        $scope.isEditStep = function(step) {
            return step === $scope.editStep;
        }

        $scope.isEditing = function() {
            if (!derivationId) {
                return false;
            }

            return true;
        }

        $scope.steps = [];

        function reset() {
            $scope.newEquation = "";
            $scope.newTransition = "";
            $scope.newNotes = "";
        }

        Array.prototype.remove = function () {
            var what, a = arguments, L = a.length, ax;
            while (L && this.length) {
                what = a[--L];
                while ((ax = this.indexOf(what)) !== -1) {
                    this.splice(ax, 1);
                }
            }
            return this;
        };

        function updateMathJax() {
            setTimeout(function () {
                if (MathJax) {
                    MathJax.Hub.Queue(["Typeset", MathJax.Hub]);
                }
            }, 0);
        }

        $scope.$watch('editStep', function (value) {
            if (!value) {
                updateMathJax();
            }
        }, false);

        $scope.$watch('steps', function (value) {
            updateMathJax();
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