from server_handler_helper import ServerHandlerHelper
from uuid import uuid1
from dropbox_api import DropboxApi

class ClientServerHandler(ServerHandlerHelper):
    __DATA = ["country", "state", "city", "organization", "unit", "name", "email", "login"]
    __dba = DropboxApi()
    def handle(self):
        """

        """
        command = self._receiveString().lower()
        if command == "new-user":
            self.__newUserCommand()
        elif command == "new-group":
            self.__newGroupCommadn()
        else:
            self._sendString("unknown-command")

    def __newUserCommand(self):
        self._sendString("new-user-type")
        type = self._receiveString().lower()
        if type == "remote":
            self.__remoteUserGenerate()
        elif type == "local":
            self.__localUserGenerate()
        else:
            self._sendString("unknown-command")

    def __remoteUserGenerate(self):
        self._sendString("provide-data")
        data = self._receiveString().split(";")
        login = data[7]
        mail = data[6]
        data = {self.__DATA[i] : data[i] for i in range(0, len(self.__DATA))}
        if not self.__doesUserExists(login):
            uuid = uuid1()
            certificate, key = "cert.pem", "key.pem"
            self._sendString(uuid.hex)
            self._sendFile(certificate)
            self._sendFile(key)
            receipt = self._receiveString().lower()
            if receipt == "done":
                self.__saveUser(uuid, login, mail)
        else:
            self._sendString("user-exists")

    def __localUserGenerate(self):
        pass
    
    def __doesUserExists(self, login):
        self.__dba._parseDatabase()
        nicks = [o._nick for o in self.__dba._usersList]
        for i in nicks:
            if i == login:
                return True

        return False



    def __newUser(self, uuid, data = None, certificateRequestFile = None):
        if data is not None:
            return ("cert.pem", "key.pem")
        elif certificateRequestFile is not None:
            return "cert.pem"

    def __saveUser(self, uuid, login, mail):
        self.__dba._addUserToList(uuid.hex, login, mail)
        self.__dba._createDatabae()
        self.__dba._uploadFile("list.xml")
        print("User has been added successfully!")
        pass

    def __newGroupCommadn(self):
        self._sendString("provide-data")
        data = self._receiveString().split(";")

        pass