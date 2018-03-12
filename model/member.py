from datetime import datetime, timedelta
from pymodm import MongoModel, fields
from model.karma import Karma, KarmaType


class Member(MongoModel):
    _karma_list = fields.EmbeddedDocumentListField(Karma)
    karma_expiry_days = 30

    def __get_recent_karma(self):
        cut_off = datetime.today() - timedelta(days=Member.karma_expiry_days)
        return list(filter((lambda k: k.awarded > cut_off), self._karma_list))

    def get_current_karma(self):
        recent_karma = list(map((lambda k: k.karma_type), self.__get_recent_karma()))
        positive_karma = len(list(filter((lambda k: k == str(KarmaType.POZZYPOZ)), recent_karma)))
        negative_karma = len(list(filter((lambda k: k == str(KarmaType.NEGGYNEG)), recent_karma)))
        net_karma = positive_karma - negative_karma
        return net_karma

    def add_karma(self, new_karma):
        self._karma_list.append(new_karma)

    def get_karma_reasons(self):
        recent_karma = self.__get_recent_karma()
        karma_with_reasons = list(filter((lambda k: k.reason != Karma.default_reason), recent_karma))
        karma_without_reasons = list(filter((lambda k: k.reason == Karma.default_reason), recent_karma))
        return {'reasonless':len(karma_without_reasons), 'reasoned':karma_with_reasons}

    def __init__(self, karma_list):
       super(Member, self).__init__()
       self._karma_list = karma_list
