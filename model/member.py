from datetime import datetime, timedelta
from model.karma import KarmaType
from functools import reduce


class Member(object):
    """Represents a member of a chat channel"""

    karma_expiry_days = 30

    def get_current_karma(self):
        """a function doc string"""
        cut_off = datetime.today() - timedelta(days=Member.karma_expiry_days)
        recent_karma = map((lambda k: k.karma_type), filter((lambda k: k.awarded < cut_off), self.__karma_list))
        net_karma = reduce((lambda tally, next_karma: tally + 1 if next_karma == KarmaType.POZZYPOZ else tally - 1),
                           recent_karma, 0)
        return net_karma

    def __init__(self, karma_list):
        super(Member, self).__init__()
        self.__karma_list = karma_list
