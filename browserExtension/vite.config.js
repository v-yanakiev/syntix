import { defineConfig } from "vite";
import { resolve } from "path";
import fs from "fs";

// Copy browser-polyfill to the output directory
function copyPolyfill() {
  return {
    name: "copy-polyfill",
    writeBundle: {
      sequential: true,
      order: "post",
      async handler({ dir }) {
        const source = resolve(
          __dirname,
          "node_modules/webextension-polyfill/dist/browser-polyfill.min.js"
        );
        const dest = resolve(dir, "browser-polyfill.min.js");
        await fs.promises.copyFile(source, dest);
      },
    },
  };
}

export default defineConfig(({ command, mode }) => {
  const browser = process.env.BROWSER || "chrome";
  const target = process.env.BUILD_TARGET || "content";
  const isBackground = target === "background";
  const entryFile = isBackground ? "background.ts" : "contentScript.ts";
  const outputFile = isBackground ? "background" : "contentScript";

  return {
    build: {
      outDir: `dist/${browser}`,
      emptyOutDir: isBackground,
      lib: {
        entry: resolve(__dirname, entryFile),
        name: outputFile,
        formats: ["iife"],
        fileName: () => `${outputFile}.js`,
      },
      rollupOptions: {
        output: {
          format: "iife",
          extend: true,
          dir: `dist/${browser}`,
        },
      },
    },
    plugins: [copyPolyfill()],
  };
});
