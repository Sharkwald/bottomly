# coding=utf-8
import unittest
from unittest.mock import patch

import os

from commands import GetLeaderBoardCommand
from config import Config
from model.member import Member
from slack_channel import GetLeaderBoardEventHandler

test_prefix = "_"
valid_event_no_size = {"text": test_prefix + "leaderboard"}
valid_event_size_10 = {"text": test_prefix + "leaderboard 10"}
valid_event_guff_size= {"text": test_prefix + "leaderboard -1"}
valid_event_size_0= {"text": test_prefix + "leaderboard 0"}
invalid_event = {'text': '_g test'}
fake_member = Member(username="member_name", slack_id="slack_id")
help_event = {"text": test_prefix + "leaderboard -?"}
command_result = list([{"username": "cool guy", "net_karma": 1},
                       {"username": "guy", "net_karma": 0},
                       {"username": "loser", "net_karma": -1}])
expected_response = "cool guy: 1" + os.linesep + \
                    "guy: 0" + os.linesep + \
                    "loser: -1"


@patch.object(Config, "connect_to_db")
@patch.object(Config, "get_config_value")
@patch.object(Config, "get_prefix", return_value=test_prefix)
class TestGetLeaderBoardEventHandler(unittest.TestCase):

    def test_handles_correct_event_no_size(self, prefix_method, config_method, db_method):
        handler = GetLeaderBoardEventHandler()
        can_handle = handler.can_handle(valid_event_no_size)
        self.assertTrue(can_handle)

    def test_handles_correct_event_with_size(self, prefix_method, config_method, db_method):
        handler = GetLeaderBoardEventHandler()
        can_handle = handler.can_handle(valid_event_size_10)
        self.assertTrue(can_handle)

    def test_handles_correct_event_with_guff_size(self, prefix_method, config_method, db_method):
        handler = GetLeaderBoardEventHandler()
        can_handle = handler.can_handle(valid_event_guff_size)
        self.assertTrue(can_handle)

    def test_handles_correct_event_with_size_0(self, prefix_method, config_method, db_method):
        handler = GetLeaderBoardEventHandler()
        can_handle = handler.can_handle(valid_event_size_0)
        self.assertTrue(can_handle)

    def test_does_not_handle_different_event(self, prefix_method, config_method, db_method):
        handler = GetLeaderBoardEventHandler()
        can_handle = handler.can_handle(invalid_event)
        self.assertFalse(can_handle)

    @patch.object(GetLeaderBoardEventHandler, "_send_message_response")
    @patch.object(GetLeaderBoardCommand, "execute", return_value=command_result)
    def test_command_execute_is_called_with_no_size(self, execute_method, response_method, config_method,
                                                    prefix_method, db_method):
        handler = GetLeaderBoardEventHandler()
        handler.handle(valid_event_no_size)
        execute_method.assert_called_once_with(3)

    @patch.object(GetLeaderBoardEventHandler, "_send_message_response")
    @patch.object(GetLeaderBoardCommand, "execute", return_value=command_result)
    def test_command_execute_is_called_with_size(self, execute_method, response_method, config_method,
                                                 prefix_method, db_method):
        handler = GetLeaderBoardEventHandler()
        handler.handle(valid_event_size_10)
        execute_method.assert_called_once_with(10)

    @patch.object(GetLeaderBoardEventHandler, "_send_message_response")
    @patch.object(GetLeaderBoardCommand, "execute", return_value=command_result)
    def test_command_execute_is_called_with_guff_size(self, execute_method, response_method, config_method,
                                                      prefix_method, db_method):
        handler = GetLeaderBoardEventHandler()
        handler.handle(valid_event_guff_size)
        execute_method.assert_called_once_with(3)

    @patch.object(GetLeaderBoardEventHandler, "_send_message_response")
    @patch.object(GetLeaderBoardCommand, "execute", return_value=command_result)
    def test_command_execute_is_called_with_size_0(self, execute_method, response_method, config_method,
                                                   prefix_method, db_method):
        handler = GetLeaderBoardEventHandler()
        handler.handle(valid_event_size_0)
        execute_method.assert_called_once_with(3)

    @patch.object(GetLeaderBoardCommand, "execute", return_value=command_result)
    @patch.object(GetLeaderBoardEventHandler, "_send_message_response")
    def test_command_result_is_correctly_built(self, response_method, execute_method, config_method,
                                               prefix_method, db_method):
        handler = GetLeaderBoardEventHandler()
        handler.handle(valid_event_no_size)
        response_method.assert_called_once_with(expected_response, valid_event_no_size)

    @patch.object(GetLeaderBoardCommand, "get_purpose", return_value="GetLeaderBoard")
    @patch.object(GetLeaderBoardEventHandler, "_send_message_response")
    def test_get_usage(self, response_method, purpose_method, config_method, prefix_method, db_method):
        handler = GetLeaderBoardEventHandler()
        handler.handle(help_event)
        expected_help = "Get Leaderboard" + os.linesep + \
                        "GetLeaderBoard"+ os.linesep +"Usage: `" + \
                        test_prefix + "leaderboard [size of leaderboard. Default is 3]`"
        response_method.assert_called_once_with(expected_help, help_event)


if __name__ == '__main__':
    unittest.main()