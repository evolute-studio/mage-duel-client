mergeInto(LibraryManager.library, {
    GetControllerUsername: function() {
        console.log("GetControllerUsername called");
        return window.unityConnector.GetUsername();
        },
    CallControllerLogin: function() {
            console.log("ControllerLogin called");
            return window.unityConnector.ControllerLogin();
        }
}); 