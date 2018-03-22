import os
from slacker import Slacker

from commands.add_member import AddMemberCommand
from config import Config, ConfigKeys


token = Config().get_config_value(ConfigKeys.slack_bot_token)


class InitialMemberlistPopulator(object):

    def populate(self):
        Config().connect_to_db()
        slack = Slacker(token)
        response = slack.users.list()
        users = response.body['members']
        data = ""
        for user in users:
            if not user['deleted']:
                username = user['name']
                slack_id = user['id']
                c = AddMemberCommand(username, slack_id)
                c.execute()
                data += "Added " + username + os.linesep
        return data

    def __init__(self):
        super(InitialMemberlistPopulator, self)