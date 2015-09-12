class Configuration(object):
    """
    Konfgiguracja całego serwera. Instancja klasy tworzona jest raz w trakcie działania programu.
    Obiekt klasy zawiera wszystkie informacje o serwerze wymagane do pracy wszystkich jego komponentów.
    """
    __INSTANCE = None

    def __new__(cls, *args, **kwargs):
        """
        Implementacja wzorca Singletone. W programie zawsze istnieć będzie tylko jedna istancja klasy.
        """
        if not cls.__INSTANCE:
            cls.__INSTANCE = super().__new__(cls)
        return cls.__INSTANCE

    def __init__(self):
        """
        Tworzy instancje klasy i pobiera konfigurację serwera.
        """
        self.__getConfiguration()

    def __getConfiguration(self):
        """
        Pobiera konfigurację serwera.
        """
        self.__certificateFile = "cert.crt"
        self.__keysFile = "keys.key"
        self.__certificatesDir = "certs"
        self.__keysDir = "keys"
        self.__host = ""
        self.__port = 12345
        self.__caInformations = {"country":"PL", "state":"Pomorskie", "city":"Gdańsk", "organization":"PG", "unit":"ETI", "name":"PKI Cloud"}
        self.__informationsKeys = ["country", "state", "city", "organization", "unit", "name"]
        self.__notBefore = 0
        self.__notAfter = 10*356*24*60*60
        self.__signMethod = "sha1"
        self.__caPassword = "123456"
        self.__encoding = "utf-8"
        self.__bufferSize = 1024
        self.__dropboxApiKey = "q7RSg2cm1vAAAAAAAAAAC7sy1AfF2zsSStdhuG0KdJs3ieupiQ6A2Izek-5r8DE-"
        self.__databaseFileName = "list.xml"

    @property
    def certificateFile(self):
        return self.__certificateFile

    @property
    def keysFile(self):
        return self.__keysFile

    @property
    def certificatesDir(self):
        return self.__certificatesDir

    @property
    def keysDir(self):
        return self.__keysDir

    @property
    def host(self):
        return self.__host

    @property
    def port(self):
        return self.__port

    @property
    def caInformations(self):
        return self.__caInformations

    @property
    def informationsKeys(self):
        return self.__informationsKeys

    @property
    def notBefore(self):
        return self.__notBefore

    @property
    def notAfter(self):
        return self.__notAfter

    @property
    def signMethod(self):
        return self.__signMethod

    @property
    def caPassword(self):
        return self.__caPassword

    @property
    def encoding(self):
        return self.__encoding

    @property
    def bufferSize(self):
        return self.__bufferSize

    @property
    def dropboxApiKey(self):
        return self.__dropboxApiKey

    @property
    def databaseFileName(self):
        return self.__databaseFileName