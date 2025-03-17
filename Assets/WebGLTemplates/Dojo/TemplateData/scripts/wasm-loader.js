// WASM Module Loader
class WasmLoader {
    static async loadDojoWasm() {
        return new Promise((resolve, reject) => {
            const dojoScript = document.createElement("script");
            dojoScript.src = "TemplateData/dojo.js/dojo_c.js";
            
            dojoScript.onload = async () => {
                try {
                    // Wait for WASM module to load
                    await wasm_bindgen("TemplateData/dojo.js/dojo_c_bg.wasm");
                    console.log("WASM module initialized successfully");
                    resolve();
                } catch (error) {
                    console.error("Failed to initialize WASM module:", error);
                    reject(error);
                }
            };
            
            dojoScript.onerror = (error) => {
                console.error("Failed to load WASM script:", error);
                reject(error);
            };
            
            document.body.appendChild(dojoScript);
        });
    }
} 