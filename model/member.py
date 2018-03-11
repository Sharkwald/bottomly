from datetime import datetime, timedelta


class Member(object):
    """Represents a member of a chat channel"""

    karma_expiry_days = 30;

    def get_current_karma(self):
        """a function doc string"""
        cut_off = datetime.today() - timedelta(days = Member.karma_expiry_days);
        recent_karma = filter((lambda k: k.awarded < cut_off), self.__karma_list)
        return list(recent_karma).__len__()

    def __init__(self, karma_list):
        super(Member, self).__init__()
        self.__karma_list = karma_list
