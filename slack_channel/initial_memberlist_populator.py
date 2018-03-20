from slacker import Slacker
from config import Config, ConfigKeys


token = Config().get_config_value(ConfigKeys.slack_bot_token)


class InitialMemberlistPopulator(object):

    def populate(self):
        slack = Slacker(token)
        response = slack.users.list()
        users = response.body['members']
        for user in users:
            if not user['deleted']:
                print(user['id'], user['name'], user['is_admin'], user['is_owner'])

    def __init__(self):
        super(InitialMemberlistPopulator, self)