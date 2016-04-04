app.controller('mainCtrl',
    ['$scope', '$http', '$timeout', function ($scope, $http, $timeout) {
        function loadDocuments() {
            $http.get(baseUrl + 'api/Derivation').success(function (docs) {
                $scope.documents = docs;
                $scope.documentsSafe = [].concat($scope.documents);
            });
        }

        loadDocuments();

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

        $scope.confirmEdit = function () {
            var newDoc = {
                Body: $scope.newDocumentBody,
                Title: $scope.newDocumentTitle,
                Author: $scope.newDocumentAuthor,
                Id: $scope.editingId,
                Owner: $scope.editingOwner
            }
            $http.put(baseUrl + 'api/Derivation', newDoc).success(function () {
                loadDocuments();
                $('#editDocumentModal').modal('hide');
                $scope.editingId = '';
                $scope.editingOwner = '';
            });
        }

        $scope.edit = function (row) {
            window.location.href = baseUrl + 'AddNew?id=' + row.Id + '&edit=true';
        }

        $scope.confirmDelete = function (doc) {
            $http.delete(baseUrl + 'api/Derivation/' + $scope.docToDelete.Id).success(function () {
                loadDocuments();
                $('#deleteDocumentModal').modal('hide');
                $scope.docToDelete = '';
            });
        }

        $scope.trash = function (doc) {
            $scope.docToDelete = doc;
            $('#deleteDocumentModal').modal('show');
        }

        $scope.addNewDerivation = function() {
            window.location.href = baseUrl + 'AddNew';
        };
    }]);