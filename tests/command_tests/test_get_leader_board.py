# coding=utf-8
import unittest

from commands.get_leader_board import GetLeaderBoardCommand
from config import Config
from model.karma import Karma
from tests.model_tests.test_karma import TestKarma


class TestAddKarma(unittest.TestCase):

    def setUp(self):
        # Set up
        Config().connect_to_db()
        old_karma = Karma.objects.all()
        for ok in old_karma:
            ok.delete()
        TestKarma.setup_leaderboard()


    def test_get_leader_board(self):
        # Arrange
        expected = [{"username": "cool guy", "net_karma": 2},
                    {"username": "guy 1", "net_karma": 1},
                    {"username": "guy 2", "net_karma": 1}]

        # Act
        c = GetLeaderBoardCommand()
        leader_board = c.execute()

        # Assert
        self.assertEqual(expected, leader_board)

    def test_get_leader_board_size_specified(self):
        # Arrange
        expected = [{"username": "cool guy", "net_karma": 2},
                    {"username": "guy 1", "net_karma": 1},
                    {"username": "guy 2", "net_karma": 1},
                    {"username": "loser", "net_karma": -1}]

        # Act
        c = GetLeaderBoardCommand()
        leader_board = c.execute(size=4)

        # Assert
        self.assertEqual(expected, leader_board)


if __name__ == '__main__':
    unittest.main()