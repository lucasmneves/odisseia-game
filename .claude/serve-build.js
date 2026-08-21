/**
 * Servidor estático mínimo para testar a build WebGL localmente.
 * O WebGL não abre por file:// — precisa de HTTP. Sem dependências: usa só o
 * módulo http do Node.
 *
 * A build usa Brotli com decompressionFallback, então os .unityweb são
 * descomprimidos pelo próprio loader em JS e este servidor não precisa mandar
 * nenhum header de Content-Encoding.
 */
const http = require("http");
const fs = require("fs");
const path = require("path");

const ROOT = path.join(__dirname, "..", "Builds", "WebGL");
const PORT = Number(process.env.PORT) || 8080;

const TYPES = {
  ".html": "text/html; charset=utf-8",
  ".js": "application/javascript",
  ".json": "application/json",
  ".wasm": "application/wasm",
  ".data": "application/octet-stream",
  ".unityweb": "application/octet-stream",
  ".css": "text/css",
  ".png": "image/png",
  ".jpg": "image/jpeg",
  ".ico": "image/x-icon",
};

http.createServer((req, res) => {
  const urlPath = decodeURIComponent(req.url.split("?")[0]);
  const rel = urlPath === "/" ? "index.html" : urlPath.replace(/^\/+/, "");
  const file = path.join(ROOT, rel);

  // impede sair da pasta da build
  if (!file.startsWith(ROOT)) {
    res.writeHead(403).end("forbidden");
    return;
  }

  fs.readFile(file, (err, data) => {
    if (err) {
      console.log(`404 ${urlPath}`);
      res.writeHead(404).end("not found");
      return;
    }
    console.log(`200 ${urlPath} (${data.length} bytes)`);
    res.writeHead(200, { "Content-Type": TYPES[path.extname(file)] || "application/octet-stream" });
    res.end(data);
  });
}).listen(PORT, () => {
  console.log(`Servindo ${ROOT} em http://localhost:${PORT}`);
});
