# coding=utf-8
import unittest

from unittest.mock import patch

import os

from commands import AddKarmaCommand
from model.karma import KarmaType
from slack_channel import IncrementKarmaEventHandler

valid_event = {"text": "++ username for being awesome", "user": "kinduser"}
valid_event_no_space = {"text": "++username for being awesome", "user": "kinduser"}

invalid_event = {"text": "this is missing a valid command prefix"}
help_event = {"text": "++ -?"}

class TestIncrementKarmaEventHandler(unittest.TestCase):

    def test_handles_correct_event(self):
        handler = IncrementKarmaEventHandler()
        can_handle = handler.can_handle(valid_event)
        self.assertTrue(can_handle)

    def test_does_not_handle_different_event(self):
        handler = IncrementKarmaEventHandler()
        can_handle = handler.can_handle(invalid_event)
        self.assertFalse(can_handle)

    @patch.object(IncrementKarmaEventHandler, "_send_reaction_response")
    @patch.object(AddKarmaCommand, "execute")
    def test_command_executes_and_response_sent(self, execute_method, response_method):
        handler = IncrementKarmaEventHandler()
        handler.handle(valid_event)
        execute_method.assert_called_once_with(
            awarded_to="username",
            awarded_by=valid_event["user"],
            reason="being awesome",
            karma_type=KarmaType.POZZYPOZ
        )
        response_method.assert_called_once_with(valid_event)

    @patch.object(AddKarmaCommand, "get_purpose", return_value="Karmas")
    @patch.object(IncrementKarmaEventHandler, "_send_message_response")
    def test_get_usage(self, response_method, purpose_method):
        handler = IncrementKarmaEventHandler()
        handler.handle(help_event)
        expected_help = "Pozzy-poz" + os.linesep + \
                        "Karmas" + os.linesep +\
                        "Usage: `++ recipient [[for <if recipient is not a known user>] reason]`"
        purpose_method.assert_called_once_with()
        response_method.assert_called_once_with(expected_help, help_event)

    @patch.object(IncrementKarmaEventHandler, "_send_reaction_response")
    @patch.object(AddKarmaCommand, "execute")
    def test_command_executes_and_response_sent_nospace(self, execute_method, response_method):
        handler = IncrementKarmaEventHandler()
        handler.handle(valid_event_no_space)
        execute_method.assert_called_once_with(
            awarded_to="username",
            awarded_by=valid_event["user"],
            reason="being awesome",
            karma_type=KarmaType.POZZYPOZ
        )
        response_method.assert_called_once_with(valid_event_no_space)

    if __name__ == '__main__':
        unittest.main()