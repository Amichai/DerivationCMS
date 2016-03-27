app.controller('documentCtrl',
    ['$scope', '$http', function ($scope, $http) {
        $scope.isDocumentRtl = false;

        $scope.pullLeft = true;

        $scope.contentLoaded = false;

        function loadAnnotatedDocument() {
            if (canView != "True") {
                $scope.contentLoaded = true;
                return;
            }
            $http.get(baseUrl + 'api/AnnotatedDocument/' + documentId).success(function (annotatedDocument) {
                $scope.annotations = annotatedDocument.Annotations;
                $scope.isDocumentRtl = annotatedDocument.isRTL;
                $scope.document = annotatedDocument.Document;
                $scope.annotators = annotatedDocument.Annotators;
                for (var i = 0; i < $scope.annotations.length; i++) {
                    $scope.annotations[i].isExpanded = false;
                }

                function f() {
                    for (var i = 0; i < $scope.document.Tokens.length; i++) {
                        var token = $scope.document.Tokens[i];
                        var linked = token.LinkedAnnotations;
                        if (linked.length > 0) {
                            $('#token_' + i).addClass('hasAnnotation');
                        }
                    }
                    $scope.contentLoaded = true;
                }
                delay(f);
            });
        }
        
        function getSelectionText() {
            var text = "";
            if (window.getSelection) {
                text = window.getSelection().toString();
            } else if (document.selection && document.selection.type != "Control") {
                text = document.selection.createRange().text;
            }
            return text;
        }

        $('#newAnnotationModal').on('shown.bs.modal', function (event) {
            var text = getSelectionText();
            if (text) {
                //TODO: tokenize the selection so that it links properly
                var toSet = "`" + text + "`";
                $scope.newAnnotationText = toSet;
                $scope.$apply();
            }
        })

        $scope.getUserId = function () {
            return userId;
        }

        loadAnnotatedDocument();

        $scope.canDelete = function (ann) {
            return role == "admin" || userId == ann.Author;
        }

        $scope.starAnnotation = function (annotation, state) {
            annotation.IsStarred = state;
            event.stopPropagation();
            //$http.post(baseUrl + 'api/Annotation/')
        }

        $scope.canEdit = function (ann) {
            return userId = ann.Author;
        }

        $scope.editAnnotation = function (ann) {
            $scope.annotationBeingEdited = ann;
            $('#editAnnotationModal').modal('show');
            event.stopPropagation();
        }

        $scope.deleteAnnotation = function (annotation) {
            $http.delete(baseUrl + 'api/Annotation?id=' + annotation.Id).success(function(success) {
                if (success) {
                    var indexToRemove = -1;
                    for (var i = 0; i < $scope.annotations.length; i++) {
                        var ann = $scope.annotations[i];
                        if (ann.Id == annotation.Id) {
                            indexToRemove = i;
                            break;
                        }
                    }
                    if (indexToRemove != -1) {
                        $scope.annotations.splice(indexToRemove, 1);
                    }
                }
            });
            event.stopPropagation();
        }

        $scope.currentUrl = window.location.href;

        $scope.saveAnnotationEdits = function () {
            $http.put(baseUrl + 'api/Annotation/', $scope.annotationBeingEdited).success(function (annotatedDocument) {
                $('#editAnnotationModal').modal('hide');
                $scope.annotationBeingEdited = undefined;
                loadAnnotatedDocument();
            });
        }

        $scope.saveAnnotation = function () {
            var newAnnotation = new Object();
            newAnnotation.Body = $scope.newAnnotationText;
            newAnnotation.DocumentId = documentId;
            $http.post(baseUrl + 'api/Annotation/', newAnnotation).success(function (annotatedDocument) {
                $('#newAnnotationModal').modal('hide');
                $scope.newAnnotationText = "";
                loadAnnotatedDocument();
            });
        }

        function delay(callback) {
            setTimeout(callback, 0);
        }

        $scope.pageHeight = window.innerHeight - 60;

        $scope.hoverToken = function (token, tokenIndex, entered) {
            for(var i=0; i < token.LinkedAnnotations.length; i++) {
                var annotationIndex = token.LinkedAnnotations[i];
                if (entered) {
                    $('#annotation_' + annotationIndex).addClass('highlightedText');
                    $('#token_' + tokenIndex).addClass('highlightedText');
                } else {
                    $('#annotation_' + annotationIndex).removeClass('highlightedText');
                    $('#token_' + tokenIndex).removeClass('highlightedText');
                }
            }
        }

        $scope.expandAnnotation = function (annotation, index) {
            if (annotation.TokenRange) {
                var id = annotation.TokenRange.StartIdx;
                document.getElementById('token_' + id).scrollIntoView(true);
            }

            if (annotation.isExpanded) {
                annotation.isExpanded = false;
                return;
            }
            //Get some jslinq here
            for (var i = 0; i < $scope.annotations.length; i++) {
                $scope.annotations[i].isExpanded = false;
            }
            annotation.isExpanded = true;
            setTimeout(function () {
                document.getElementById('annotation_' + index).scrollIntoView(true);
            }, 0);
        }

        $scope.clickToken = function (token) {
            if (token.LinkedAnnotations.length == 0) {
                return;
            }
            var id = token.LinkedAnnotations[0];
            document.getElementById('annotation_' + id).scrollIntoView(true);
        }

        $scope.hover = function (ann, annotationIndex, entered) {
            if (!ann.TokenRange) {
                return;
            }
            var start = ann.TokenRange.StartIdx;
            var range = ann.TokenRange.Range;
            if (entered) {
                for (var idx = start; idx < start + range; idx++) {
                    $('#token_' + idx).addClass('highlightedText');
                }
                $('#annotation_' + annotationIndex).addClass('highlightedText');
            } else {
                for (var idx = start; idx < start + range; idx++) {
                    $('#token_' + idx).removeClass('highlightedText');
                }
                $('#annotation_' + annotationIndex).removeClass('highlightedText');
            }
        }
            
        window.addEventListener('resize', onResize, false);
        function onResize() {
            $scope.pageHeight = window.innerHeight - 60;
            $scope.$apply();
        }
    }]);