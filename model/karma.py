from datetime import datetime


class Karma(object):
    """docstring for Karma"""
    def __init__(self, awarder = "default awarder", reason = "default reason", awarded = datetime.today()):
        super(Karma, self).__init__()
        self.awarder = awarder
        self.reason = reason
        self.awarded = awarded