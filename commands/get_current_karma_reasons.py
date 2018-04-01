from commands.abstract_command import AbstractCommand
from config import Config
from model.karma import Karma


class GetCurrentKarmaReasonsCommand(AbstractCommand):

    def get_purpose(self):
        return "Returns the justifications for someone's/something's current score of imaginary internet points"

    def execute(self, recipient):
        reasons = Karma.get_current_karma_reasons_for_recipient(recipient)
        return reasons

    def __init__(self):
        super(GetCurrentKarmaReasonsCommand, self)
        config = Config()
        config.connect_to_db()