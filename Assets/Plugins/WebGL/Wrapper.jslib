mergeInto(LibraryManager.library, {
    get_controller_username: function() {
        console.log("GetControllerUsername called");
        var username = window.unityConnector.GetUsername();
        var bufferSize = lengthBytesUTF8(username) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(username, buffer, bufferSize);
        return buffer;
    },
    controller_login: function() {
        console.log("ControllerLogin called");
        return window.unityConnector.ControllerLogin();
    },
    is_controller_logged_in: function() {
        console.log("IsControllerLoggedIn called");
        return window.unityConnector.IsControllerLoggedIn() ? 1 : 0;
    }
    execute_controller: function(transaction) {
        return window.unityConnector.ExecuteTransaction(transaction);
    }
}); 