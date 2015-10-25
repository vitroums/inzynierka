from socketserver import TCPServer
from ssl import wrap_socket, CERT_OPTIONAL
from handlers.client_server_handler import ClientServerHandler
from utils.configuration import Configuration


class ClientServer(TCPServer):
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
        self.socket = wrap_socket(self.socket, cert_reqs=CERT_OPTIONAL, server_side = True, 
                                  ca_certs=self.__configuration.certificateFile, certfile=self.__configuration.certificateFile, keyfile=self.__configuration.keysFile, do_handshake_on_connect=True)

    def get_request(self):
        """
        Odbiera połączenie i weryfikuje szyfrowanie.

        Return:
            SSLSocket, (str, int): Zabezpieczony socket, adres, z którego przychodzi połączenie.
        """
        (socket, address) = super().get_request()
        print("New connection from:", address)
        return (socket, address)