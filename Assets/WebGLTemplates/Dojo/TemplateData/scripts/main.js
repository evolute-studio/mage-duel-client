// Main initialization script
document.addEventListener("DOMContentLoaded", async function() {
    try {
        // Initialize orientation manager
        OrientationManager.init();
        
        // Show loading bar
        const loadingBar = document.querySelector("#unity-loading-bar");
        loadingBar.style.display = "block";
        
        // First initialize and wait for Service Worker activation
        console.log('Initializing Service Worker...');
        await ServiceWorkerManager.init();
        
        // Preload resources
        console.log('Preloading resources...');
        await ServiceWorkerManager.preloadResources();
        
        // Configure Unity Loader
        const buildUrl = "Build";
        const loaderUrl = buildUrl + "/{{{ LOADER_FILENAME }}}";
        const config = {
            dataUrl: buildUrl + "/{{{ DATA_FILENAME }}}",
            frameworkUrl: buildUrl + "/{{{ FRAMEWORK_FILENAME }}}",
            #if USE_THREADS
            workerUrl: buildUrl + "/{{{ WORKER_FILENAME }}}",
            #endif
            #if USE_WASM
            codeUrl: buildUrl + "/{{{ CODE_FILENAME }}}",
            #endif
            #if MEMORY_FILENAME
            memoryUrl: buildUrl + "/{{{ MEMORY_FILENAME }}}",
            #endif
            #if SYMBOLS_FILENAME
            symbolsUrl: buildUrl + "/{{{ SYMBOLS_FILENAME }}}",
            #endif
            streamingAssetsUrl: "StreamingAssets",
            companyName: {{{ JSON.stringify(COMPANY_NAME) }}},
            productName: {{{ JSON.stringify(PRODUCT_NAME) }}},
            productVersion: {{{ JSON.stringify(PRODUCT_VERSION) }}},
            showBanner: function unityShowBanner(msg, type) {
                const warningBanner = document.querySelector("#unity-warning");
                
                function updateBannerVisibility() {
                    warningBanner.style.display = warningBanner.children.length ? 'block' : 'none';
                }
                
                var div = document.createElement('div');
                div.innerHTML = msg;
                warningBanner.appendChild(div);
                
                if (type == 'error') div.style = 'background: red; padding: 10px;';
                else {
                    if (type == 'warning') div.style = 'background: yellow; padding: 10px;';
                    setTimeout(function() {
                        warningBanner.removeChild(div);
                        updateBannerVisibility();
                    }, 5000);
                }
                
                updateBannerVisibility();
            }
        };
        
        // Initialize Unity Loader
        console.log('Initializing Unity Loader...');
        const unityLoader = UnityLoader.init(buildUrl, loaderUrl, config);
        
        // Setup for mobile devices
        unityLoader.setupMobileDevice();
        
        // Load Unity
        console.log('Loading Unity...');
        await unityLoader.loadUnity()
            .then(instance => {
                console.log("Unity loaded successfully");
            })
            .catch(error => {
                console.error("Failed to load Unity:", error);
            });
    } catch (error) {
        console.error("Critical error during initialization:", error);
    }
}); 