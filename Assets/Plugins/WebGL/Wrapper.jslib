mergeInto(LibraryManager.library, {
    // get_controller_username: function() {
    //     console.log("GetControllerUsername called");
    //     var username = window.unityConnector.GetUsername();
    //     var bufferSize = lengthBytesUTF8(username) + 1;
    //     var buffer = _malloc(bufferSize);
    //     stringToUTF8(username, buffer, bufferSize);
    //     return buffer;
    // },
    controller_login: function() {
        console.log("ControllerLogin called");
        return window.unityConnector.ControllerLogin();
    },
    controller_logout: function() {
        console.log("ControllerLogout called");
        return window.controllerInstance.disconnect();
    },
    is_controller_logged_in: function() {
        console.log("IsControllerLoggedIn called");
        return window.unityConnector.IsControllerLoggedIn() ? 1 : 0;
    },
    get_controller_username: function() {
        console.log("GetControllerUsername called");
        return window.unityConnector.GetControllerUsername();
    },
    execute_controller: function(transaction) {
        console.log("ExecuteController called with transaction:", transaction);
        return window.unityConnector.ExecuteTransaction(UTF8ToString(transaction));
    },
    get_connection_data: function() {
        var connectionData = JSON.stringify(window.unityConnector.GetConnectionData());
        var bufferSize = lengthBytesUTF8(connectionData) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(connectionData, buffer, bufferSize);
        return buffer;
    },
    open_controller_profile: function() {
        console.log("OpenControllerProfile called");
        window.controllerInstance.controller.openProfile("achievements");
    }
}); 