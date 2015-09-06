class Talker(object):
    """
    Dekorator funkcji, który przed i po wywołaniu danej metody wyświetla podany komunikat.
    """
    def __init__(self, before="", after=""):
        """
        Tworzy dekorator i zapisuje wiadomości do wyświetlenia przed i po wywołaniu funkcji.
        """
        self.__before = before
        self.__after = after

    def __call__(self, _function):
        """
        Obuduwuje wołaną funkcję i wyświetla przed oraz po żądaną wiadomość.
        """
        def result_function(*args, **kwargs):
            if self.__before:
                print(self.__before)
            result = _function(*args, **kwargs)
            if self.__after:
                print(self.__after)
            return result
        return result_function