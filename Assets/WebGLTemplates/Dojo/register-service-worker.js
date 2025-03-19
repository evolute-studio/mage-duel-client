(function() {
    if ('serviceWorker' in navigator) {
        navigator.serviceWorker.addEventListener('message', function(event) {
            console.log('Message received from ServiceWorker:', event.data);
            if (event.data && event.data.type === 'RELOAD_PAGE') {
                console.log('Reloading page by ServiceWorker command');
                window.location.reload();
            }
        });
        
        let refreshing = false;
        
        navigator.serviceWorker.addEventListener('controllerchange', function() {
            console.log('ServiceWorker controller has changed');
            if (!refreshing) {
                refreshing = true;
                console.log('Reloading page after controller change');
                window.location.reload();
            }
        });
        
        function registerAndUpdate() {
            console.log('Starting ServiceWorker registration');
            
            navigator.serviceWorker.register('/ServiceWorker.js', {
                updateViaCache: 'none',
                scope: '/'
            }).then(function(registration) {
                console.log('ServiceWorker registered:', registration.scope);
                
                registration.update().then(function() {
                    console.log('Update check completed');
                }).catch(function(error) {
                    console.error('Update check error:', error);
                });
                
                registration.addEventListener('updatefound', function() {
                    console.log('New ServiceWorker version found');
                    const newWorker = registration.installing;

                    newWorker.addEventListener('statechange', function() {
                        console.log('New ServiceWorker state:', newWorker.state);
                        
                        if (newWorker.state === 'installed') {
                            console.log('New ServiceWorker installed, trying to activate');
                            
                            if (navigator.serviceWorker.controller) {
                                newWorker.postMessage({ type: 'SKIP_WAITING' });
                            }
                        }
                    });
                });
                
                setInterval(function() {
                    registration.update().then(function() {
                        console.log('Regular update check completed');
                    });
                }, 30000);

            }).catch(function(error) {
                console.error('ServiceWorker registration error:', error);
            });
        }
        
        registerAndUpdate();
        
        if (navigator.serviceWorker.controller) {
            console.log('Existing ServiceWorker found, trying to update');
            navigator.serviceWorker.controller.postMessage({ type: 'SKIP_WAITING' });
        }
    }
})();