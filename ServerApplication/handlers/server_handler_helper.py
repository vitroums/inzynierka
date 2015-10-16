from socketserver import BaseRequestHandler
from os import SEEK_SET, SEEK_END
from utils.configuration import Configuration

class ServerHandlerHelper(BaseRequestHandler):
    """
    Klasa wspomagająca obsługę operacji na socketach.
    """
    def __init__(self, request, client_address, server):
        """
        Tworzy instancję klasy i pobiera konfiguracje serwera.
        """
        self._configuration = Configuration()
        super().__init__(request, client_address, server)

    def _receiveString(self):
        """
        Odbiera wiadomość i przekształca ją na postać stringa.

        Return:
            str: Otrzymana wiadomość.
        """
        size = self.__receiveStreamSize()
        readBytes = 0
        message = ""
        while readBytes < size:
             buffer = self.request.recv(self._configuration.bufferSize)
             readBytes += len(buffer)
             message += buffer.decode()
        print("From {}:{} -".format(self.client_address[0], self.client_address[1]), message)
        return message

    def _receiveFile(self, path):
        """
        Odebiera plik i zapisuje go w podanej lokalizacji.

        Args:
            path (str): Lokalizacja do której zapisany zostanie otrzymany plik.
        """
        size = self.__receiveStreamSize()
        readBytes = 0
        print("From {}:{} -".format(self.client_address[0], self.client_address[1]), "receiving file ({})".format(path))
        file = open(path, "wb")
        while readBytes < size:
            buffer = self.request.recv(self._configuration.bufferSize)
            readBytes += len(buffer)
            file.write(buffer)
        file.close()
        print("From {}:{} -".format(self.client_address[0], self.client_address[1]), "file received ({})".format(path))

    def __receiveStreamSize(self):
        """
        Pobiera rozmiar następnej wiadomości.

        Return:
            int: Rozmiar następnej wiadomości.
        """
        return int(self.request.recv(self._configuration.bufferSize).decode(self._configuration.encoding), 2)

    def _sendString(self, message):
        """
        Wysyła wiadomość w postaci stringa.
        """
        print("To {}:{} -".format(self.client_address[0], self.client_address[1]), message)
        message = message.encode(self._configuration.encoding)
        self.__sendSize(len(message))
        self.request.sendall(message)

    def _sendFile(self, path):
        """
        Wysyła plik ze wskazanej lokalizacji.
        """
        print("To {}:{} -".format(self.client_address[0], self.client_address[1]), "sending file ({})".format(path))
        file = open(path, "rb")
        file.seek(0, SEEK_END)
        self.__sendSize(file.tell())
        file.seek(0, SEEK_SET)
        data = file.read(self._configuration.bufferSize)
        while data:
            self.request.sendall(data)
            data = file.read(self._configuration.bufferSize)
        print("To {}:{} -".format(self.client_address[0], self.client_address[1]), "file sent ({})".format(path))

    def __sendSize(self, size):
        """
        Wysyła rozmiar następnej wiadomości
        """
        print(size)
        size = bin(size).replace("b", "0").zfill(64).encode(self._configuration.encoding)
        self.request.sendall(size)