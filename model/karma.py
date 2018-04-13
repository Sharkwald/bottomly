import re
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
    def get_leader_board():
        cut_off = datetime.today() - timedelta(days=karma_expiry_days)
        query_set = Karma.objects.aggregate({ "$match" : {"karma_type" : "KarmaType.POZZYPOZ",
                                                          'awarded': {'$gt':cut_off}}},
                                            {"$group" : {"_id" : "$awarded_to_username","total" : {"$sum" : 1.0}}})
        result = map((lambda r: {"username": r["_id"], "positive_karma": r["total"]}), list(query_set))
        return list(result)

    @staticmethod
    def get_recent_karma_for_recipient(recipient: str):
        recipient = recipient.lower()
        cut_off = datetime.today() - timedelta(days=karma_expiry_days)
        query_set = Karma.objects.raw({'awarded_to_username': re.compile(recipient, re.IGNORECASE),
                                       'awarded': {'$gt':cut_off}})
        return list(query_set)

    @staticmethod
    def get_current_net_karma_for_recipient(recipient: str):
        recent_karma = list(map((lambda k: k.karma_type), Karma.get_recent_karma_for_recipient(recipient)))
        positive_karma = len(list(filter((lambda k: k == str(KarmaType.POZZYPOZ)), recent_karma)))
        negative_karma = len(list(filter((lambda k: k == str(KarmaType.NEGGYNEG)), recent_karma)))
        net_karma = positive_karma - negative_karma
        return net_karma

    @staticmethod
    def get_current_karma_reasons_for_recipient(recipient: str):
        recent_karma = Karma.get_recent_karma_for_recipient(recipient)
        karma_with_reasons = list(filter((lambda k: k.reason != Karma.default_reason), recent_karma))
        karma_without_reasons = list(filter((lambda k: k.reason == Karma.default_reason), recent_karma))
        return {'reasonless': len(karma_without_reasons), 'reasoned': karma_with_reasons}

    awarded_to_username = fields.CharField()
    default_reason = ""
    awarded_by_username = fields.CharField()
    reason = fields.CharField(blank=True)
    awarded = fields.DateTimeField()
    karma_type = fields.CharField()

    def validate(self) -> bool:
        valid = True
        if self.karma_type == str(KarmaType.POZZYPOZ):
            valid = self.awarded_by_username != self.awarded_to_username  # can't give yourself positive karma
        return valid

    def validate_and_save(self):
        if self.validate():
            self.save()

    class Meta:
        write_concern = WriteConcern(j=True)
        connection_alias = Config.Connection
