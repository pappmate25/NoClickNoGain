import http.server
import socketserver

PORT = 8000

class Handler(http.server.SimpleHTTPRequestHandler):
    def end_headers(self):
        self.send_header('Access-Control-Allow-Origin', '*')
        path = self.translate_path(self.path)
        if path.endswith(".br"):
            self.send_header("Content-Encoding", "br")
        http.server.SimpleHTTPRequestHandler.end_headers(self)

with socketserver.TCPServer(("", PORT), Handler) as httpd:
    print("serving at port", PORT)
    httpd.serve_forever()
