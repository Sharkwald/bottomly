# coding=utf-8
import unittest

from unittest.mock import patch

from commands import GetLeaderBoardCommand
from model.karma import Karma


class TestGetLeaderBoard(unittest.TestCase):
    def test_get_leader_board(self):
        # Arrange
        expected = [{"username": "cool guy", "net_karma": 2},
                    {"username": "guy 1", "net_karma": 1},
                    {"username": "guy 2", "net_karma": 1}]

        with patch.object(Karma, "get_leader_board", return_value=expected) as execution_method:
            # Act
            c = GetLeaderBoardCommand()
            leader_board = c.execute()

            # Assert
            execution_method.assert_called_once()
            self.assertEqual(expected, leader_board)

    def test_get_leader_board_size_specified(self):
        # Arrange
        expected = [{"username": "cool guy", "net_karma": 2},
                    {"username": "guy 1", "net_karma": 1},
                    {"username": "guy 2", "net_karma": 1},
                    {"username": "loser", "net_karma": -1}]

        # Act
        with patch.object(Karma, "get_leader_board", return_value=expected) as execution_method:
            c = GetLeaderBoardCommand()
            leader_board = c.execute(size=4)

            # Assert
            execution_method.assert_called_once_with(size=4)
            self.assertEqual(expected, leader_board)


if __name__ == '__main__':
    unittest.main()