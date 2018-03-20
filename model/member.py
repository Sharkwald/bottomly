from datetime import datetime, timedelta
from pymodm import MongoModel, fields
from pymongo import WriteConcern
from model.karma import Karma, KarmaType
from config import Config


class Member(MongoModel):
    username = fields.CharField(primary_key=True)
    karma_expiry_days = 30

    def _get_recent_karma(self):
        cut_off = datetime.today() - timedelta(days=Member.karma_expiry_days)
        all_time_karma = list(Karma.objects.raw({'awarded_to_username': self.username}))
        return list(filter((lambda k: k.awarded > cut_off), all_time_karma))

    def get_current_karma(self):
        recent_karma = list(map((lambda k: k.karma_type), self._get_recent_karma()))
        positive_karma = len(list(filter((lambda k: k == str(KarmaType.POZZYPOZ)), recent_karma)))
        negative_karma = len(list(filter((lambda k: k == str(KarmaType.NEGGYNEG)), recent_karma)))
        net_karma = positive_karma - negative_karma
        return net_karma

    def get_karma_reasons(self):
        recent_karma = self._get_recent_karma()
        karma_with_reasons = list(filter((lambda k: k.reason != Karma.default_reason), recent_karma))
        karma_without_reasons = list(filter((lambda k: k.reason == Karma.default_reason), recent_karma))
        return {'reasonless':len(karma_without_reasons), 'reasoned':karma_with_reasons}

    class Meta:
        write_concern = WriteConcern(j=True)
        connection_alias = Config.Connection
