mergeInto(LibraryManager.library, {
    //flush our file changes to IndexedDB
    SyncDB: function () {
        FS.syncfs(false, function (err) {
           if (err) console.log("syncfs error: " + err);
        });
    },

    InitBrowserQuitDetection: function (gameObjectName) {
        var gameObjectNameStr = UTF8ToString(gameObjectName);
        
        window.addEventListener('beforeunload', function(e) {
            console.log("Browser is closing, triggering save...");
            SendMessage(gameObjectNameStr, 'OnBrowserQuitting', '');
        });
    }
});