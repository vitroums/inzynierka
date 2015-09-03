from socketserver import TCPServer, ThreadingMixIn
from ssl import wrap_socket
from server_handler import ClientServerHandler


class ClientServer(ThreadingMixIn, TCPServer):
    CERTIFICATE_FILE = "cert.pem"
    KEY_FILE = "key.pem"

    def __init__(self, address):
        TCPServer.__init__(self, address, ClientServerHandler)

    def server_bind(self):
        super().server_bind()
        self.socket = wrap_socket(self.socket, server_side = True, certfile = self.CERTIFICATE_FILE, keyfile = self.KEY_FILE)

    def get_request(self):
        (socket, address) = super().get_request()
        print("-- NEW CONNECTION --")
        socket.do_handshake()
        return (socket, address)