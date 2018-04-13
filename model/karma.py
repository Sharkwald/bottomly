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
    def get_current_net_karma(filter) -> list:
        projection = {"$project": {"_id": "$awarded_to_username",
                                   "karma_value": {
                                       "$cond": [{"$eq": ["$karma_type", str(KarmaType.POZZYPOZ)]}, 1, -1]}}}
        grouping = {"$group": {"_id": "$_id", "total": {"$sum": "$karma_value"}}}
        sort = {"$sort": {"total": -1}}
        query_set = Karma.objects.aggregate(filter, projection, grouping, sort)
        result = map((lambda r: {"username": r["_id"], "net_karma": r["total"]}), list(query_set))
        return list(result)

    @staticmethod
    def get_leader_board():
        cut_off = datetime.today() - timedelta(days=karma_expiry_days)
        filter = {"$match": {'awarded': {'$gt': cut_off}}}
        return Karma.get_current_net_karma(filter)

    @staticmethod
    def get_recent_karma_for_recipient(recipient: str):
        recipient = recipient.lower()
        cut_off = datetime.today() - timedelta(days=karma_expiry_days)
        query_set = Karma.objects.raw({'awarded_to_username': re.compile(recipient, re.IGNORECASE),
                                       'awarded': {'$gt':cut_off}})
        return list(query_set)

    @staticmethod
    def get_current_net_karma_for_recipient(recipient: str):
        cut_off = datetime.today() - timedelta(days=karma_expiry_days)
        filter = {"$match": {'awarded_to_username':re.compile(recipient, re.IGNORECASE),
                             'awarded': {'$gt': cut_off}}}
        current_net_karma =  Karma.get_current_net_karma(filter)
        if len(current_net_karma) == 0:
            return 0
        recipient_net_karma = current_net_karma[0]
        return recipient_net_karma["net_karma"]


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
