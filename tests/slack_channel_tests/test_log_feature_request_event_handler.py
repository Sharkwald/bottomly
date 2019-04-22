# coding=utf-8
import unittest
from unittest.mock import patch

import os

from commands import LogFeatureRequestCommand
from config import Config
from slack_channel import LogFeatureRequestEventHandler

test_prefix = "_"
valid_event = {"text": test_prefix + "featureRequest some feature that I'd like",  "user": "someone", "channel": "C1234569"}
invalid_event = {"text": "this is missing a valid command prefix"}
help_event = {"text": test_prefix + "featureRequest -?"}


@patch.object(Config, "get_config_value")
@patch.object(Config, "get_prefix", return_value=test_prefix)
class TestLogFeatureRequestEventHandler(unittest.TestCase):

    def test_handles_correct_event(self, prefix_method, config_method):
        handler = LogFeatureRequestEventHandler()
        can_handle = handler.can_handle(valid_event)
        self.assertTrue(can_handle)

    def test_does_not_handle_different_event(self, prefix_method, config_method):
        handler = LogFeatureRequestEventHandler()
        can_handle = handler.can_handle(invalid_event)
        self.assertFalse(can_handle)

    @patch.object(LogFeatureRequestEventHandler, "_send_reaction_response")
    @patch.object(LogFeatureRequestCommand, "execute")
    def test_command_execute_is_called(self, execute_method, response_method, config_method, prefix_method):
        handler = LogFeatureRequestEventHandler()
        handler.handle(valid_event)
        execute_method.assert_called_once_with(request=valid_event["text"][16:], requester=valid_event["user"])
        response_method.assert_called_once_with(valid_event)

    @patch.object(LogFeatureRequestCommand, "get_purpose", return_value="Logs a request")
    @patch.object(LogFeatureRequestEventHandler, "_send_message_response")
    def test_get_usage(self, response_method, purpose_method, config_method, prefix_method):
        handler = LogFeatureRequestEventHandler()
        handler.handle(help_event)
        expected_help = "Log Feature Request" + os.linesep + \
                        "Logs a request" + os.linesep +\
                        "Usage: `" + test_prefix + "featureRequest <request details>" + "`"
        purpose_method.assert_called_once_with()
        response_method.assert_called_once_with(expected_help, help_event)


if __name__ == '__main__':
    unittest.main()
