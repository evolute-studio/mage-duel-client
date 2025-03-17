// Screen Orientation Manager
class OrientationManager {
    static init() {
        // Check orientation when page loads and whenever window is resized
        window.addEventListener('load', this.checkOrientation);
        window.addEventListener('resize', this.checkOrientation);

        // Reload the page when orientation changes
        window.addEventListener('orientationchange', function() {
            // Small delay to allow orientation to fully change before reloading
            setTimeout(function() {
                window.location.reload();
            }, 100);
        });
    }

    // Function to check if the device is in portrait mode and if aspect ratio is below minimum
    static checkOrientation() {
        if (/iPhone|iPad|iPod|Android/i.test(navigator.userAgent)) {
            const aspectRatio = window.innerWidth / window.innerHeight;
            const portraitOverlay = document.getElementById('portrait-overlay');
            const unityContainer = document.getElementById('unity-container');
            const loadingBar = document.getElementById('unity-loading-bar');

            // Check if device is in portrait orientation or aspect ratio is less than 1.46
            if (window.innerHeight > window.innerWidth || aspectRatio < 1.46) {
                portraitOverlay.style.display = 'flex';
                // Hide the Unity container and loading bar to prevent game from loading behind overlay
                unityContainer.style.display = 'none';
                loadingBar.style.display = 'none';

                // If game instance exists, pause the game
                if (window.gameInstance) {
                    try {
                        window.gameInstance.SendMessage('GameManager', 'PauseGame');
                    } catch (e) {
                        console.log('Could not pause game');
                    }
                }

                // Stop loading the Unity instance if we're in portrait mode
                if (!window.gameInstance && window.stopUnityLoading) {
                    window.stopUnityLoading = true;
                }
            } else {
                portraitOverlay.style.display = 'none';
                // Show the Unity container
                unityContainer.style.display = 'block';

                // If game loading was stopped and we still don't have a game instance, reload the page
                if (window.stopUnityLoading && !window.gameInstance) {
                    window.location.reload();
                    return;
                }

                // If game instance exists, resume the game
                if (window.gameInstance) {
                    try {
                        window.gameInstance.SendMessage('GameManager', 'ResumeGame');
                    } catch (e) {
                        console.log('Could not resume game');
                    }
                }
            }
        }
    }
} 