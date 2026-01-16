import { UserConfig, defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig(async ({ mode }) => {
    // Load app-level env vars to node-level env vars.
    const env = { ...process.env, ...loadEnv(mode, process.cwd()) };
    process.env = env;

    const config: UserConfig = {
        publicDir: "public",
        build: {
            outDir: "build",
            rollupOptions: {
                input: "src/index.tsx",
            },

        },
        plugins: [react()],
        optimizeDeps: {
            include: [],
        },
        server: {
            port: 5173,
            hmr: {
                clientPort: 5173,
            },
            strictPort: true,
        },
    };

    return config;
});