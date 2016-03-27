app.controller('registerCtrl', ['$scope', '$http', function ($scope, $http) {
    $scope.errorMessage = "";

    $scope.register = function () {
        var name = $('#registrationUsername').val();
        var pass = $('#registrationPassword').val();
        var data = {
            Username: name,
            Password: pass
        };
        //url: 'api/Authentication/Register',
        $.ajax({
            type: 'POST',
            url: 'api/Users/',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(data)
        }).done(function (val) {
            console.log("Done: " + val);
            setTimeout(function () {
                $scope.authenticate(name, pass);
            }, 500);
        }).fail(function (val) {
            console.log("Fail: " + val.responseText);
            $scope.errorMessage = JSON.parse(val.responseText).Message;
            $scope.$apply();
        });
    }


    $scope.authenticate = function(name, pass) {
        if (name == undefined || pass == undefined) {
            name = $('#loginUsername').val();
            pass = $('#loginPassword').val();
        }
        var data = {
            Username: name,
            Password: pass
        };
        $.ajax({
            type: 'POST',
            url: 'api/Authentication/Authenticate',
            contentType: 'application/json; charset=utf-8',
            data: JSON.stringify(data)
        }).done(function (val) {
            console.log("Done: " + val);
            window.location.href = baseUrl;
        }).fail(function (val) {
            console.log("Fail: " + val.responseText);
            $scope.errorMessage = JSON.parse(val.responseText).Message;
            $scope.$apply();
        });
    }
}]);