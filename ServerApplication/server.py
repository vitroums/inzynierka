from socketserver import TCPServer, ThreadingMixIn
from ssl import wrap_socket
from handlers.client_server_handler import ClientServerHandler
from utils.configuration import Configuration


class ClientServer(ThreadingMixIn, TCPServer):
    """
    Głowny serwer aplikacji. Działa na protokole TCP. Każde połączenie obsługiwane jest w osobnym wątku.
    Połączenia szyfrowane są protokołem SSL.
    """
    def __init__(self):
        """
        Tworzy instancję serwera.
        """
        self.__configuration = Configuration()
        TCPServer.__init__(self, (self.__configuration.host, self.__configuration.port), ClientServerHandler)   

    def server_bind(self):
        """
        Podpina serwer do socketa i tworzy szyfrowane połączenie.
        """
        super().server_bind()
        self.socket = wrap_socket(self.socket, server_side = True, certfile = self.__configuration.certificateFile, keyfile = self.__configuration.keysFile)

    def get_request(self):
        """
        Odbiera połączenie i weryfikuje szyfrowanie.

        Return:
            SSLSocket, (str, int): Zabezpieczony socket, adres, z którego przychodzi połączenie.
        """
        (socket, address) = super().get_request()
        print("New conneftion from:", address)
        socket.do_handshake()
        return (socket, address)