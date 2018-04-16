from commands.abstract_command import AbstractCommand
from config import Config
from model.karma import Karma


class GetLeaderBoardCommand(AbstractCommand):

    def get_purpose(self):
        return "Gets the current karma leaderboard"

    def execute(self, size: int=3):
        reasons = Karma.get_leader_board(size=size)
        return reasons

    def __init__(self):
        super(GetLeaderBoardCommand, self)
        config = Config()
        config.connect_to_db()