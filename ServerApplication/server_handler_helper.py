from socketserver import BaseRequestHandler
from os import SEEK_SET, SEEK_END

class ServerHandlerHelper(BaseRequestHandler):
    __ENCODING = "utf-8"
    __BUFFER_SIZE = 1024

    def _receiveString(self):
        size = self.__receiveStreamSize()
        readBytes = 0
        message = ""
        while readBytes < size:
             buffer = self.request.recv(self.__BUFFER_SIZE)
             readBytes += len(buffer)
             message += buffer.decode()
        print("From {}:{} -".format(self.client_address[0], self.client_address[1]), message)
        return message

    def _receiveFile(self, path):
        size = self.__receiveStreamSize()
        readBytes = 0
        print("From {}:{} -".format(self.client_address[0], self.client_address[1]), "receiving file ({})".format(path))
        file = open(path, "wb")
        while readBytes < size:
            buffer = self.request.recv(self.__BUFFER_SIZE)
            readBytes += len(buffer)
            file.write(buffer)
        file.close()
        print("From {}:{} -".format(self.client_address[0], self.client_address[1]), "file received ({})".format(path))


    def __receiveStreamSize(self):
        return int(self.request.recv(self.__BUFFER_SIZE).decode(self.__ENCODING), 2)

    def _sendString(self, message):
        print("To {}:{} -".format(self.client_address[0], self.client_address[1]), message)
        message = message.encode(self.__ENCODING)
        self.__sendSize(len(message))
        self.request.sendall(message)

    def _sendFile(self, path):
        print("To {}:{} -".format(self.client_address[0], self.client_address[1]), "sending file ({})".format(path))
        file = open(path, "rb")
        file.seek(0, SEEK_END)
        self.__sendSize(file.tell())
        file.seek(0, SEEK_SET)
        data = file.read(self.__BUFFER_SIZE)
        while data:
            self.request.sendall(data)
            data = file.read(self.__BUFFER_SIZE)
        print("To {}:{} -".format(self.client_address[0], self.client_address[1]), "file sent ({})".format(path))

    def __sendSize(self, size):
        size = bin(size).replace("b", "0").zfill(64).encode(self.__ENCODING)
        self.request.sendall(size)