from datetime import datetime, timedelta
from pymodm import MongoModel, fields
from enum import Enum

from pymodm.errors import ValidationError
from pymongo import WriteConcern
from config import Config


karma_expiry_days = 30


class KarmaType(Enum):
    POZZYPOZ = 1
    NEGGYNEG = -1


class Karma(MongoModel):

    @staticmethod
    def _get_net_karma(**kwargs) -> list:
        # aggregate defaults
        projection = {"$project": {"_id": "$_id", "recipient": {"$toLower": "$awarded_to_username"},
                                   "net_karma": {"$cond": [{"$eq": ["$karma_type", str(KarmaType.POZZYPOZ)]}, 1, -1]},
                                   "awarded": "$awarded"}}
        match = {"$match": {'awarded': {'$gt': _get_cut_off_date()}}}
        grouping = {"$group": {"_id": "$recipient", "net_karma": {"$sum": "$net_karma"}}}
        sort = {"$sort": {"net_karma": -1}}
        limit = 3

        # kwarg aggregate overrides
        if "recipient" in kwargs:
            match["$match"]["recipient"] = str(kwargs["recipient"]).lower()
        if "sort" in kwargs and kwargs["sort"] == "asc":
            sort["$sort"]["net_karma"] = 1
        if "limit" in kwargs:
            limit = int(kwargs["limit"])
        if "at" in kwargs:
            parsed_at = kwargs["at"]
            match["$match"]["awarded"] = {'$lte': parsed_at,'$gt': _get_cut_off_date(parsed_at)}

        # execution
        query_set = Karma.objects.aggregate(projection,
                                            match,
                                            grouping,
                                            sort)
        result_set = list(query_set)[:limit]
        return [{"username": r["_id"], "net_karma": r["net_karma"]} for r in result_set]

    @staticmethod
    def get_leader_board(size: int=3) -> list:
        return Karma._get_net_karma(limit=size)

    @staticmethod
    def get_loser_board(size: int=3) -> list:
        return Karma._get_net_karma(limit=size, sort="asc")

    @staticmethod
    def get_current_net_karma_for_recipient(recipient: str) -> int:
        net_karma_results = Karma._get_net_karma(recipient=recipient)
        if len(net_karma_results) == 0:
            return 0
        recipient_net_karma = net_karma_results[0]
        return recipient_net_karma["net_karma"]

    @staticmethod
    def get_historical_net_karma_for_recipient(recipient: str, at: datetime) -> int:
        net_karma_results = Karma._get_net_karma(recipient=recipient, at=at)
        if len(net_karma_results) == 0:
            return 0
        recipient_net_karma = net_karma_results[0]
        return recipient_net_karma["net_karma"]

    @staticmethod
    def get_current_karma_reasons_for_recipient(recipient: str) -> dict:
        projection = {"$project": {"_id": "$_id",
                                   "awarded_to_username": {"$toLower": "$awarded_to_username"},
                                   "awarded_by_username": {"$toLower": "$awarded_by_username"},
                                   "karma_type": "$karma_type",
                                   "awarded": "$awarded",
                                   "reason": "$reason"
                                   }}
        match = {'$match': {'awarded_to_username': recipient.lower(),
                            'awarded': {'$gt': _get_cut_off_date()}}}
        query_set = Karma.objects.aggregate(projection, match)
        recent_karma = [Karma(awarded_to_username=k["awarded_to_username"],
                              awarded_by_username=k["awarded_by_username"],
                              karma_type=k["karma_type"],
                              awarded=k["awarded"],
                              reason=k["reason"],
                              _id=k["_id"]) for k in list(query_set)]
        karma_with_reasons = [k for k in recent_karma if k.reason != Karma.default_reason]
        karma_without_reasons = [k for k in recent_karma if k.reason == Karma.default_reason]
        return {'reasonless': len(karma_without_reasons), 'reasoned': karma_with_reasons}

    awarded_to_username = fields.CharField()
    default_reason = ""
    awarded_by_username = fields.CharField()
    reason = fields.CharField(blank=True)
    awarded = fields.DateTimeField()
    karma_type = fields.CharField()

    def validate_auto_pozzypoz(self):
        valid = True
        if self.karma_type == str(KarmaType.POZZYPOZ):
            valid = self.awarded_by_username != self.awarded_to_username  # can't give yourself positive karma
        if not valid:
            raise ValidationError("Can't give yourself positive karma")

    def clean(self):
        self.validate_auto_pozzypoz()


    class Meta:
        write_concern = WriteConcern(j=True)
        connection_alias = Config.Connection


def _get_cut_off_date(at: datetime=datetime.today()) -> datetime:
    return at - timedelta(days=karma_expiry_days)
