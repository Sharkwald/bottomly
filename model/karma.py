from datetime import datetime, timedelta
from pymodm import MongoModel, fields
from enum import Enum

from pymongo import WriteConcern
from config import Config


karma_expiry_days = 30

class KarmaType(Enum):
    POZZYPOZ = 1
    NEGGYNEG = -1

class Karma(MongoModel):

    @staticmethod
    def get_recent_karma_for_recipient(recipient):
        cut_off = datetime.today() - timedelta(days=karma_expiry_days)
        all_time_karma = list(Karma.objects.raw({'awarded_to_username': recipient}))
        return list(filter((lambda k: k.awarded > cut_off), all_time_karma))

    @staticmethod
    def get_current_net_karma_for_recipient(recipient):
        recent_karma = list(map((lambda k: k.karma_type), Karma.get_recent_karma_for_recipient(recipient)))
        positive_karma = len(list(filter((lambda k: k == str(KarmaType.POZZYPOZ)), recent_karma)))
        negative_karma = len(list(filter((lambda k: k == str(KarmaType.NEGGYNEG)), recent_karma)))
        net_karma = positive_karma - negative_karma
        return net_karma

    @staticmethod
    def get_current_karma_reasons_for_recipient(recipient):
        recent_karma = Karma.get_recent_karma_for_recipient(recipient)
        karma_with_reasons = list(filter((lambda k: k.reason != Karma.default_reason), recent_karma))
        karma_without_reasons = list(filter((lambda k: k.reason == Karma.default_reason), recent_karma))
        return {'reasonless': len(karma_without_reasons), 'reasoned': karma_with_reasons}

    awarded_to_username = fields.CharField()
    default_reason = "default reason"
    awarded_by_username = fields.CharField()
    reason = fields.CharField(blank=True)
    awarded = fields.DateTimeField()
    karma_type = fields.CharField()

    class Meta:
        write_concern = WriteConcern(j=True)
        connection_alias = Config.Connection
