# coding=utf-8
import logging
import logging.config
import os
from slacker import Slacker

from commands.add_member import AddMemberCommand
from config import Config, ConfigKeys


token = Config().get_config_value(ConfigKeys.slack_bot_token)

logging.config.fileConfig('./logging.conf')
logger = logging.getLogger('bottomly')


class InitialMemberlistPopulator(object):

    @staticmethod
    def populate():
        try:
            print("Beginning population...")
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
        except Exception as ex:
            message = "Error running initial populate"
            logger.exception(message)
            print(message + ": " + ex)

    def __init__(self):
        super(InitialMemberlistPopulator, self)


if __name__ == '__main__':
    InitialMemberlistPopulator().populate()
