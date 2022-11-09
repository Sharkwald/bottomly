# coding=utf-8
import unittest
from unittest.mock import patch

import os

from commands import ReleaseCommand
from config import Config
from slack_channel import ReleaseEventHandler

test_prefix = "_"
valid_event = {"text": test_prefix + "release"}
invalid_event = {"text": "this is missing a valid command prefix"}
help_event = {"text": test_prefix + "release -?"}
release_response = "These are some release notes"


@patch.object(Config, "get_config_value")
@patch.object(Config, "get_prefix", return_value=test_prefix)
class TestReleaseEventHandler(unittest.TestCase):

    def test_handles_correct_event(self, prefix_method, config_method):
        handler = ReleaseEventHandler()
        can_handle = handler.can_handle(valid_event)
        self.assertTrue(can_handle)

    def test_does_not_handle_different_event(self, prefix_method, config_method):
        handler = ReleaseEventHandler()
        can_handle = handler.can_handle(invalid_event)
        self.assertFalse(can_handle)

    @patch.object(ReleaseEventHandler, "_send_message_response")
    @patch.object(ReleaseCommand, "execute", return_value = release_response)
    def test_command_execute_is_called(self, execute_method, response_method, config_method, prefix_method):
        handler = ReleaseEventHandler()
        handler.handle(valid_event)
        execute_method.assert_called_once_with()

    @patch.object(ReleaseCommand, "execute", return_value=release_response)
    @patch.object(ReleaseEventHandler, "_send_message_response")
    def test_command_result_is_correctly_built(self, response_method, execute_method, config_method, prefix_method):
        handler = ReleaseEventHandler()
        handler.handle(valid_event)
        response_method.assert_called_once_with(release_response, valid_event)

    @patch.object(ReleaseCommand, "execute", return_value=None)
    @patch.object(ReleaseEventHandler, "_send_message_response")
    def test_no_result_message_correctly_sent(self, response_method, execute_method, config_method, prefix_method):
        handler = ReleaseEventHandler()
        handler.handle(valid_event)
        empty_results_message = 'Unable to retrieve latest release info.'
        response_method.assert_called_once_with(empty_results_message, valid_event)

    @patch.object(ReleaseCommand, "get_purpose", return_value="Releases")
    @patch.object(ReleaseEventHandler, "_send_message_response")
    def test_get_usage(self, response_method, purpose_method, config_method, prefix_method):
        handler = ReleaseEventHandler()
        handler.handle(help_event)
        expected_help = "Release" + os.linesep + \
                        "Releases" + os.linesep + \
                        "Usage: `_release`"
        purpose_method.assert_called_once_with()
        response_method.assert_called_once_with(expected_help, help_event)

if __name__ == '__main__':
    unittest.main()
