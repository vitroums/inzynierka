class Talker(object):
    def __init__(self, before="", after=""):
        self.__before = before
        self.__after = after

    def __call__(self, _function):
        def result_function(*args, **kwargs):
            if self.__before:
                print(self.__before)
            result = _function(*args, **kwargs)
            if self.__after:
                print(self.__after)
            return result
        return result_function