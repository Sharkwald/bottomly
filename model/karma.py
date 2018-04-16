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
    def _get_current_net_karma(**kwargs) -> list:
        # aggregate defaults
        filter = {"$match": {'awarded': {'$gt': _get_cut_off_date()}}}
        projection = {"$project": {"_id": "$awarded_to_username",
                                   "karma_value": {
                                       "$cond": [{"$eq": ["$karma_type", str(KarmaType.POZZYPOZ)]}, 1, -1]}}}
        grouping = {"$group": {"_id": "$_id", "net_karma": {"$sum": "$karma_value"}}}
        sort = {"$sort": {"net_karma": -1, "_id": 1}}
        limit = {"$limit": 3}

        # kwarg aggregate overrides
        if "awarded_to_username" in kwargs:
            filter["$match"]["awarded_to_username"] = re.compile(kwargs["awarded_to_username"], re.IGNORECASE)
        if "sort" in kwargs and kwargs["sort"] == "asc":
            sort["$sort"]["net_karma"] = 1
        if "limit" in kwargs:
            limit["$limit"] = kwargs["limit"]

        # execution
        query_set = Karma.objects.aggregate(filter, projection, grouping, sort, limit)
        result = map((lambda r: {"username": r["_id"], "net_karma": r["net_karma"]}), list(query_set))
        return list(result)

    @staticmethod
    def get_leader_board(size: int=3) -> list:
        return Karma._get_current_net_karma(limit=size)

    @staticmethod
    def get_loser_board(size: int=3) -> list:
        return Karma._get_current_net_karma(sort="asc", limit=size)

    @staticmethod
    def get_current_net_karma_for_recipient(recipient: str) -> int:
        net_karma_results =  Karma._get_current_net_karma(awarded_to_username=recipient)
        if len(net_karma_results) == 0:
            return 0
        recipient_net_karma = net_karma_results[0]
        return recipient_net_karma["net_karma"]

    @staticmethod
    def get_current_karma_reasons_for_recipient(recipient: str) -> dict:
        query_set = Karma.objects.raw({'awarded_to_username': re.compile(recipient, re.IGNORECASE),
                                       'awarded': {'$gt': _get_cut_off_date()}})
        recent_karma = list(query_set)
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


def _get_cut_off_date() -> datetime:
    return datetime.today() - timedelta(days=karma_expiry_days)
