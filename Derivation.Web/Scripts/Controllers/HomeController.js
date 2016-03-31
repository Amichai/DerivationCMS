app.controller('mainCtrl',
    ['$scope', '$http', '$timeout', function ($scope, $http, $timeout) {
        function loadDocuments() {
            $scope.newDocumentBody = '';
            $scope.newDocumentTitle = '';
            $scope.newDocumentAuthor = '';
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

        $scope.removePermissionedUser = function (user) {
            var docId = $scope.editPermissionsDoc.Id;
            $http.delete(baseUrl + 'api/Permissions?docId=' + docId + '&user=' + user).success(function (success) {
                if (success) {
                    $scope.editPermissionsDoc.Permissioned.remove(user);
                    loadDocuments();
                }
            });
        }

        $scope.addNewPermissionedUser = function () {
            var permissionsModel = new Object();
            permissionsModel.DocumentId = $scope.editPermissionsDoc.Id;
            permissionsModel.User = $scope.newPermissionedUser;
            $http.post(baseUrl + 'api/Permissions', permissionsModel).success(function (success) {
                if (success) {
                    $scope.editPermissionsDoc.Permissioned.push($scope.newPermissionedUser);
                    $scope.newPermissionedUser = '';
                    loadDocuments();
                }
            });
        }

        $scope.editPermissionsDoc = undefined;

        $scope.editDocumentPermissions = function (doc) {
            $scope.editPermissionsDoc = doc;
        }

        $scope.addNewDocument = function () {
            var newDoc = {
                Body: $scope.newDocumentBody,
                Title: $scope.newDocumentTitle,
                Author: $scope.newDocumentAuthor,
                IsPublic: $scope.newDocumentIsPublic,
                IsOpen: $scope.newDocumentIsOpen
            }
            $http.post(baseUrl + 'api/Document', newDoc).success(function () {
                loadDocuments();
                $('#newDocumentModal').modal('hide');
            });
        }

        $scope.makePublic = function (doc, state) {
            $http.post(baseUrl + 'api/MakePublic?docId=' + doc.Id + '&state=' + state).success(function (success) {
                if (success) {
                    loadDocuments();
                }
            });
        }

        $scope.makeOpen = function (doc, state) {
            if (!doc.IsPublic && state) {
                return;
            }
            $http.post(baseUrl + 'api/MakeOpen?docId=' + doc.Id + '&state=' + state).success(function (success) {
                if (success) {
                    loadDocuments();
                }
            });
        }

        $scope.$watch("newDocumentIsPublic", function (newVal, oldVal) {
            if (newVal == false) {
                $scope.newDocumentIsOpen = false;
            }
        });
        $scope.newDocumentIsPublic = true;
        $scope.newDocumentIsOpen = false;

        $scope.confirmEdit = function () {
            var newDoc = {
                Body: $scope.newDocumentBody,
                Title: $scope.newDocumentTitle,
                Author: $scope.newDocumentAuthor,
                Id: $scope.editingId,
                Owner: $scope.editingOwner
            }
            $http.put(baseUrl + 'api/Document', newDoc).success(function () {
                loadDocuments();
                $('#editDocumentModal').modal('hide');
                $scope.editingId = '';
                $scope.editingOwner = '';
            });
        }

        $scope.edit = function (row) {
            $http.get(baseUrl + 'api/Document/' + row.Id).success(function (doc) {
                $scope.newDocumentAuthor = row.Author;
                $scope.newDocumentTitle = row.Title;
                $scope.newDocumentBody = doc.Body;
                $scope.editingId = row.Id;
                $scope.editingOwner = row.Owner;
                $('#editDocumentModal').modal('show');
            });
        }

        $scope.confirmDelete = function (doc) {
            $http.delete(baseUrl + 'api/Document/' + $scope.docToDelete.Id).success(function () {
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