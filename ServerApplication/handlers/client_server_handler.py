from handlers.server_handler_helper import ServerHandlerHelper
from uuid import uuid1
from dropbox_api import DropboxApi
from CA.certificate_authority import CertificateAuthority
import dropbox


class ClientServerHandler(ServerHandlerHelper):
    """
    Główna klasa do obsługi połączeń serwera.
    """
    def __init__(self, request, client_address, server):
        """
        Tworzy handler dla serwera. Pobiera instancje klasy konfiguracji
        """
        self.__ca = CertificateAuthority()
        super().__init__(request, client_address, server)
        self.__dba = DropboxApi
        

    def handle(self):
        """
        Obsługuje połączenie przychodzące
        """
        command = self._receiveString().lower()
        if command == "new-user":
            self.__newUserCommand()
        elif command == "new-group":
            self.__newGroupCommand()
        else:
            self._sendString("unknown-command")

    def __newUserCommand(self):
        """
        Obsługuję proces tworzenia nowego użytkownika
        """
        # TODO Dodać pobieranie hasła do klucza
        name, mail = self.__requestUserData()
        if self.__doesUserExist(name, mail):
            self._sendString("user-exists")
            return
        informations = self.__requestUserInformations()
        uuid = uuid1()
        certificate, keys = self.__ca.newCertificate(informations, None, uuid)
        self.__sendNewUserFiles(certificate, keys)
        cmd = self._receiveString()
        if cmd == "is-added":
            self._sendString(self.__newUserGenerate())
        else:
            self._sendString("failed")


    def __requestUserData(self):
        """
        Żąda od użytkownika jego podstawowych danych.

        Return:
            str, str: Nazwa użytkownika i jego hasło.
        """
        self._sendString("provide-user-name")
        name = self._receiveString()
        self._sendString("provide-user-mail")
        mail = self._receiveString()
        return name, mail

    def __doesUserExist(self, name, mail):
        """
        Sprawdza w bazie danych czy istnieje podany użytkownik.

        Args:
            name (str): Nazwa użytkownika.
            mail (str): Adres e-mail.

        Return:
            bool: True jeśli użytkownik istnieje, False jeżeli nie istnieje. 
        """

        self.__dba._parseDatabase()
        nicks = [o._nick for o in self.__dba._usersList]
        mails = [o._mail for o in self.__dba._usersList]
        for i in nicks:
            if i == name:
                return True
        for j in mails:
            if j == name:
                return True

        return False

    def __requestUserInformations(self):
        """
        Żąda od użytkonika przesłania danych potrzebnych do stworzenia certyfikatu.
        Po otrzymaniu parsuje je do postaci potrzebnej dla CA.

        Return:
            dict: Słownik z danymi użytkownika.
        """
        self._sendString("provide-user-data")
        data = self._receiveString().split(";")
        return {self._configuration.informationsKeys[i]:data[i]
                for i in range(0, len(self._configuration.informationsKeys))}

    def __generateCertificate(self, informations, password, uuid):
        """
        Generuje certyfikat i klucz dla nowego użytkownika.

        Retrun:
            str, str: Ścieżka do pliku certyfikatu, ścieżka do pliku klucza
        """
        try:
            return self.__ca.newCertificate(informations, password, uuid)
        except ValueError:
            self._sendString("wrong-information-format")
        except IOError:
            self._sendString("server-error")

    def __sendNewUserFiles(self, certificate, keys):
        """
        Wysłanie plików certyfikatu i klucza do użytkownika.
        """
        self._sendString("certificate-file")
        self._sendFile(certificate)
        self._sendString("keys-file")
        self._sendFile(keys)
        

    def __newGroupCommand(self):
        self._sendString("provide-group-data")
        data = self._receiveString().split(";")
        pass

    def __newUserGenerate(self, name, mail, uuid):
        try:
            self.__dba.addNewUser(uuid, name, mail)
            # TODO kopia publiczngo do root'a
        except dropbox.rest.ErrorResponse:
            print("ERROR! Couldnt add new user!")
            return "failed"
        return "success"
