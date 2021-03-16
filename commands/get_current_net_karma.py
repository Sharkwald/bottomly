from commands.abstract_command import AbstractCommand
from config import Config
from model.karma import Karma


class GetCurrentNetKarmaCommand(AbstractCommand):

    def get_purpose(self):
        return "Returns someone's/something's current score of imaginary internet points"

    def execute(self, recipient):
        net_karma = Karma.get_current_net_karma_for_recipient(recipient)
        return net_karma

    def __init__(self):
        super(GetCurrentNetKarmaCommand, self)
        config = Config()
        config.connect_to_db()