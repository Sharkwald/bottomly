# coding=utf-8
import unittest
from unittest.mock import patch

import os

from commands import GetCurrentKarmaReasonsCommand
from config import Config
from model.karma import Karma, KarmaType
from model.member import Member
from slack_channel import GetCurrentKarmaReasonsEventHandler

test_prefix = "_"
valid_event_plain_recipient = {"text": test_prefix + "reasons recipient", "user": "username"}
valid_event_slack_id_recipient = {"text": test_prefix + "reasons <@slack_id>", "user": "username"}
valid_event_slack_id_recipient_trailing_stuff = {"text": test_prefix + "reasons <@slack_id> some extra guff", "user": "username"}
valid_event_no_recipient = {"text": test_prefix + "reasons", "user": "username"}
invalid_event = {'text': '_g test'}
fake_member = Member(username="member_name", slack_id="slack_id")
help_event = {"text": test_prefix + "reasons -?"}
command_result = {"reasonless": 10, "reasoned": list([Karma(reason="reason1", karma_type=KarmaType.POZZYPOZ, awarded_by_username="user1"),
                                                      Karma(reason="reason2", karma_type=KarmaType.POZZYPOZ, awarded_by_username="user2"),
                                                      Karma(reason="reason3", karma_type=KarmaType.NEGGYNEG, awarded_by_username="user3")])}
expected_response = "Recent Karma for recipient:" + os.linesep + \
                    "Recently awarded with no reason: 10." + os.linesep + \
                    '++ from user1 for "reason1"' + os.linesep + \
                    '++ from user2 for "reason2"' + os.linesep + \
                    '-- from user3 for "reason3"'


@patch.object(Config, "connect_to_db")
@patch.object(Config, "get_config_value")
@patch.object(Config, "get_prefix", return_value=test_prefix)
class TestGetCurrentKarmaReasonsEventHandler(unittest.TestCase):

    def test_handles_correct_event_with_plain_recipient(self, prefix_method, config_method, db_method):
        handler = GetCurrentKarmaReasonsEventHandler()
        can_handle = handler.can_handle(valid_event_plain_recipient)
        self.assertTrue(can_handle)

    def test_handles_correct_event_with_slack_id_recipient(self, prefix_method, config_method, db_method):
        handler = GetCurrentKarmaReasonsEventHandler()
        can_handle = handler.can_handle(valid_event_slack_id_recipient)
        self.assertTrue(can_handle)

    def test_handles_correct_event_no_recipient(self, prefix_method, config_method, db_method):
        handler = GetCurrentKarmaReasonsEventHandler()
        can_handle = handler.can_handle(valid_event_no_recipient)
        self.assertTrue(can_handle)

    def test_does_not_handle_different_event(self, prefix_method, config_method, db_method):
        handler = GetCurrentKarmaReasonsEventHandler()
        can_handle = handler.can_handle(invalid_event)
        self.assertFalse(can_handle)

    @patch.object(GetCurrentKarmaReasonsEventHandler, "_send_dm_response")
    @patch.object(GetCurrentKarmaReasonsCommand, "execute", return_value=command_result)
    def test_command_execute_is_called_with_plain_recipient(self, execute_method, response_method, config_method,
                                                            prefix_method, db_method):
        handler = GetCurrentKarmaReasonsEventHandler()
        handler.handle(valid_event_plain_recipient)
        execute_method.assert_called_once_with("recipient")

    @patch.object(GetCurrentKarmaReasonsEventHandler, "_send_dm_response")
    @patch.object(GetCurrentKarmaReasonsCommand, "execute", return_value=command_result)
    def test_command_execute_is_called_with_slack_id_recipient(self, execute_method, response_method, config_method,
                                                            prefix_method, db_method):
        with patch.object(Member, "get_member_by_slack_id", return_value=fake_member):
            handler = GetCurrentKarmaReasonsEventHandler()
            handler.handle(valid_event_slack_id_recipient)
            execute_method.assert_called_once_with(fake_member.username)\

    @patch.object(GetCurrentKarmaReasonsEventHandler, "_send_dm_response")
    @patch.object(GetCurrentKarmaReasonsCommand, "execute", return_value=command_result)
    def test_command_execute_is_called_with_slack_id_and_guff_recipient(self, execute_method, response_method,
                                                                       config_method, prefix_method, db_method):
        with patch.object(Member, "get_member_by_slack_id", return_value=fake_member):
            handler = GetCurrentKarmaReasonsEventHandler()
            handler.handle(valid_event_slack_id_recipient_trailing_stuff)
            execute_method.assert_called_once_with(fake_member.username)

    @patch.object(GetCurrentKarmaReasonsEventHandler, "_send_dm_response")
    @patch.object(GetCurrentKarmaReasonsCommand, "execute", return_value=command_result)
    def test_command_execute_is_called_no_recipient(self, execute_method, response_method, config_method,
                                                    prefix_method, db_method):
        handler = GetCurrentKarmaReasonsEventHandler()
        handler.handle(valid_event_no_recipient)
        execute_method.assert_called_once_with(valid_event_no_recipient["user"])

    @patch.object(GetCurrentKarmaReasonsCommand, "execute", return_value=command_result)
    @patch.object(GetCurrentKarmaReasonsEventHandler, "_send_dm_response")
    def test_command_result_is_correctly_built(self, response_method, execute_method,
                                               config_method, prefix_method, db_method):
        handler = GetCurrentKarmaReasonsEventHandler()
        handler.handle(valid_event_plain_recipient)
        response_method.assert_called_once_with(expected_response, valid_event_plain_recipient)

    @patch.object(GetCurrentKarmaReasonsCommand, "get_purpose", return_value="GetCurrentKarmaReasons")
    @patch.object(GetCurrentKarmaReasonsEventHandler, "_send_message_response")
    def test_get_usage(self, response_method, purpose_method, config_method, prefix_method, db_method):
        handler = GetCurrentKarmaReasonsEventHandler()
        handler.handle(help_event)
        expected_help = "Karma Reasons" + os.linesep + \
                        "GetCurrentKarmaReasons"+ os.linesep +"Usage: `" + \
                        test_prefix + "reasons [recipient <if blank, will default to you>]`"
        response_method.assert_called_once_with(expected_help, help_event)


if __name__ == '__main__':
    unittest.main()