// Unity Loader
class UnityLoader {
    static init(buildUrl, loaderUrl, config) {
        this.buildUrl = buildUrl;
        this.loaderUrl = loaderUrl;
        this.config = config;
        this.canvas = document.querySelector("#unity-canvas");
        this.loadingBar = document.querySelector("#unity-loading-bar");
        this.fullscreenButton = document.querySelector("#unity-fullscreen-button");
        
        // Initialize flag to potentially stop Unity loading
        window.stopUnityLoading = false;
        window.gameInstance = null;
        
        // Check orientation before loading Unity
        OrientationManager.checkOrientation();
        
        return this;
    }
    
    static setupMobileDevice() {
        if (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent)) {
            // Mobile device style: fill the whole browser client area with the game canvas
            const container = document.querySelector("#unity-container");
            
            var meta = document.createElement('meta');
            meta.name = 'viewport';
            meta.content = 'width=device-width, height=device-height, initial-scale=1.0, user-scalable=no, shrink-to-fit=yes';
            document.getElementsByTagName('head')[0].appendChild(meta);
            container.className = "unity-mobile";
            this.canvas.className = "unity-mobile";

            // To lower canvas resolution on mobile devices to gain some
            // performance, uncomment the following line:
            // this.config.devicePixelRatio = 1;

            #if SHOW_DIAGNOSTICS
            // position the diagnostics icon in the corner on the canvas
            const diagnostics_icon = document.getElementById("diagnostics-icon");
            diagnostics_icon.style.position = "fixed";
            diagnostics_icon.style.bottom = "10px";
            diagnostics_icon.style.right = "0px";
            this.canvas.after(diagnostics_icon);
            #endif
        } else {
            // Desktop style: Render the game canvas in a window that can be maximized to fullscreen
            this.canvas.style.width = "{{{ WIDTH }}}px";
            this.canvas.style.height = "{{{ HEIGHT }}}px";
        }

        #if BACKGROUND_FILENAME
        this.canvas.style.background = "url('" + this.buildUrl + "/{{{ BACKGROUND_FILENAME.replace(/'/g, '%27') }}}') center / cover";
        #endif
        this.loadingBar.style.display = "block";
    }
    
    static async loadUnity() {
        try {
            // First load WASM module
            await WasmLoader.loadDojoWasm();
            
            // Then load Unity
            return new Promise((resolve, reject) => {
                // Only load Unity if we're not in portrait mode/incorrect aspect ratio
                if (window.stopUnityLoading) {
                    reject("Unity loading stopped due to portrait orientation");
                    return;
                }
                
                const script = document.createElement("script");
                script.src = this.loaderUrl;
                
                script.onload = () => {
                    // Check one more time before creating Unity instance
                    if (!window.stopUnityLoading) {
                        createUnityInstance(this.canvas, this.config, (progress) => {
                            // Progress is still tracked but not visually displayed
                            console.log('Loading progress: ' + (100 * progress) + '%');
                        }).then((unityInstance) => {
                            window.gameInstance = unityInstance; // Make it accessible globally
                            
                            // Check orientation once the game is loaded
                            OrientationManager.checkOrientation();
                            
                            #if SHOW_DIAGNOSTICS
                            const diagnostics_icon = document.getElementById("diagnostics-icon");
                            diagnostics_icon.onclick = () => {
                                unityDiagnostics.openDiagnosticsDiv(unityInstance.GetMemoryInfo);
                            };
                            #endif
                            
                            this.fullscreenButton.onclick = () => {
                                unityInstance.SetFullscreen(1);
                            };
                            
                            resolve(unityInstance);
                        }).catch((message) => {
                            alert(message);
                            reject(message);
                        });
                    } else {
                        reject("Unity loading stopped due to portrait orientation");
                    }
                };
                
                script.onerror = (error) => {
                    reject("Failed to load Unity script: " + error);
                };
                
                document.body.appendChild(script);
            });
        } catch (error) {
            console.error("Error loading Unity:", error);
            throw error;
        }
    }
} 