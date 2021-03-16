from commands.abstract_command import AbstractCommand
from config import Config
from model.karma import Karma


class GetLoserBoardCommand(AbstractCommand):

    def get_purpose(self):
        return "Gets the current karma loserboard"

    def execute(self, size: int=3):
        reasons = Karma.get_loser_board(size=size)
        return reasons

    def __init__(self):
        super(GetLoserBoardCommand, self)
        config = Config()
        config.connect_to_db()