from datetime import datetime, timedelta
from model.karma import KarmaType
from functools import reduce


class Member(object):
    """Represents a member of a chat channel"""

    karma_expiry_days = 30

    def get_current_karma(self):
        """a function doc string"""
        cut_off = datetime.today() - timedelta(days=Member.karma_expiry_days)
        recent_karma = list(map((lambda k: k.karma_type), filter((lambda k: k.awarded > cut_off), self.__karma_list)))
        positive_karma = list(filter((lambda k: k == KarmaType.POZZYPOZ), recent_karma)).__len__()
        negative_karma = list(filter((lambda k: k == KarmaType.NEGGYNEG), recent_karma)).__len__()
        net_karma = positive_karma - negative_karma
        return net_karma

    def __init__(self, karma_list):
        super(Member, self).__init__()
        self.__karma_list = karma_list
