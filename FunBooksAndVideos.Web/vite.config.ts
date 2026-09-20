import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

// The browser calls relative URLs such as /api/v1/products. This proxy forwards /api to the real API,
// so the browser only ever talks to one origin: no CORS setup, and the API needs no changes.
// Point it at an API on another port with API_URL, for example API_URL=http://localhost:5199 npm run dev.
// (Node's `process` is declared here because @types/node is not one of this demo's dependencies.)
declare const process: { env: Record<string, string | undefined> };
const apiUrl = process.env.API_URL ?? "http://localhost:5163";

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: { "/api": apiUrl },
  },
});
