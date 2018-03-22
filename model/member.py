
from pymodm import MongoModel, fields
from pymongo import WriteConcern
from model.karma import Karma
from config import Config


class Member(MongoModel):
    username = fields.CharField(primary_key=True)
    slack_id = fields.CharField()

    @staticmethod
    def get_member_by_username(username):
        members = list(Member.objects.raw({'_id': username}))
        if (len(members) == 0):
            return None
        else:
            return members[0]

    @staticmethod
    def get_member_by_slack_id(slack_id):
        members = list(Member.objects.raw({'slack_id': slack_id}))
        if (len(members) == 0):
            return None
        else:
            return members[0]

    def _get_recent_karma(self):
        return Karma.get_recent_karma_for_recipient(self.username)

    def get_current_karma(self):
        return Karma.get_current_net_karma_for_recipient(self.username)

    def get_karma_reasons(self):
        return Karma.get_current_karma_reasons_for_recipient(self.username)

    class Meta:
        write_concern = WriteConcern(j=True)
        connection_alias = Config.Connection
