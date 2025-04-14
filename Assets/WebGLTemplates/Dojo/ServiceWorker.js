// Serviceworker.js - with forced activation
console.log('Serviceworker has been downloaded');
console.log('Test service worker update 15'); // Change the number to track updates

// Примусово пропускаємо чергу очікування під час встановлення
self.addEventListener('install', event => {
    console.log('Serviceworker: Setting a new version');
    // Forge up installation without expectation
    event.waitUntil(self.skipWaiting());
});

// When activated forcibly capture control
self.addEventListener('activate', event => {
    console.log('ServiceWorker: Activation of the new version ');

    // Forcibly capture customer control
    event.waitUntil(
        clients.claim().then(() => {
            // Reload all windows directly
            return self.clients.matchAll().then(clients => {
                clients.forEach(client => {
                    // Reboot the page straight from Service Worker
                    client.navigate(client.url);
                });
            });
        })
    );
});

// Fetch minimum handler
self.addEventListener('fetch', event => {
    // We do nothing with requests - just miss them
    // We return the request as is, without cache
    return;
});

// Message handler for additional control
self.addEventListener('message', event => {
    if (event.data && event.data.type === 'SKIP_WAITING') {
        console.log('Serviceworker has received the Skip_waiting command');
        self.skipWaiting();
    }
});