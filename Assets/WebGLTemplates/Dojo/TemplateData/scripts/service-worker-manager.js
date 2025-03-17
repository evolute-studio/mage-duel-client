// Service Worker Manager
class ServiceWorkerManager {
    static async init() {
        if ("serviceWorker" in navigator) {
            try {
                // Function to unregister all Service Workers
                await this.unregisterOldWorkers();
                
                // Register new Service Worker and wait for activation
                const registration = await navigator.serviceWorker.register("ServiceWorker.js");
                
                // Check for updates
                await registration.update();
                
                // Set up periodic updates
                setInterval(() => {
                    registration.update();
                }, 60 * 60 * 1000);
                
                // Wait for Service Worker to activate
                if (registration.installing) {
                    console.log('Service Worker is installing...');
                    return new Promise((resolve) => {
                        registration.installing.addEventListener('statechange', (e) => {
                            if (e.target.state === 'activated') {
                                console.log('Service Worker activated');
                                resolve();
                            }
                        });
                    });
                } else if (registration.waiting) {
                    console.log('Service Worker waiting for activation');
                    // Force activate Service Worker
                    registration.waiting.postMessage({type: 'SKIP_WAITING'});
                    return new Promise((resolve) => {
                        navigator.serviceWorker.addEventListener('controllerchange', () => {
                            console.log('Service Worker activated');
                            resolve();
                        }, {once: true});
                    });
                } else if (registration.active) {
                    console.log('Service Worker already active');
                    return Promise.resolve();
                }
                
                return Promise.resolve();
            } catch (error) {
                console.log('Service worker registration failed:', error);
                return Promise.resolve(); // Continue loading even on error
            }
        }
        return Promise.resolve(); // If Service Worker is not supported
    }
    
    static async unregisterOldWorkers() {
        const registrations = await navigator.serviceWorker.getRegistrations();
        return Promise.all(registrations.map(registration => registration.unregister()));
    }
    
    // Method for preloading resources
    static async preloadResources() {
        const resourcesToPreload = [
            "Build/{{{ LOADER_FILENAME }}}",
            "Build/{{{ FRAMEWORK_FILENAME }}}",
            #if USE_THREADS
            "Build/{{{ WORKER_FILENAME }}}",
            #endif
            "Build/{{{ DATA_FILENAME }}}",
            "Build/{{{ CODE_FILENAME }}}",
            "TemplateData/dojo.js/dojo_c.js",
            "TemplateData/dojo.js/dojo_c_bg.wasm"
        ];
        
        console.log('Preloading resources...');
        
        const preloadPromises = resourcesToPreload.map(url => {
            return new Promise((resolve, reject) => {
                const xhr = new XMLHttpRequest();
                xhr.open('GET', url, true);
                xhr.onload = () => {
                    if (xhr.status >= 200 && xhr.status < 300) {
                        console.log(`Resource ${url} loaded`);
                        resolve();
                    } else {
                        console.warn(`Failed to load ${url}: ${xhr.status}`);
                        resolve(); // Continue even on error
                    }
                };
                xhr.onerror = () => {
                    console.warn(`Error loading ${url}`);
                    resolve(); // Continue even on error
                };
                xhr.send();
            });
        });
        
        return Promise.all(preloadPromises);
    }
} 