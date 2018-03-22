# coding=utf-8
import unittest
from unittest.mock import patch

import os

from commands import GetCurrentNetKarmaCommand
from config import Config
from slack_channel import GetCurrentNetKarmaEventHandler

test_prefix = "_"
valid_event = {"text": test_prefix + "karma recipient", "user": "username"}
valid_event_no_recipient = {"text": test_prefix + "karma", "user": "username"}
invalid_event = {"text": "this is missing a valid command prefix"}
help_event = {"text": test_prefix + "karma -?"}


@patch.object(Config, "connect_to_db")
@patch.object(Config, "get_config_value")
@patch.object(Config, "get_prefix", return_value=test_prefix)
class TestGetCurrentNetKarmaEventHandler(unittest.TestCase):

    def test_handles_correct_event_with_recipient(self, prefix_method, config_method, db_method):
        handler = GetCurrentNetKarmaEventHandler()
        can_handle = handler.can_handle(valid_event)
        self.assertTrue(can_handle)

    def test_handles_correct_event_no_recipient(self, prefix_method, config_method, db_method):
        handler = GetCurrentNetKarmaEventHandler()
        can_handle = handler.can_handle(valid_event_no_recipient)
        self.assertTrue(can_handle)

    def test_does_not_handle_different_event(self, prefix_method, config_method, db_method):
        handler = GetCurrentNetKarmaEventHandler()
        can_handle = handler.can_handle(invalid_event)
        self.assertFalse(can_handle)

    @patch.object(GetCurrentNetKarmaEventHandler, "_send_response")
    @patch.object(GetCurrentNetKarmaCommand, "execute", return_value = 0)
    def test_command_execute_is_called_with_recipient(self, execute_method, response_method, config_method, prefix_method, db_method):
        handler = GetCurrentNetKarmaEventHandler()
        handler.handle(valid_event)
        execute_method.assert_called_once_with("recipient")

    @patch.object(GetCurrentNetKarmaEventHandler, "_send_response")
    @patch.object(GetCurrentNetKarmaCommand, "execute", return_value=0)
    def test_command_execute_is_called_no_recipient(self, execute_method, response_method, config_method, prefix_method, db_method):
        handler = GetCurrentNetKarmaEventHandler()
        handler.handle(valid_event_no_recipient)
        execute_method.assert_called_once_with(valid_event_no_recipient["user"])

    @patch.object(GetCurrentNetKarmaCommand, "execute", return_value=0)
    @patch.object(GetCurrentNetKarmaEventHandler, "_send_response")
    def test_command_result_is_correctly_built(self, response_method, execute_method, config_method, prefix_method, db_method):
        handler = GetCurrentNetKarmaEventHandler()
        handler.handle(valid_event)
        response_method.assert_called_once_with("recipient: 0", valid_event)

    @patch.object(GetCurrentNetKarmaCommand, "get_purpose", return_value="GetCurrentNetKarma")
    @patch.object(GetCurrentNetKarmaEventHandler, "_send_response")
    def test_get_usage(self, response_method, purpose_method, config_method, prefix_method, db_method):
        handler = GetCurrentNetKarmaEventHandler()
        handler.handle(help_event)
        expected_help = "GetCurrentNetKarma"+ os.linesep +"Usage: `" + test_prefix + "karma [recipient <if blank, will default to you>]`"
        response_method.assert_called_once_with(expected_help, help_event)

    if __name__ == '__main__':
        unittest.main()